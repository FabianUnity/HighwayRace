using Unity.Entities;
using Unity.Mathematics;

public struct OvertakerComponent : IComponentData
{
    public float DistanceToCarInFront;
    public float DistanceToCarInRight;
    public float DistanceToCarInLeft;
    public float DistanceToCarInRightBack;
    public float DistanceToCarInLeftBack;
    public float OvertakeDistance;
    public float CarInFrontSpeed;
    public float CarInRightSpeed;
    public float OvertakeEargerness;
    public float CarInLeftSpeed;
    public bool Blocked;

    public static bool WantsToOvertake(float speed, float carInFrontSpeed, float overtakeDistance, float distance, float eagerness)
    {
        if (distance > overtakeDistance) return false;
        var s = carInFrontSpeed / speed;
        return s < eagerness;
    }
}
