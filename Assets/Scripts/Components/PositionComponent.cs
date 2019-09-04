using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PositionComponent : IComponentData
{
    public float2 Position; /* x = angle, y = radius */
}