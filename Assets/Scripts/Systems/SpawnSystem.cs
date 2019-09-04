using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] 
    private GameObject carPrefab;
    [SerializeField] 
    private int carAmount;

    private void Start()
    {
        Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(carPrefab, World.Active);
        var entityManager = World.Active.EntityManager;

        for (int i = 0; i < carAmount; i++)
        {
            RenderMesh render = new RenderMesh();
            
            var instance = entityManager.Instantiate(prefab);
            entityManager.SetComponentData(instance, new Translation());
            entityManager.AddComponentData(instance, new PositionComponent { Position = new float2(60, 6) });
            entityManager.AddComponentData(instance, new SpeedComponent {CurrentSpeed = 0, DefaultSpeed = 15, OvertakeSpeed = 20, TargetSpeed = 15});
        }
    }
}