using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementJob : IJobForEach<PositionComponent, SpeedComponent>
    {
        public float DeltaTime; 
            
        public void Execute(ref PositionComponent position, ref SpeedComponent speed)
        {
            //position.Position.y=30;
            position.Position.x = CarBufferElement.LoopPosition(position.Position.x + speed.CurrentSpeed * DeltaTime);
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MovementJob(){DeltaTime = Time.deltaTime};
        return job.Schedule(this, inputDeps);
    }
}
