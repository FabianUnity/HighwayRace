using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FrontSensorSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    struct FrontSensorJob : IJobForEachWithEntity<PositionComponent, SpeedComponent, OvertakerComponent, LaneChangeComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref PositionComponent positionComponent, ref SpeedComponent speedComponent, [ReadOnly]ref OvertakerComponent overtakerComponent, ref LaneChangeComponent laneChangeComponent)
        {
            if(laneChangeComponent.IsWantToOvertake)
                return;
            if (overtakerComponent.DistanceToCarInFront < overtakerComponent.OvertakeDistance)
            {
                var targetSpeed = math.min(speedComponent.DefaultSpeed, overtakerComponent.CarInFrontSpeed);
                speedComponent.TargetSpeed = targetSpeed;
                speedComponent.CurrentSpeed = speedComponent.TargetSpeed;
                overtakerComponent.Blocked = true;
                Debug.Log(overtakerComponent.CarInFrontSpeed / speedComponent.DefaultSpeed);
                if (overtakerComponent.OvertakeEargerness >= overtakerComponent.CarInFrontSpeed / speedComponent.DefaultSpeed)
                    laneChangeComponent.IsWantToOvertake = true;
            }
            else
            {
                speedComponent.TargetSpeed = speedComponent.DefaultSpeed;
                overtakerComponent.Blocked = false;
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
