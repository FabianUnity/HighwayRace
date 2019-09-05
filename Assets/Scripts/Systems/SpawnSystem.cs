using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] 
    private GameObject carPrefab;
    [SerializeField] 
    private int carAmount;
    [SerializeField] 
    private Color[] colors;

    private void Start()
    {
        CreateCars();
    }

    private void CreateCars()
    {
        Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(carPrefab, World.Active);
        var entityManager = World.Active.EntityManager;
        var carElementArray =
            new NativeArray<CarBufferElement>(carAmount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        
        float3 position = float3.zero;
        float distance = 360f / carAmount;
        float minRadius = 115f / 4f;
        float radius = 0;
        float angle = 0;
        int land;

        for (int i = 0; i < carAmount; i++)
        {
            Random.InitState(((int) System.DateTime.Now.Ticks) * (i+1));
            land = Random.Range(0,4);
            radius = minRadius + (land * 1.7f);
            var instance = entityManager.Instantiate(prefab);
            position.x = radius * math.cos(angle);
            position.z = radius * math.sin(angle);
            entityManager.SetComponentData(instance, new Translation{Value = position});
            entityManager.AddComponentData(instance, new PositionComponent { Position = new float2(angle, radius) });
            entityManager.AddComponentData(instance, new SpeedComponent {CurrentSpeed = Random.Range(0.007f,0.009f), DefaultSpeed = 15, OvertakeSpeed = 20, TargetSpeed = 15});
            entityManager.AddComponentData(instance, new CarElementPositionComponent(){Value = i});
            entityManager.AddComponentData(instance, new OvertakerComponent());
            entityManager.AddComponentData(instance, new HasOvertakenComponent(){NewPosition = -1, hasToChange = false});
            
            carElementArray[i] = new CarBufferElement()
            {
                Position = 0,
                Entity = instance,
                NextInLane = -1,
                NextLeft = -1,
                NextRight = -1,
                PrevLane = -1,
                PrevLeft = -1,
                PrevRight = -1
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