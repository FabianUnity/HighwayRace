using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class RaceSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    private EntityQuery _highWayQuery;
    
    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
        
        //initialize command buffer
        _entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    private struct UpdatePositions : IJob
    {
        public EntityCommandBuffer CommandBuffer;
        public NativeArray<CarBufferElement> CarElements;
       
        public void Execute()
        {
            var switched = new NativeArray<bool>(CarElements.Length,Allocator.Temp);
            for (var carIndex = 0; carIndex < CarElements.Length; carIndex++)
            {
                var carElement = CarElements[carIndex];
                var nextCarIndex = (carIndex + 1) % CarElements.Length;
                var nextCarElement = CarElements[nextCarIndex];
                var distance = CarBufferElement.Distance(nextCarElement, carElement);

                //if has overtaken
                if (distance < 0)
                {
                    //switch the cars
                    CarElements[nextCarIndex] = carElement;
                    CarElements[carIndex] = nextCarElement;

                    //mark the entities to update indexes
                    if (switched[carIndex])
                    {
                        CommandBuffer.SetComponent(carElement.Entity,
                            new HasOvertakenComponent() {NewPosition = nextCarIndex});
                    }
                    else
                    {
                        CommandBuffer.AddComponent(carElement.Entity,
                            new HasOvertakenComponent() {NewPosition = nextCarIndex});
                    }

                    if (switched[nextCarIndex])
                    {
                        CommandBuffer.SetComponent(nextCarElement.Entity,
                            new HasOvertakenComponent() {NewPosition = carIndex});
                    }
                    else
                    {
                        CommandBuffer.AddComponent(nextCarElement.Entity,
                            new HasOvertakenComponent() {NewPosition = carIndex});
                    }

                    switched[nextCarIndex] = true;
                    switched[carIndex] = true;
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var bufferLookup = GetBufferFromEntity<CarBufferElement>(false);
        var buffer = bufferLookup[highWayEntity];
                   
        var updateJobHandle = new UpdatePositions()
        {
            CommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer(),
            CarElements = buffer.AsNativeArray()
        }.Schedule(inputDeps);

        //updateJobHandle.Complete();
        _entityCommandBufferSystem.AddJobHandleForProducer(updateJobHandle);
        
        return updateJobHandle;
    }
}
