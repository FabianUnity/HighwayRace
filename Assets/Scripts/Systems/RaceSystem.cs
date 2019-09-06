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
    private struct ExtractCarData : IJobForEach<CarElementPositionComponent,PositionComponent, SpeedComponent, LaneComponent>
    {
        [NativeDisableParallelForRestriction] public NativeArray<CarBufferElement> CarElements;
        
        public void Execute(
            [ReadOnly] ref CarElementPositionComponent carElementPosition,
            [ReadOnly] ref PositionComponent position,
            [ReadOnly] ref SpeedComponent speed,
            [ReadOnly] ref LaneComponent lane)
        {
            var carElement = CarElements[carElementPosition.Value];
            carElement.Position = position.Position.x;
            carElement.Speed = speed.CurrentSpeed;
            carElement.Lane = lane.Lane;
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
            //TODO: Improve this sh*t
            
            var carIndex = carElementPosition.Value;
            var carElement = CarElements[carIndex];

            // Car in front
            var speed = CarBufferElement._2PI;
            var distance = CarBufferElement._2PI;
            for (int i = 1; i < CarElements.Length; i++)
            {
                var nextCar = CarElements[(carIndex + i) % CarElements.Length];
                if (nextCar.Lane == carElement.Lane)
                {
                    distance = CarBufferElement.Distance(carElement, nextCar);
                    speed = nextCar.Speed;
                    break;
                }
            }
            overtaker.DistanceToCarInFront = distance;
            overtaker.CarInFrontSpeed = speed;
            
            // Car in right
            speed = CarBufferElement._2PI;
            distance = CarBufferElement._2PI;
            var prevDistance = CarBufferElement._2PI;
            if (carElement.Lane > 0)
            {
                for (int i = 1; i < CarElements.Length; i++)
                {
                    var nextCar = CarElements[(carIndex + i) % CarElements.Length];
                    var prevCar = CarElements[(CarElements.Length + carIndex - i) % CarElements.Length];
                    if (prevCar.Lane == carElement.Lane - 1 && prevDistance>=CarBufferElement._2PI)
                    {
                        prevDistance = CarBufferElement.Distance(prevCar, carElement);
                    }
                    if (nextCar.Lane == carElement.Lane - 1)
                    {
                        distance = CarBufferElement.Distance(carElement, nextCar);
                        speed = nextCar.Speed;
                        break;
                    }
                }
            }

            overtaker.DistanceToCarInRight = distance;
            overtaker.DistanceToCarInRightBack = prevDistance;
            overtaker.CarInRightSpeed = speed;
            
            // Car in left
            speed = CarBufferElement._2PI;
            distance = CarBufferElement._2PI;
            prevDistance = CarBufferElement._2PI;
            if (carElement.Lane < 3)
            {
                for (int i = 1; i < CarElements.Length; i++)
                {
                    var nextCar = CarElements[(carIndex + i) % CarElements.Length];
                    var prevCar = CarElements[(CarElements.Length + carIndex - i) % CarElements.Length];
                    if (prevCar.Lane == carElement.Lane + 1 && prevDistance>=CarBufferElement._2PI)
                    {
                        prevDistance = CarBufferElement.Distance(prevCar, carElement);
                    }
                    if (nextCar.Lane == carElement.Lane + 1)
                    {
                        distance = CarBufferElement.Distance(carElement, nextCar);
                        speed = nextCar.Speed;
                        break;
                    }
                }
            }

            overtaker.DistanceToCarInLeft = distance;
            overtaker.DistanceToCarInLeftBack = prevDistance;
            overtaker.CarInLeftSpeed = speed;
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

        //var sortBufferJobHandle = bufferArray.SortJob(extractJobHandle);
                   
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
