using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SideSensorSystem : JobComponentSystem
{
    public const float BACK_DISTANCE = 0.3f;
    public const float SIDE_DISTANCE = 0.2f;
    
    
    struct RightSensorJob : IJobForEach<PositionComponent, LaneComponent, OvertakerComponent, SpeedComponent, LaneChangeComponent, OvertakingComponent>
    {
        public void Execute([ReadOnly]ref PositionComponent positionComponent, ref LaneComponent laneComponent, ref OvertakerComponent overtakerComponent, [ReadOnly] ref SpeedComponent speedComponent, ref LaneChangeComponent laneChangeComponent, ref OvertakingComponent overtakingComponent)
        {
            if(laneChangeComponent.LastLane != laneComponent.Lane && overtakingComponent.TimeLeft > 0)
                return;

            bool eargerness = overtakerComponent.OvertakeEargerness <=
                               overtakerComponent.CarInRightSpeed / speedComponent.DefaultSpeed;
            
            if (laneComponent.Lane > 0 && 
                (SIDE_DISTANCE < overtakerComponent.DistanceToCarInRight && eargerness) &&
                overtakerComponent.DistanceToCarInRightBack >= BACK_DISTANCE)
            {
                laneChangeComponent.CurrentTime = 0;
                laneChangeComponent.LastLane = laneComponent.Lane;
                laneComponent.Lane -= 1;
                return;
            }

            if (laneChangeComponent.IsWantToOvertake && laneComponent.Lane < 3 && 
                (SIDE_DISTANCE < overtakerComponent.DistanceToCarInLeft || overtakerComponent.CarInLeftSpeed >= speedComponent.OvertakeSpeed)&&
                overtakerComponent.DistanceToCarInLeftBack >= BACK_DISTANCE)
            {
                laneChangeComponent.CurrentTime = 0;
                laneChangeComponent.LastLane = laneComponent.Lane;
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