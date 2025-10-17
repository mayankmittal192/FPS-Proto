using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ProjectileFireSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (player, input, transform) in
            SystemAPI.Query<RefRW<PlayerGhost>, RefRO<PlayerGhostInput>, RefRO<LocalTransform>>().WithNone<IsDeadComponent>())
        {
            bool currentFire = input.ValueRO.fire;
            bool previousFire = player.ValueRO.prevFireState;

            // Fire only on the rising edge (button just pressed)
            if (currentFire && !previousFire)
            {
                var prefab = SystemAPI.GetSingleton<SpawnManager>().projectileGhost;
                var projectile = commandBuffer.Instantiate(prefab);

                float3 up = transform.ValueRO.Up();
                float3 forward = transform.ValueRO.Forward();
                
                var pitch = math.radians(-input.ValueRO.pitch);   // camera pitch inversion correction
                var muzzleDirection = up * math.sin(pitch) + forward * math.cos(pitch);
                var muzzlePos = transform.ValueRO.Position + up * 0.6f + muzzleDirection * 1.5f;

                commandBuffer.SetComponent(projectile, new LocalTransform
                {
                    Position = muzzlePos,
                    Rotation = quaternion.identity,
                    Scale = 0.1f
                });

                commandBuffer.SetComponent(projectile, new ProjectileGhost
                {
                    velocity = muzzleDirection * GameConfig.ProjectileSpeed,
                    gravity = GameConfig.ProjectileGravity,
                    timeRemaining = GameConfig.ProjectileLifetime
                });
            }

            // Store current fire for next frame comparison
            player.ValueRW.prevFireState = currentFire;
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
