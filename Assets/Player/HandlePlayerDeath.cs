using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct IsDeadComponent : IComponentData
{
}

public struct PlayerRespawnTimer : IComponentData
{
    public Entity playerEntity;
    public FixedString32Bytes playerName;
    public float timeLeft;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerDeathSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (player, trans, entity) in
            SystemAPI.Query<RefRO<PlayerGhost>, RefRW<LocalTransform>>().WithNone<IsDeadComponent>().WithEntityAccess())
        {
            // Death detection
            if (player.ValueRO.playerHP <= 0)
            {
                // Prepare for respawn
                var respawnEntity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(respawnEntity, new PlayerRespawnTimer
                {
                    playerEntity = entity,
                    playerName = player.ValueRO.playerName,
                    timeLeft = player.ValueRO.respawnDelay
                });

                // Despawn player entity & mark it as Dead
                trans.ValueRW.Scale = 0f;
                commandBuffer.AddComponent<IsDeadComponent>(entity);
                commandBuffer.SetComponent(entity, new PlayerGhost
                {
                    playerName = player.ValueRO.playerName,
                    isDead = true
                });
                UnityEngine.Debug.Log($"Player dead: {player.ValueRO.playerName}");
            }
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerRespawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (timer, entity) in SystemAPI.Query<RefRW<PlayerRespawnTimer>>().WithEntityAccess())
        {
            timer.ValueRW.timeLeft -= dt;

            if (timer.ValueRW.timeLeft <= 0f)
            {
                // Respawn player and remove the Dead tag
                commandBuffer.RemoveComponent<IsDeadComponent>(timer.ValueRO.playerEntity);

                float3 respawnPoint = GameConfig.GetPlayerRespawnLocation(SystemAPI.Time.ElapsedTime);

                // Reset the local transform
                commandBuffer.SetComponent(timer.ValueRO.playerEntity, new LocalTransform
                {
                    Position = respawnPoint,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                // Reset the initial properties
                commandBuffer.SetComponent(timer.ValueRO.playerEntity, new PlayerGhost
                {
                    playerName = timer.ValueRO.playerName,
                    playerHP = GameConfig.PlayerHP,
                    respawnDelay = GameConfig.PlayerRespawnDelay
                });

                // Reset the input
                commandBuffer.SetComponent(timer.ValueRO.playerEntity, new PlayerGhostInput { });

                commandBuffer.DestroyEntity(entity);
            }
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
