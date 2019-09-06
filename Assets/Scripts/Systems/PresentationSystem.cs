using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class PresentationSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct PresentationJob : IJobForEach<Translation, PositionComponent, SpeedComponent>
    {
        public void Execute( ref Translation translation, [Unity.Collections.ReadOnly] ref PositionComponent positionComponent, [Unity.Collections.ReadOnly] ref SpeedComponent speedComponent)
        {
            float radius = positionComponent.Position.y;
            float angle = -positionComponent.Position.x;
            
            translation.Value = new float3(radius * math.cos(angle), 0, radius * math.sin(angle));
        }
    }

    [ExcludeComponent(typeof(ColorChangeComponent))]
    struct ColorJob : IJobForEachWithEntity<SpeedComponent, ColorComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        public void Execute(Entity entity, int index, [Unity.Collections.ReadOnly] ref SpeedComponent speedComponent, ref ColorComponent colorComponent)
        {
            var speed = speedComponent.CurrentSpeed;
            var maxSpeed = speedComponent.OvertakeSpeed;
            var defaultSpeed = speedComponent.DefaultSpeed;
            var s = 0.1f + 0.9f * (speed-defaultSpeed) / (maxSpeed-defaultSpeed);
            var color = (int) math.clamp(math.floor(s * 10),0,10);
            var previousColor = colorComponent.Value;
            if (color != previousColor)
            {
                CommandBuffer.SetComponent(index,entity,new ColorComponent(){Value = color});
                CommandBuffer.AddComponent(index,entity,new ColorChangeComponent());
            }
        }
    }

    struct ColorChangeJob : IJobForEachWithEntity<ColorChangeComponent, ColorComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref ColorChangeComponent colorChangeComponent,[ReadOnly] ref ColorComponent colorComponent)
        {
            var mesh = Highway.GetRenderMeshes()[colorComponent.Value];
            CommandBuffer.SetSharedComponent(index,entity,new RenderMesh(){mesh = mesh.mesh, material = mesh.material});
            CommandBuffer.RemoveComponent<ColorChangeComponent>(index,entity);
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var colorJob = new ColorJob(){CommandBuffer = commandBuffer}.Schedule(this,inputDeps);
        var changeColorJob = new ColorChangeJob() {CommandBuffer = commandBuffer}.Schedule(this, colorJob);
        
        //entityCommandBufferSystem.AddJobHandleForProducer(colorJob);
        entityCommandBufferSystem.AddJobHandleForProducer(changeColorJob);
        
        var presentationJob = new PresentationJob().Schedule(this, inputDeps);
        
        return JobHandle.CombineDependencies(colorJob,  changeColorJob, presentationJob);
    }
}