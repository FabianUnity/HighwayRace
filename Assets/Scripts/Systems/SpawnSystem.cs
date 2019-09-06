using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] 
    private GameObject carPrefab;
    [SerializeField] 
    private int carAmount;
    [SerializeField] 
    private Highway highway;
    [SerializeField] 
    private Color[] colors;
    [SerializeField] 
    private float landOffset = -1.7f;
    [SerializeField] 
    private float landSize = 1.7f;
    
    private void Start()
    {
        Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(carPrefab, World.Active);
        var entityManager = World.Active.EntityManager;
        var carElementArray =
            new NativeArray<CarBufferElement>(carAmount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        
        
        Unity.Mathematics.Random random = new Unity.Mathematics.Random();
        float3 position = float3.zero;
        float distance = (2*math.PI) / carAmount;
        float minRadius = highway.Radius;
        float radius;
        float angle = 0;
        int lane;

        float4 laneRadius = new float4(minRadius + landOffset, minRadius + landSize + landOffset, minRadius + (2 * landSize) + landOffset, minRadius + (3 * landSize) + landOffset);
        
        for (int i = 0; i < carAmount; i++)
        {
            random.InitState((uint)((System.DateTime.Now.Ticks) * (i+1)));
            lane = random.NextInt(0, 4);
            radius = laneRadius[lane];
            var instance = entityManager.Instantiate(prefab);
            position.x = radius * math.cos(angle);
            position.z = radius * math.sin(angle);
            entityManager.SetComponentData(instance, new Translation{Value = position});
            entityManager.AddComponentData(instance, new PositionComponent { Position = new float2(angle, radius) });
            float speed = random.NextFloat(0.05f, 0.5f);
            entityManager.AddComponentData(instance, new SpeedComponent {CurrentSpeed = speed, DefaultSpeed = speed, OvertakeSpeed = 20, TargetSpeed = 15});
            entityManager.AddComponentData(instance, new CarElementPositionComponent(){Value = i});
            entityManager.AddComponentData(instance, new OvertakerComponent
            {
                CarInFrontSpeed = speed,
                OvertakeDistance = random.NextFloat(0.05f, 0.1f),
                OvertakeEargerness = random.NextFloat(0.7f,2f),
                Blocked = false
            });
            entityManager.AddComponentData(instance, new LaneComponent { Lane = lane });
            entityManager.AddComponentData(instance, new LaneChangeComponent { LastLane = lane, IsWantToOvertake = false });
            entityManager.AddComponentData(instance, new ColorComponent { Value = 1 });
            entityManager.AddComponentData(instance, new OvertakingComponent() { TimeLeft = 0});
            carElementArray[i] = new CarBufferElement()
            {
                Position = 0,
                previousIndex = i,
                newIndex = i
                /*Entity = instance,
                NextInLane = -1,
                NextLeft = -1,
                NextRight = -1,
                PrevLane = -1,
                PrevLeft = -1,
                PrevRight = -1*/
            };
            
            angle += distance;
        }
        
        //create the race entity
        var highwayEntity = entityManager.CreateEntity(typeof(HighWayComponent), typeof(CarBufferElement));
        var raceQuery = entityManager.CreateEntityQuery(typeof(HighWayComponent));
        
        raceQuery.SetSingleton(new HighWayComponent { LaneRadius = laneRadius });

        var buffer = entityManager.GetBuffer<CarBufferElement>(highwayEntity);
        buffer.AddRange(carElementArray);
    }
}