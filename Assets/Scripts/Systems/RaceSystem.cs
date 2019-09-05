using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class RaceSystem : JobComponentSystem
{
    //private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    private EntityQuery _highWayQuery;
    
    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
        
        //initialize command buffer
        //_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    [BurstCompile]
    private struct ExtractCarData : IJobForEach<CarElementPositionComponent,PositionComponent, SpeedComponent>
    {
        [NativeDisableParallelForRestriction] public NativeArray<CarBufferElement> CarElements;
        
        public void Execute(
            [ReadOnly] ref CarElementPositionComponent carElementPosition,
            [ReadOnly] ref PositionComponent position,
            [ReadOnly] ref SpeedComponent speed)
        {
            var carElement = CarElements[carElementPosition.Value];
            carElement.Position = position.Position.x;
            carElement.Speed = speed.CurrentSpeed;
            CarElements[carElementPosition.Value] = carElement;
        }
    }

    [BurstCompile]
    private struct UpdateOrder : IJob
    {
        public NativeArray<CarBufferElement> CarElements;

        public void Execute()
        {
            CarElements.Sort();

            for (var carIndex = 0; carIndex < CarElements.Length; carIndex++)
            {
                var carElement = CarElements[carIndex];
                var previousIndex = carElement.previousIndex;
                carElement.previousIndex = carIndex;
                CarElements[carIndex] = carElement;

                var previousElement = CarElements[previousIndex];
                previousElement.newIndex = carIndex;
                CarElements[previousIndex] = previousElement;
            }
        }
    }
    
    [BurstCompile]
    private struct UpdateOvertakenIndexCars:  IJobForEach<CarElementPositionComponent>
    {
        [ReadOnly] public NativeArray<CarBufferElement> CarElements;

        public void Execute(ref CarElementPositionComponent carElementPosition)
        {
            var currentIndex = carElementPosition.Value;
            var carElement = CarElements[currentIndex];
            
            carElementPosition.Value = carElement.newIndex;
        }
    }
    
    [BurstCompile]
    private struct UpdateOvertaker : IJobForEach<OvertakerComponent, CarElementPositionComponent>
    {
        [ReadOnly] public NativeArray<CarBufferElement> CarElements;
        
        public void Execute(ref OvertakerComponent overtaker, [ReadOnly] ref CarElementPositionComponent carElementPosition)
        {
            var carIndex = carElementPosition.Value;
            var carElement = CarElements[carIndex];

            //distance to car in
            for (int i = 0; i < CarElements.Length; i++)
            {
                
            }
            var nextCarIndex = (carIndex + 1) % CarElements.Length;
            var nextCarElement = CarElements[nextCarIndex];
            var distance = CarBufferElement.Distance(carElement, nextCarElement);
            overtaker.DistanceToCarInFront = distance;
            overtaker.CarInFrontSpeed = nextCarElement.Speed;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var bufferLookup = GetBufferFromEntity<CarBufferElement>(false);
        var buffer = bufferLookup[highWayEntity];
        var bufferArray = buffer.AsNativeArray();
        
        var extractJobHandle = new ExtractCarData()
        {
            CarElements = bufferArray
        }.Schedule(this,inputDeps);
                   
        var orderJobHandle = new UpdateOrder()
        {
            CarElements = bufferArray
        }.Schedule(extractJobHandle);
        
        var updateOvertakenJobHandle = new UpdateOvertakenIndexCars()
        {
            CarElements = bufferArray
        }.Schedule(this, orderJobHandle);
        
        var updateDistancesJobHandle = new UpdateOvertaker()
        {
            CarElements = bufferArray
        }.Schedule(this, updateOvertakenJobHandle);
        
        
        
        //_entityCommandBufferSystem.AddJobHandleForProducer(updateJobHandle);

        //return orderJobHandle;
        return updateDistancesJobHandle;
    }
}
