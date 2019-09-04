using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class OvertakingSystem : JobComponentSystem
{
    
    private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;

    protected override void OnCreate()
    {
        _entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    private struct UpdateOvertakenCars:  IJobForEachWithEntity<HasOvertakenComponent, CarElementPositionComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref HasOvertakenComponent hasOvertaken, ref CarElementPositionComponent carElementPosition)
        {
            carElementPosition.Value = hasOvertaken.NewPosition;
            CommandBuffer.RemoveComponent(index, entity,typeof(HasOvertakenComponent));
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var updateJobHandle = new UpdateOvertakenCars()
        {
            CommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);
        
        _entityCommandBufferSystem.AddJobHandleForProducer(updateJobHandle);
        return updateJobHandle;
    }
}