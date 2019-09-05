﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class FrontSensorSystem : JobComponentSystem
{
    struct FronSensorJob : IJobForEach<PositionComponent, SpeedComponent, OvertakerComponent>
    {
        public void Execute([ReadOnly] ref PositionComponent positionComponent, ref SpeedComponent speedComponent, [ReadOnly]ref OvertakerComponent overtakerComponent)
        {
            Debug.Log("Distance: " + overtakerComponent.DistanceToCarInFront + " OvertakeDistance: " + overtakerComponent.OvertakeDistance);
            if (overtakerComponent.DistanceToCarInFront <= overtakerComponent.OvertakeDistance)
            {
                speedComponent.TargetSpeed = overtakerComponent.CarInFrontSpeed;
                speedComponent.CurrentSpeed = overtakerComponent.CarInFrontSpeed;
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
        var job = new FronSensorJob();
        return job.Schedule(this,inputDeps);
    }
}