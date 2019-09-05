using Unity.Entities;

public struct HasOvertakenComponent : IComponentData
{
    public int NewPosition;
    public bool hasToChange;
}