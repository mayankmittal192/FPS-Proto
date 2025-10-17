using Unity.Collections;
using Unity.Mathematics;

public static class GameConfig
{
    public static readonly int PlayerHP = 3;
    public static readonly int PlayerRespawnDelay = 5;

    public static readonly int ProjectileSpeed = 30;
    public static readonly float3 ProjectileGravity = new(0f, -9.81f, 0f);
    public static readonly int ProjectileLifetime = 5;

    public static readonly int MaxTargetBoxes = 100;

    public static FixedString32Bytes GetThinClientUsername()
    {
        return new("ThinClient");
    }

    public static float3 GetPlayerRespawnLocation(double elapsedTime)
    {
        uint seed = (uint)(elapsedTime * 1000.0);
        Random rng = new(seed);
        float posX = rng.NextFloat(-10f, 10f);
        float posZ = rng.NextFloat(-10f, 10f);
        return new(posX, 1f, posZ);
    }
}
