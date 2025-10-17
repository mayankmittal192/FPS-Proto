using Unity.Entities;
using UnityEngine;

public struct SpawnManager : IComponentData
{
    public Entity playerGhost;
    public Entity projectileGhost;
    public Entity targetBoxGhost;
}

[DisallowMultipleComponent]
public class SpawnManagerAuthoring : MonoBehaviour
{
    public GameObject playerGhostPrefab;
    public GameObject projectileGhostPrefab;
    public GameObject targetBoxGhostPrefab;

    class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            SpawnManager component = default;
            component.playerGhost = GetEntity(authoring.playerGhostPrefab, TransformUsageFlags.Dynamic);
            component.projectileGhost = GetEntity(authoring.projectileGhostPrefab, TransformUsageFlags.Dynamic);
            component.targetBoxGhost = GetEntity(authoring.targetBoxGhostPrefab, TransformUsageFlags.Dynamic);
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, component);
        }
    }
}
