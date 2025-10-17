using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent]
public struct PlayerGhost : IComponentData
{
    [GhostField] public FixedString32Bytes playerName;
    [GhostField] public int playerHP;
    [GhostField] public bool isDead;
    public int respawnDelay;
    public bool prevFireState;
}

public struct PlayerGhostInput : IInputComponentData
{
    public float yaw;
    public float pitch;
    public float2 moveValue;
    public bool fire;
}

[DisallowMultipleComponent]
public class PlayerGhostAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerGhostAuthoring>
    {
        public override void Bake(PlayerGhostAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerGhost>(entity);
            AddComponent<PlayerGhostInput>(entity);
        }
    }
}
