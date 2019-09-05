using System;
using Unity.Entities;
using Unity.Mathematics;

public struct CarBufferElement : IBufferElementData, IComparable<CarBufferElement>
{
    public const float _2PI = 2 * math.PI;
    
    public float Position;
    public float Speed;
    /*public int Lane;
    public int NextInLane;
    public int NextRight;
    public int NextLeft;
    public int PrevRight;
    public int PrevLeft;
    public int PrevLane;*/
    public bool Dirty;
    public int newIndex;
    public int previousIndex;
    //public Entity Entity;

    public static float Distance(CarBufferElement A, CarBufferElement B)
    {
        var delta = LoopPosition(B.Position - A.Position);
        /*if (delta > math.PI)
        {
            delta -= _2PI;
        }*/

        return delta;
    }

    public static float LoopPosition(float position)
    {
        return math.clamp(position - math.floor(position / _2PI) * _2PI, 0f, _2PI);
    }

    public int CompareTo(CarBufferElement other)
    {
        var distance = other.Position - this.Position;
        return  distance < 0 ? 1 : -1;
    }
}
