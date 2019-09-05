using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class PresentationSystem : JobComponentSystem
{
    [BurstCompile]
    struct PresentationJob : IJobForEach<Translation, PositionComponent, SpeedComponent>
    {
        public void Execute( ref Translation translation, [ReadOnly]ref PositionComponent positionComponent, [ReadOnly]ref SpeedComponent speedComponent)
        {
            float radius = positionComponent.Position.y;
            float angle = positionComponent.Position.x;
            
            translation.Value = new float3(radius * math.cos(angle), 0, radius * math.sin(angle));
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PresentationJob();
        return job.Schedule(this, inputDeps);
    }
}