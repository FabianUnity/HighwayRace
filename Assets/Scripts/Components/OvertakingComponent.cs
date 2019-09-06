using Unity.Entities;

namespace Components
{
    public struct OvertakingComponent : IComponentData
    {
        public float TimeLeft;
        // posA + speedA*t = posB + speedB*t
        // posA - posB = t*(speedB-speedA)
        // t = (posA - posB) / (speedB - speedA) 
    }
}