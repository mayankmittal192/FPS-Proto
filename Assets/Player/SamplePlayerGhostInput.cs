using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct SamplePlayerGhostInput : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<SpawnManager>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerGhostInput>>().WithAll<GhostOwnerIsLocal>())
        {
            float2 lookValue = InputSystemManager.Look;
            float2 moveValue = InputSystemManager.Move;
            bool fireValue = InputSystemManager.Fire;

            // Convert look input into yaw and pitch values
            // Sync client calculated yaw and pitch because look(mouse delta per frame) input is stateless (transient)
            playerInput.ValueRW.yaw += lookValue.x;
            playerInput.ValueRW.pitch = math.clamp(playerInput.ValueRW.pitch - lookValue.y, -80f, 80f);

            // Sync move & fire input as it is with server as button press inputs are stateful
            playerInput.ValueRW.moveValue = moveValue;
            playerInput.ValueRW.fire = fireValue;
        }
    }
}
