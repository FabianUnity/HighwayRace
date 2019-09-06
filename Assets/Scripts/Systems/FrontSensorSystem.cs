using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class FrontSensorSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    [ExcludeComponent(typeof(WantToOvertakeTag))]
    struct FrontSensorJob : IJobForEachWithEntity<PositionComponent, SpeedComponent, OvertakerComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref PositionComponent positionComponent, ref SpeedComponent speedComponent, [ReadOnly]ref OvertakerComponent overtakerComponent)
        {
            if (overtakerComponent.DistanceToCarInFront < overtakerComponent.OvertakeDistance)
            {
                speedComponent.TargetSpeed = overtakerComponent.CarInFrontSpeed;
                speedComponent.CurrentSpeed = overtakerComponent.CarInFrontSpeed;
                if (overtakerComponent.OvertakeEargerness > overtakerComponent.CarInFrontSpeed/speedComponent.DefaultSpeed)
                    CommandBuffer.AddComponent(index, entity, new WantToOvertakeTag());
            }
            else
            {
                speedComponent.TargetSpeed = speedComponent.DefaultSpeed;
                speedComponent.CurrentSpeed = speedComponent.DefaultSpeed;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new FrontSensorJob
        {
            CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(this, inputDeps);

        entityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
