using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class FrontSensorSystem : JobComponentSystem
{
    [BurstCompile]
    struct FronSensorJob : IJobForEach<PositionComponent, SpeedComponent, OvertakerComponent>
    {
        public void Execute([ReadOnly] ref PositionComponent positionComponent, ref SpeedComponent speedComponent, ref OvertakerComponent overtakerComponent)
        {
            if (overtakerComponent.DistanceToCarInFront < overtakerComponent.OvertakeDistance)
            {
                speedComponent.TargetSpeed = overtakerComponent.CarInFrontSpeed;
                speedComponent.CurrentSpeed = overtakerComponent.CarInFrontSpeed;
                overtakerComponent.Blocked = true;
            }
            else
            {
                speedComponent.TargetSpeed = speedComponent.DefaultSpeed;
                speedComponent.CurrentSpeed = speedComponent.DefaultSpeed;
                overtakerComponent.Blocked = false;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new FronSensorJob();
        return job.Schedule(this,inputDeps);
    }
}
