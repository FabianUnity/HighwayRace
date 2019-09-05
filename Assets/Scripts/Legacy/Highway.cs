using Unity.Mathematics;
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
            GameObject instance = Instantiate(piecePrefab, position, quaternion.RotateY(-angle), transform);
            angle += distance;
            
        }
        
        Camera.main.transform.position = Vector3.up*(radius*2);
    }
}