using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ProjectileGhost : IComponentData
{
    public float3 velocity;         // Movement velocity
    public float3 gravity;          // Gravity impact
    public float timeRemaining;     // Remaining time
}

[DisallowMultipleComponent]
public class ProjectileGhostAuthoring : MonoBehaviour
{
    class Baker : Baker<ProjectileGhostAuthoring>
    {
        public override void Bake(ProjectileGhostAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ProjectileGhost>(entity);
        }
    }
}
