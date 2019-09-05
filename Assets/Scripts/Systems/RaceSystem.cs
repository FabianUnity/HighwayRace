using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class RaceSystem : JobComponentSystem
{
    //private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    private EntityQuery _highWayQuery;
    
    protected override void OnCreate()
    {
        _highWayQuery = GetEntityQuery(typeof(HighWayComponent));
        
        //initialize command buffer
        //_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    private struct UpdatePositions : IJob
    {
        //public EntityCommandBuffer CommandBuffer;
        public NativeArray<CarBufferElement> CarElements;
       
        public void Execute()
        {
            //var switched = new NativeArray<bool>(CarElements.Length,Allocator.Temp);
            for (var carIndex = 0; carIndex < CarElements.Length; carIndex++)
            {
                var carElement = CarElements[carIndex];
                var nextCarIndex = (carIndex + 1) % CarElements.Length;
                var nextCarElement = CarElements[nextCarIndex];
                var distance = CarBufferElement.Distance(nextCarElement, carElement);

                //if has overtaken
                if (distance < 0)
                {
                    carElement.Dirty = true;
                    carElement.NewIndex = carIndex;
                    nextCarElement.Dirty = true;
                    nextCarElement.NewIndex = nextCarIndex;
                    
                    //switch the cars
                    CarElements[nextCarIndex] = carElement;
                    CarElements[carIndex] = nextCarElement;
                    
                    

                    //mark the entities to update indexes
                    /*if (switched[carIndex])
                    {*/
                        //CommandBuffer.SetComponent(carElement.Entity,
                            //new HasOvertakenComponent() {NewPosition = nextCarIndex, hasToChange = true});
                    /*}
                    else
                    {
                        CommandBuffer.AddComponent(carElement.Entity,
                            new HasOvertakenComponent() {NewPosition = nextCarIndex});
                    }

                    if (switched[nextCarIndex])
                    {*/
                    //Unity.Entities.World.Active.EntityManager.SetComponentData(nextCarElement.Entity,new HasOvertakenComponent() {NewPosition = carIndex, hasToChange = true});

                        /*CommandBuffer.SetComponent(nextCarElement.Entity,
                            new HasOvertakenComponent() {NewPosition = carIndex, hasToChange = true});
                    /*}
                    else
                    {
                        CommandBuffer.AddComponent(nextCarElement.Entity,
                            new HasOvertakenComponent() {NewPosition = carIndex});
                    }*/

                    /*switched[nextCarIndex] = true;
                    switched[carIndex] = true;*/
                }
            }
        }
    }
    
    private struct ExtractCarPositions : IJobForEach<CarElementPositionComponent,PositionComponent>
    {
        [NativeDisableParallelForRestriction] public NativeArray<CarBufferElement> CarElements;
        
        public void Execute([ReadOnly] ref CarElementPositionComponent carElementPosition, [ReadOnly] ref PositionComponent position)
        {
            var carElement = CarElements[carElementPosition.Value];
            carElement.Position = position.Position.x;
            CarElements[carElementPosition.Value] = carElement;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var highWayEntity = _highWayQuery.GetSingletonEntity();
        var bufferLookup = GetBufferFromEntity<CarBufferElement>(false);
        var buffer = bufferLookup[highWayEntity];
        var bufferArray = buffer.AsNativeArray();
        
        var extractJobHandle = new ExtractCarPositions()
        {
            CarElements = bufferArray
        }.Schedule(this,inputDeps);
                   
        var updateJobHandle = new UpdatePositions()
        {
            //CommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer(),
            CarElements = bufferArray
        }.Schedule(extractJobHandle);
        

        //updateJobHandle.Complete();
        //_entityCommandBufferSystem.AddJobHandleForProducer(updateJobHandle);

        return updateJobHandle;
    }
}
