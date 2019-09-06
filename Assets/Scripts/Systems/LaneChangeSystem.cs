using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LaneChangeSystem : JobComponentSystem
{
    private const float DURATION = 0.3f;
    
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
    private EntityQuery _highWayQuery;
    
    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    struct ChangeLaneJob : IJobForEachWithEntity<PositionComponent, LaneComponent, LaneChangeComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float4 LaneRadius;
        public float DeltaTime;

        public void Execute(Entity entity, int index, ref PositionComponent positionComponent,[ReadOnly] ref LaneComponent laneComponent, ref LaneChangeComponent laneChangeComponent)
        {
            if(laneComponent.Lane == laneChangeComponent.LastLane)
                return;
            
            laneChangeComponent.CurrentTime += DeltaTime;

            if (laneChangeComponent.CurrentTime >= DURATION)
            {
                positionComponent.Position.y = LaneRadius[laneComponent.Lane];
                if (laneComponent.Lane > laneChangeComponent.LastLane)
                    laneChangeComponent.IsWantToOvertake = false;
                laneChangeComponent.LastLane = laneComponent.Lane;
                return;
            }
            
            positionComponent.Position.y = math.lerp(LaneRadius[laneChangeComponent.LastLane], LaneRadius[laneComponent.Lane], laneChangeComponent.CurrentTime/DURATION);
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var highWayComponent = GetComponentDataFromEntity<HighWayComponent>(true)[highWayEntity];
        
        var job = new ChangeLaneJob
        {
            CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            LaneRadius = highWayComponent.LaneRadius,
            DeltaTime = Time.deltaTime
        }.Schedule(this, inputDeps);
        
        entityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}