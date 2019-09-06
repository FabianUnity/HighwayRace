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
    
    struct RightSensorJob : IJobForEachWithEntity<PositionComponent, LaneComponent, OvertakerComponent, SpeedComponent, LaneChangeComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly]ref PositionComponent positionComponent, ref LaneComponent laneComponent, [ReadOnly] ref OvertakerComponent overtakerComponent, [ReadOnly] ref SpeedComponent speedComponent, ref LaneChangeComponent laneChangeComponent)
        {
            if(laneChangeComponent.LastLane != laneComponent.Lane)
                return;
            
            if (laneComponent.Lane > 0 && (overtakerComponent.OvertakeDistance - overtakerComponent.DistanceToCarInRight < -0.5f ||
                                           (overtakerComponent.OvertakeDistance >= overtakerComponent.DistanceToCarInRight &&
                                               overtakerComponent.CarInRightSpeed >= speedComponent.DefaultSpeed)))
            {
                Random random = new Random();
                random.InitState((uint) ((System.DateTime.Now.Ticks) * (entity.Index + 1)));
                if (random.NextFloat(0, 100) >= 90)
                {
                    laneChangeComponent.LastLane = laneComponent.Lane;
                    laneChangeComponent.CurrentTime = 0;
                    laneComponent.Lane -= 1;
                    return;
                }
            }
            
            if (laneChangeComponent.IsWantToOvertake && laneComponent.Lane < 3 && (overtakerComponent.OvertakeDistance - overtakerComponent.DistanceToCarInLeft < -0.5f||
                                           (overtakerComponent.OvertakeDistance >= overtakerComponent.DistanceToCarInLeft &&
                                            overtakerComponent.CarInLeftSpeed >= speedComponent.DefaultSpeed)))
            {
                laneChangeComponent.LastLane = laneComponent.Lane;
                laneChangeComponent.CurrentTime = 0;
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

        return rightJob;
    }
}