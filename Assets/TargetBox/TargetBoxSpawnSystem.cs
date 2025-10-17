using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct TargetBoxSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();
        var prefab = spawnManager.targetBoxGhost;
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        var rng = new Random(12345);
        
        for (int i = 0; i < GameConfig.MaxTargetBoxes; i++)
        {
            var targetBox = commandBuffer.Instantiate(prefab);
            float3 pos = new(rng.NextFloat(-499.5f, 499.5f), 0.5f, rng.NextFloat(-499.5f, 499.5f));
            float3 vel = new(rng.NextFloat(-2f, 2f), 0f, rng.NextFloat(-2f, 2f));

            commandBuffer.SetComponent(targetBox, new LocalTransform
            {
                Position = pos,
                Rotation = quaternion.identity,
                Scale = 1f
            });
            commandBuffer.SetComponent(targetBox, new TargetBoxGhost
            {
                velocity = vel,
                health = 3
            });
        }

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        // Disable system after first spawn
        state.Enabled = false;
    }
}
