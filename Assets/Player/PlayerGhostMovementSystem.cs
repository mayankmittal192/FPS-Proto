using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerGhostMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        foreach (var (input, trans) in SystemAPI.Query<RefRO<PlayerGhostInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            // Apply yaw to player body
            trans.ValueRW.Rotation = quaternion.RotateY(math.radians(input.ValueRO.yaw));

            // Apply movement to player body
            var moveInput = input.ValueRO.moveValue * dt;
            trans.ValueRW.Position += trans.ValueRW.Right() * moveInput.x + trans.ValueRW.Forward() * moveInput.y;

            // Stop if reached boundary/walls
            if (math.abs(trans.ValueRO.Position.x) > 499.5f)
            {
                var pos = trans.ValueRO.Position;
                var posXSign = math.sign(pos.x);
                pos.x = 499.5f * posXSign;
                trans.ValueRW.Position = pos;
            }
            if (math.abs(trans.ValueRO.Position.z) > 499.5f)
            {
                var pos = trans.ValueRO.Position;
                var posZSign = math.sign(pos.z);
                pos.z = 499.5f * posZSign;
                trans.ValueRW.Position = pos;
            }
        }
    }
}
