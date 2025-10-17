using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ProjectileHitDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        // Simple logic based hit detection on server. Idea behind doing logic based hit detection
        // is to optimize as Physics Raycast and custom collision detections are very expensive

        // First we will detect projectile hit with players
        foreach (var (proj, projTrans, projEntity) in
            SystemAPI.Query<RefRO<ProjectileGhost>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            foreach (var(player, playerTrans) in
                SystemAPI.Query<RefRW<PlayerGhost>, RefRO<LocalTransform>>().WithNone<IsDeadComponent>())
            {
                // ---Broad phase hit detection---
                if (projTrans.ValueRO.Position.y > 2f)
                {
                    // Since all players have height of 2, therefore hit is impossible from above that height
                    continue;
                }

                // ---Narrow phase hit detection---
                float3 projectileProjectionOnPlane = new(projTrans.ValueRO.Position.x, projTrans.ValueRO.Position.z, 0.05f);
                float3 playerProjectionOnPlane = new(playerTrans.ValueRO.Position.x, playerTrans.ValueRO.Position.z, 0.5f);

                // Hit if projections overlap
                // Note: Basically a capsule can be approximated as a cylinder, that's why we are doing projection overlapping check
                if (CheckOverlap(projectileProjectionOnPlane, playerProjectionOnPlane))
                {
                    // Destroy projectile entity upon hit
                    commandBuffer.DestroyEntity(projEntity);

                    // Decrease player health by hit damage
                    player.ValueRW.playerHP--;
                }
            }
        }

        // Next we will detect projectile hit with target boxes
        foreach (var (proj, projTrans, projEntity) in
            SystemAPI.Query<RefRO<ProjectileGhost>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            foreach (var (target, targetTrans, targetEntity) in
                SystemAPI.Query<RefRW<TargetBoxGhost>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                // ---Broad phase hit detection---
                if (projTrans.ValueRO.Position.y > 1.05f)
                {
                    // Since all target have height of 1, therefore hit is impossible from above that height
                    // here extra 0.05 is added to take projectile's radius into account
                    continue;
                }

                // ---Narrow phase hit detection---
                var projPosX = projTrans.ValueRO.Position.x;
                var projPosZ = projTrans.ValueRO.Position.z;
                var targetPosX = targetTrans.ValueRO.Position.x;
                var targetPosZ = targetTrans.ValueRO.Position.z;

                // Simple distance check on both (right and forward) axes
                if (math.abs(projPosX - targetPosX) < 0.55f && math.abs(projPosZ - targetPosZ) < 0.55f)
                {
                    // Destroy projectile entity upon hit
                    commandBuffer.DestroyEntity(projEntity);

                    // Decrease target health by hit damage
                    target.ValueRW.health--;

                    // Destroy target entity upon death
                    if (target.ValueRW.health == 0)
                        commandBuffer.DestroyEntity(targetEntity);
                }
            }
        }

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }

    private bool CheckOverlap(float3 projectionA, float3 projectionB)
    {
        float2 projAPoint = new(projectionA.x, projectionA.y);
        float2 projBPoint = new(projectionB.x, projectionB.y);
        float radiiSum = projectionA.z + projectionB.z;
        return math.distancesq(projAPoint, projBPoint) < (radiiSum * radiiSum);
    }
}
