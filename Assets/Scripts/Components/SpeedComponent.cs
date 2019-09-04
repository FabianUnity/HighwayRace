using Unity.Entities;

public struct SpeedComponent : IComponentData
{
    public float CurrentSpeed;
    public float TargetSpeed;
    public float DefaultSpeed;
    public float OvertakeSpeed;
}