using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FrontSensorSystem : JobComponentSystem
{

    [BurstCompile]
    struct FrontSensorJob : IJobForEach<PositionComponent, SpeedComponent, OvertakerComponent, LaneChangeComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly] ref PositionComponent positionComponent, ref SpeedComponent speedComponent, ref OvertakerComponent overtakerComponent, ref LaneChangeComponent laneChangeComponent)
        {
            if (overtakerComponent.DistanceToCarInFront < overtakerComponent.OvertakeDistance)
            {
                var targetSpeed = math.min(speedComponent.DefaultSpeed, overtakerComponent.CarInFrontSpeed);
                speedComponent.TargetSpeed = targetSpeed;
                overtakerComponent.Blocked = true;
                if (overtakerComponent.OvertakeEargerness > overtakerComponent.CarInFrontSpeed / speedComponent.DefaultSpeed)
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
        var job = new FrontSensorJob().Schedule(this, inputDeps);
        return job;
    }
}
