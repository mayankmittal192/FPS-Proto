using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class CustomBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 0;    // Prevent auto connect
        return false;
    }
}
