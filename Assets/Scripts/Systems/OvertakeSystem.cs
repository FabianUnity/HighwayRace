using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class OvertakeSystem : JobComponentSystem
{

    [BurstCompile]
    public struct OvertakingJob : IJobForEach<OvertakingComponent, SpeedComponent, OvertakerComponent>
    {
        public float DeltaTime;
        
        public void Execute(ref OvertakingComponent overtakingComponent, ref SpeedComponent speed, [ReadOnly] ref OvertakerComponent overtaker)
        {
            if (overtakingComponent.TimeLeft <= 0)
                overtakingComponent.TimeLeft = 0;
            else
                overtakingComponent.TimeLeft -= DeltaTime;

            if (overtaker.Blocked)
            {
                overtakingComponent.TimeLeft = 0;
            }
            
            if (overtakingComponent.TimeLeft <= 0)
            {
                speed.CurrentSpeed = speed.TargetSpeed;
            }
            else
            {
                speed.CurrentSpeed = speed.OvertakeSpeed;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new OvertakingJob(){DeltaTime = Time.deltaTime}.Schedule(this,inputDeps);
    }
}