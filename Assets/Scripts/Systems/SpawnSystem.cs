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

        for (int i = 0; i < carAmount; i++)
        {
            var instance = entityManager.Instantiate(prefab);
            entityManager.SetComponentData(instance, new Translation());
            entityManager.AddComponentData(instance, new PositionComponent { Position = new float2(60 + i*5, (115f/4f)) });
            entityManager.AddComponentData(instance, new SpeedComponent {CurrentSpeed = Random.Range(0.007f,0.07f), DefaultSpeed = 15, OvertakeSpeed = 20, TargetSpeed = 15});
        }
    }
}