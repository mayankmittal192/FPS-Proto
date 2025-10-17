using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

public struct RequestSpawnPlayerRpc : IRpcCommand
{
    public FixedString32Bytes playerName;
    public float3 spawnPoint;
    public int playerHP;
    public int respawnDelay;
}

public struct PlayerName : IComponentData
{
    public FixedString32Bytes Value;
}

public struct ThinClientName : IComponentData
{
    public FixedString32Bytes Value;
}

public struct PlayerSpawnRequestSent : IComponentData
{
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[BurstCompile]
public partial struct SendSpawnPlayerRequestSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        if (!state.WorldUnmanaged.IsThinClient())
            state.RequireForUpdate<PlayerName>();
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<SpawnManager>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
        {
            // Check if already sent
            if (SystemAPI.HasComponent<PlayerSpawnRequestSent>(entity)) continue;

            // Mark this request as sent so that it doesn't keep on sending it every frame
            commandBuffer.AddComponent<PlayerSpawnRequestSent>(entity);

            var isThinClient = state.WorldUnmanaged.IsThinClient();
            FixedString32Bytes username = isThinClient ?
                GameConfig.GetThinClientUsername() :
                SystemAPI.GetSingleton<PlayerName>().Value;
            float3 spawnPoint = GameConfig.GetPlayerRespawnLocation(SystemAPI.Time.ElapsedTime);

            var rpcEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(rpcEntity, new RequestSpawnPlayerRpc
            {
                playerName = username,
                spawnPoint = spawnPoint,
                playerHP = GameConfig.PlayerHP,
                respawnDelay = GameConfig.PlayerRespawnDelay
            });
            commandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ReceiveSpawnPlayerRequestSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<RequestSpawnPlayerRpc>>().WithEntityAccess())
        {
            // Get the NetworkId for the requesting client
            var sourceConnection = request.ValueRO.SourceConnection;
            var networkId = SystemAPI.GetComponent<NetworkId>(sourceConnection);

            // Instantiate the player prefab
            var prefab = SystemAPI.GetSingleton<SpawnManager>().playerGhost;
            var player = commandBuffer.Instantiate(prefab);

            // Set the local transform
            commandBuffer.SetComponent(player, new LocalTransform
            {
                Position = command.ValueRO.spawnPoint,
                Rotation = quaternion.identity,
                Scale = 1f
            });

            // Set other properties like name, health etc.
            commandBuffer.SetComponent(player, new PlayerGhost
            {
                playerName = command.ValueRO.playerName,
                playerHP = command.ValueRO.playerHP,
                respawnDelay = command.ValueRO.respawnDelay
            });

            // Associate the instantiated prefab with the connected client's assigned NetworkId
            commandBuffer.SetComponent(player, new GhostOwner
            {
                NetworkId = networkId.Value
            });

            // Add the player to the linked entity group so it is destroyed automatically on disconnect
            commandBuffer.AppendToBuffer(sourceConnection, new LinkedEntityGroup { Value = player });

            commandBuffer.DestroyEntity(entity);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
