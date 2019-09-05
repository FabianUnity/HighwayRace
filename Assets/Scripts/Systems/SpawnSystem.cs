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
        float radius = 0;
        float angle = 0;
        int land;

        for (int i = 0; i < carAmount; i++)
        {
            random.InitState((uint)((System.DateTime.Now.Ticks) * (i+1)));
            land = random.NextInt(0, 4);
            radius = minRadius + (land * landSize) + landOffset;
            var instance = entityManager.Instantiate(prefab);
            position.x = radius * math.cos(angle);
            position.z = radius * math.sin(angle);
            entityManager.SetComponentData(instance, new Translation{Value = position});
            entityManager.AddComponentData(instance, new PositionComponent { Position = new float2(angle, radius) });
            float speed = random.NextFloat(0.05f, 1f);
            entityManager.AddComponentData(instance, new SpeedComponent {CurrentSpeed = speed, DefaultSpeed = speed, OvertakeSpeed = 20, TargetSpeed = 15});
            entityManager.AddComponentData(instance, new CarElementPositionComponent(){Value = i});
            entityManager.AddComponentData(instance, new OvertakerComponent{ CarInFrontSpeed = 0.05f, OvertakeDistance = random.NextFloat(0.05f, 0.1f) });

            carElementArray[i] = new CarBufferElement()
            {
                Position = 0,
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
        raceQuery.SetSingleton(new HighWayComponent());

        var buffer = entityManager.GetBuffer<CarBufferElement>(highwayEntity);
        buffer.AddRange(carElementArray);
    }
}