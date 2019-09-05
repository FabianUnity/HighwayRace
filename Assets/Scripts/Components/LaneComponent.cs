using Unity.Entities;

public struct LaneComponent : IComponentData
{
   public int Lane;
}

public struct LaneChangeComponent : IComponentData
{
   public int Direction;
}
