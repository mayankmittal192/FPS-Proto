using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct CameraFollowSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var camera = Camera.main;

        foreach (var (input, trans) in SystemAPI.Query<RefRO<PlayerGhostInput>, RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            // Find position
            var camPos = trans.ValueRO.Position + new float3(0f, 0.6f, 0f); // eye height

            // Convert yaw & pitch into rotation
            var camRot = math.mul(
                quaternion.RotateY(math.radians(input.ValueRO.yaw)),
                quaternion.RotateX(math.radians(input.ValueRO.pitch)));

            camera.transform.SetPositionAndRotation(camPos, camRot);
        }
    }
}
