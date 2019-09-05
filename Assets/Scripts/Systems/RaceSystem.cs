using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class RaceSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    private EntityQuery _highWayQuery;
    
    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
        
        //initialize command buffer
        _entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    [BurstCompile]
    private struct ExtractCarPositions : IJobForEach<CarElementPositionComponent,PositionComponent>
    {
        [NativeDisableParallelForRestriction] public NativeArray<CarBufferElement> CarElements;
        
        public void Execute([ReadOnly] ref CarElementPositionComponent carElementPosition, [ReadOnly] ref PositionComponent position)
        {
            var carElement = CarElements[carElementPosition.Value];
            carElement.Position = position.Position.x;
            CarElements[carElementPosition.Value] = carElement;
        }
    }

    [BurstCompile]
    private struct UpdatePositions : IJob
    {
        //public EntityCommandBuffer CommandBuffer;
        public NativeArray<CarBufferElement> CarElements;
       
        public void Execute()
        {
            //var switched = new NativeArray<bool>(CarElements.Length,Allocator.Temp);
            for (var carIndex = 0; carIndex < CarElements.Length; carIndex++)
            {
                var carElement = CarElements[carIndex];
                var nextCarIndex = (carIndex + 1) % CarElements.Length;
                var referenceCarIndex = (carIndex + 2) % CarElements.Length;
                var nextCarElement = CarElements[nextCarIndex];
                var referenceCarElement = CarElements[referenceCarIndex];
                var distanceToReference = CarBufferElement.Distance(carElement, referenceCarElement);
                var distanceFromNextToReference = CarBufferElement.Distance(nextCarElement, referenceCarElement);

                //if has overtaken
                if (distanceToReference < distanceFromNextToReference)
                {
                    carElement.Dirty = true;
                    carElement.NewIndex = carIndex;
                    nextCarElement.Dirty = true;
                    nextCarElement.NewIndex = nextCarIndex;
                    
                    //switch the cars
                    CarElements[nextCarIndex] = carElement;
                    CarElements[carIndex] = nextCarElement;
                }
            }
        }
    }
    
    [BurstCompile]
    private struct UpdateOvertakerDistances : IJobForEach<OvertakerComponent, CarElementPositionComponent>
    {
        [ReadOnly] public NativeArray<CarBufferElement> CarElements;
        
        public void Execute(ref OvertakerComponent overtaker, [ReadOnly] ref CarElementPositionComponent carElementPosition)
        {
            var carIndex = carElementPosition.Value;
            var nextCarIndex = (carIndex + 1) % CarElements.Length;
            var carElement = CarElements[carIndex];
            var distance = CarBufferElement.Distance(carElement, CarElements[nextCarIndex]);
            overtaker.DistanceToCarInFront = distance;
        }
    }
    
    [BurstCompile]
    private struct UpdateOvertakenIndexCars:  IJobForEach<CarElementPositionComponent>
    {
        [NativeDisableParallelForRestriction] public NativeArray<CarBufferElement> CarElements;

        public void Execute(ref CarElementPositionComponent carElementPosition)
        {
            var carElement = CarElements[carElementPosition.Value];
            
            if (!carElement.Dirty) return;
            
            carElement.Dirty = false;
            CarElements[carElementPosition.Value] = carElement;
            
            carElementPosition.Value = carElement.NewIndex;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var bufferLookup = GetBufferFromEntity<CarBufferElement>(false);
        var buffer = bufferLookup[highWayEntity];
        var bufferArray = buffer.AsNativeArray();
        
        var extractJobHandle = new ExtractCarPositions()
        {
            CarElements = bufferArray
        }.Schedule(this,inputDeps);
                   
        var updatePositionsJobHandle = new UpdatePositions()
        {
            CarElements = bufferArray
        }.Schedule(extractJobHandle);
        
        var updateDistancesJobHandle = new UpdateOvertakerDistances()
        {
            CarElements = bufferArray
        }.Schedule(this, updatePositionsJobHandle);
        
        var updateOvertakenJobHandle = new UpdateOvertakenIndexCars()
        {
            CarElements = bufferArray
        }.Schedule(this, updateDistancesJobHandle);
        
        //_entityCommandBufferSystem.AddJobHandleForProducer(updateJobHandle);

        return updateOvertakenJobHandle;
    }
}
