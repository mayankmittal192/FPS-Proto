using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct TargetBoxMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var rng = new Random((uint)(SystemAPI.Time.ElapsedTime * 1000));

        foreach (var (target, transform) in SystemAPI.Query<RefRW<TargetBoxGhost>, RefRW<LocalTransform>>())
        {
            var t = target.ValueRW;
            var tr = transform.ValueRW;

            // Random walker logic
            t.velocity += new float3(rng.NextFloat(-10f, 10f), 0, rng.NextFloat(-10f, 10f)) * dt;
            t.velocity = math.clamp(t.velocity, new float3(-3f, 0, -3f), new float3(3f, 0, 3f));

            tr.Position += t.velocity * dt;

            // Bounce if hitting boundary/walls
            if (math.abs(tr.Position.x) > 499.5f) t.velocity.x *= -1f;
            if (math.abs(tr.Position.z) > 499.5f) t.velocity.z *= -1f;

            target.ValueRW = t;
            transform.ValueRW = tr;
        }
    }
}
