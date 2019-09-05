using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class RightSensorSystem : JobComponentSystem
{
    
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    [ExcludeComponent(typeof(LaneChangeComponent))]
    struct RightSensorJob : IJobForEachWithEntity<PositionComponent, LaneComponent, OvertakerComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly]ref PositionComponent positionComponent, ref LaneComponent laneComponent, [ReadOnly] ref OvertakerComponent overtakerComponent)
        {
            Random random = new Random();
            random.InitState((uint)((System.DateTime.Now.Ticks) * (index+1)));
            if (overtakerComponent.CanTurnRight && random.NextFloat(0, 100) > 80 )
            {
                CommandBuffer.AddComponent(index, entity, new LaneChangeComponent { LastLane = laneComponent.Lane, CurrentTime = 0});
                laneComponent.Lane -= 1; 
            }
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new RightSensorJob
        {
            CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(this, inputDeps);
        
        entityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}