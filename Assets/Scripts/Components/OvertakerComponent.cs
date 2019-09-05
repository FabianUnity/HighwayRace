using Unity.Entities;
using Unity.Mathematics;

public struct OvertakerComponent : IComponentData
{
    public float DistanceToCarInFront;
    public float OvertakeDistance;
    public float CarInFrontSpeed;
    public float CarInRightSpeed;
    public bool CanTurnRight;
    public bool CanTurnLeft;
}
