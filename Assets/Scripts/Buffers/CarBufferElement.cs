using Unity.Entities;
using Unity.Mathematics;

public struct CarBufferElement : IBufferElementData
{
    public const float _2PI = 2 * math.PI;
    
    public float Position;
    public int Lane;
    public int NextInLane;
    public int NextRight;
    public int NextLeft;
    public int PrevRight;
    public int PrevLeft;
    public int PrevLane;
    public Entity Entity;

    public static float Distance(CarBufferElement A, CarBufferElement B)
    {
        var delta = LoopPosition(B.Position - A.Position);
        if (delta > math.PI)
        {
            delta -= _2PI;
        }

        return delta;
    }

    public static float LoopPosition(float position)
    {
        return math.clamp(position - math.floor(position / _2PI) * _2PI, 0f, _2PI);
    }
}
