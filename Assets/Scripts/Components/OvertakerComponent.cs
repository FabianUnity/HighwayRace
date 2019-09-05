using Unity.Entities;

public struct OvertakerComponent : IComponentData
{
    public float DistanceToCarInFront;
    public float OvertakeDistance;
    public float CarInFrontSpeed;
}
