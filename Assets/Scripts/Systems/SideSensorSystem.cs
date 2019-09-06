using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;

public class SideSensorSystem : JobComponentSystem
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
            if (overtakerComponent.CanTurnRight && random.NextFloat(0, 100) >= 90 )
            {
                CommandBuffer.AddComponent(index, entity, new LaneChangeComponent { LastLane = laneComponent.Lane, CurrentTime = 0});
                laneComponent.Lane -= 1; 
            }
        }
    }

    [ExcludeComponent(typeof(LaneChangeComponent))]
    [RequireComponentTag(typeof(WantToOvertakeTag))]
    struct LeftSensorJob : IJobForEachWithEntity<PositionComponent, LaneComponent, OvertakerComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref PositionComponent positionComponent, ref LaneComponent laneComponent, [ReadOnly] ref OvertakerComponent overtakerComponent)
        {
            if (overtakerComponent.CanTurnLeft)
            {
                CommandBuffer.AddComponent(index, entity, new LaneChangeComponent {LastLane = laneComponent.Lane, CurrentTime = 0});
                laneComponent.Lane += 1;
            }
        }
    }
    

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.Concurrent commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(); 
        
        var rightJob = new RightSensorJob
        {
            CommandBuffer = commandBuffer
        }.Schedule(this, inputDeps);
        
        entityCommandBufferSystem.AddJobHandleForProducer(rightJob);

        var leftJob = new LeftSensorJob
        {
            CommandBuffer = commandBuffer
        }.Schedule(this, rightJob);
        
        entityCommandBufferSystem.AddJobHandleForProducer(leftJob);

        return leftJob;
    }
}