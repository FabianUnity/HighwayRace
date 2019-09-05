using Unity.Entities;

public struct LaneChangeComponent : IComponentData
{
    public int LastLane;
    public float CurrentTime;
}