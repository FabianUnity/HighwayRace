using Unity.Entities;

public struct LaneChangeComponent : IComponentData
{
    public float LastRadius;
    public float CurrentTime;
    public bool IsWantToOvertake;
}