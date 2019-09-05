using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Highway : MonoBehaviour
{
    [SerializeField] 
    private float radius = 70;
    [SerializeField] 
    private GameObject piecePrefab;
    [SerializeField] 
    private float pieceSize;
    public float Radius
    {
        get { return radius; }
    }

    public void Start()
    {
        Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(piecePrefab, World.Active);
        var entityManager = World.Active.EntityManager;
        float length = 2 * math.PI * radius;
        int piecesAmount = (int) (length / pieceSize);
        float distance = (2*math.PI)/piecesAmount;
        float angle = 0;
        float3 position = float3.zero;
        quaternion rotation = quaternion.identity;

        for (int i = 0; i < piecesAmount; i++)
        {
            position.x = radius * math.cos(angle);
            position.z = radius * math.sin(angle);
            var instance = entityManager.Instantiate(prefab);
            entityManager.SetComponentData(instance, new Translation { Value = position});
            entityManager.SetComponentData(instance, new Rotation { Value = quaternion.RotateY(-angle)});
            angle += distance;
            
        }
        Camera.main.transform.position = Vector3.up*(radius*2);
    }
}