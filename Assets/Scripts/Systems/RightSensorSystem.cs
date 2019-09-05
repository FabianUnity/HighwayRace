using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class RightSensorSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    [ExcludeComponent(typeof(LaneChangeComponent))]
    struct RightSensorJob : IJobForEachWithEntity<PositionComponent, LaneComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        public void Execute(Entity entity, int index, [ReadOnly]ref PositionComponent positionComponent, ref LaneComponent laneComponent)
        {
                
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new RightSensorJob
        {
            CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);
        
        entityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
