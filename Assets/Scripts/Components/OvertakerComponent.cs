using Unity.Entities;
using Unity.Mathematics;

public struct OvertakerComponent : IComponentData
{
    public float DistanceToCarInFront;
    public float DistanceToCarInRight;
    public float DistanceToCarInLeft;
    public float OvertakeDistance;
    public float CarInFrontSpeed;
    public float CarInRightSpeed;
    public float OvertakeEargerness;
    public float CarInLeftSpeed;
    public bool Blocked;
}
