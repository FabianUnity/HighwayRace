using Unity.Entities;
using Unity.Jobs;

public class MovementSystem : JobComponentSystem
{
    
    struct MovementJob : IJobForEach<PositionComponent, SpeedComponent>
    {
        public void Execute(ref PositionComponent position, ref SpeedComponent speed)
        {
            //position.Position.y=30;
            position.Position.x = CarBufferElement.LoopPosition(position.Position.x + speed.CurrentSpeed);
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MovementJob();
        return job.Schedule(this, inputDeps);
    }
}
