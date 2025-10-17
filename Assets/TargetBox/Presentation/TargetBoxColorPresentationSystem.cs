using Unity.Entities;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct TargetBoxColorPresentationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (target, colorProp) in SystemAPI.Query<RefRO<TargetBoxGhost>, RefRW<URPMaterialPropertyBaseColor>>())
        {
            // Map health to color -> Red if health = 1 or Yellow if health = 2 or Green if health = 3
            if (target.ValueRO.health == 1) colorProp.ValueRW.Value = new float4(1f, 0f, 0f, 1f);       // Red
            else if (target.ValueRO.health == 2) colorProp.ValueRW.Value = new float4(1f, 1f, 0f, 1f);  // Yellow
            else if (target.ValueRO.health == 3) colorProp.ValueRW.Value = new float4(0f, 1f, 0f, 1f);  // Green
        }
    }
}
