using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

public class OvertakeSystem : JobComponentSystem
{
    
    //private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    private EntityQuery _highWayQuery;


    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
        //_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    [BurstCompile]
    private struct UpdateOvertakenCars:  IJobForEachWithEntity<HasOvertakenComponent, CarElementPositionComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [NativeDisableParallelForRestriction] [NativeDisableContainerSafetyRestriction] public NativeArray<CarBufferElement> CarElements;

        public void Execute(Entity entity, int index, [ReadOnly] ref HasOvertakenComponent hasOvertaken, ref CarElementPositionComponent carElementPosition)
        {
            var carElement = CarElements[carElementPosition.Value];
            
            if (!carElement.Dirty) return;
            
            carElementPosition.Value = carElement.NewIndex;
            carElement.Dirty = false;

            //CommandBuffer.RemoveComponent(index, entity,typeof(HasOvertakenComponent));
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
            overtaker.DistanceToCarInFront =
                CarBufferElement.Distance(CarElements[nextCarIndex], CarElements[carIndex]);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var bufferLookup = GetBufferFromEntity<CarBufferElement>(false);
        var buffer = bufferLookup[highWayEntity];
        var bufferArray = buffer.AsNativeArray();
        
        var updateJobHandle = new UpdateOvertakenCars()
        {
            CarElements = bufferArray
            //CommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);
        
        //update Overtaker Component
        var updateDistancesJob = new UpdateOvertakerDistances()
        {
            CarElements = bufferArray
        }.Schedule(this, updateJobHandle);
        
        //_entityCommandBufferSystem.AddJobHandleForProducer(updateJobHandle);
        return updateDistancesJob;
    }
}