using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SideSensorSystem : JobComponentSystem
{
    struct RightSensorJob : IJobForEach<PositionComponent, LaneComponent, OvertakerComponent, SpeedComponent, LaneChangeComponent, OvertakingComponent>
    {
        public void Execute([ReadOnly]ref PositionComponent positionComponent, ref LaneComponent laneComponent, ref OvertakerComponent overtakerComponent, [ReadOnly] ref SpeedComponent speedComponent, ref LaneChangeComponent laneChangeComponent, ref OvertakingComponent overtakingComponent)
        {
            if(laneChangeComponent.LastLane != laneComponent.Lane && overtakingComponent.TimeLeft > 0)
                return;
            
            if (laneComponent.Lane > 0 && 
                (0.1f < overtakerComponent.DistanceToCarInRight || overtakerComponent.CarInRightSpeed >= speedComponent.DefaultSpeed) &&
                overtakerComponent.DistanceToCarInRightBack > 0.3f)
            {
                laneChangeComponent.LastLane = laneComponent.Lane;
                laneChangeComponent.CurrentTime = 0;
                laneComponent.Lane -= 1;
                return;
            }

            if (laneChangeComponent.IsWantToOvertake && laneComponent.Lane < 3 && 
                (0.1f < overtakerComponent.DistanceToCarInLeft || overtakerComponent.CarInLeftSpeed >= speedComponent.OvertakeSpeed)&&
                overtakerComponent.DistanceToCarInLeftBack > 0.3f)
            {
                laneChangeComponent.LastLane = laneComponent.Lane;
                laneChangeComponent.CurrentTime = 0;
                laneComponent.Lane += 1;
                
                //set time to overtake

                overtakerComponent.Blocked = false;
                overtakingComponent.TimeLeft = math.min(2,(overtakerComponent.DistanceToCarInFront) / (overtakerComponent.CarInFrontSpeed - speedComponent.OvertakeSpeed) + 1);

            }
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new RightSensorJob().Schedule(this, inputDeps);
        return job;
    }
}