using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent]
public struct TargetBoxGhost : IComponentData
{
    public float3 velocity;
    [GhostField] public int health;
}

[DisallowMultipleComponent]
public class TargetBoxGhostAuthoring : MonoBehaviour
{
    class Baker : Baker<TargetBoxGhostAuthoring>
    {
        public override void Bake(TargetBoxGhostAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TargetBoxGhost>(entity);
            AddComponent<URPMaterialPropertyBaseColor>(entity);
        }
    }
}
