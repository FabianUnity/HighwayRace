using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LaneChangeSystem : JobComponentSystem
{
    private const float DURATION = 1f;
    
    private EntityQuery _highWayQuery;
    
    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
    }
    
    [BurstCompile]
    struct ChangeLaneJob : IJobForEach<PositionComponent, LaneComponent, LaneChangeComponent>
    {
        public float4 LaneRadius;
        public float DeltaTime;

        public void Execute(ref PositionComponent positionComponent,[ReadOnly] ref LaneComponent laneComponent, ref LaneChangeComponent laneChangeComponent)
        {
            if(laneComponent.Lane == laneChangeComponent.LastRadius)
                return;
            
            laneChangeComponent.CurrentTime += DeltaTime;

            if (laneChangeComponent.CurrentTime >= DURATION)
            {
                positionComponent.Position.y = LaneRadius[laneComponent.Lane];
                if (laneComponent.Lane > laneChangeComponent.LastRadius)
                {
                    laneChangeComponent.IsWantToOvertake = false;
                }
                laneChangeComponent.LastRadius = laneComponent.Lane;
                return;
            }
            
            positionComponent.Position.y = math.lerp(laneChangeComponent.LastRadius, LaneRadius[laneComponent.Lane], laneChangeComponent.CurrentTime/DURATION);
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var highWayComponent = GetComponentDataFromEntity<HighWayComponent>(true)[highWayEntity];
        
        var job = new ChangeLaneJob
        {
            LaneRadius = highWayComponent.LaneRadius,
            DeltaTime = Time.deltaTime
        }.Schedule(this, inputDeps);

        return job;
    }
}