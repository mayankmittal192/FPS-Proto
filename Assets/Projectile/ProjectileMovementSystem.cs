using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct ProjectileMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (projectile, trans, entity) in
            SystemAPI.Query<RefRW<ProjectileGhost>, RefRW<LocalTransform>>().WithAll<Simulate>().WithEntityAccess())
        {
            // Apply movement to projectile
            projectile.ValueRW.velocity += projectile.ValueRO.gravity * dt;
            trans.ValueRW.Position += projectile.ValueRO.velocity * dt;
            projectile.ValueRW.timeRemaining -= dt;

            // Despawn on server in case of timeout/belowGound
            if (state.WorldUnmanaged.IsServer() && (projectile.ValueRW.timeRemaining <= 0 || trans.ValueRW.Position.y < -0.5f))
                commandBuffer.DestroyEntity(entity);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
