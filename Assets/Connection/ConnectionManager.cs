using System.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [Tooltip("Change this ip address to the one where the server instance will actually run")]
    [SerializeField] private string listenIP = "127.0.0.1";
    [SerializeField] private string connectIP = "192.168.1.70";
    [SerializeField] private ushort port = 7979;

    public static World serverWorld = null, clientWorld;
    private SubScene[] subScenes;

    public void InitConnection()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Create server & client worlds
        if (Application.isEditor)
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("Server");
            clientWorld = ClientServerBootstrap.CreateClientWorld("Client");
        }
        else
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("Client");
        }

        // Delete the default world
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (serverWorld != null) World.DefaultGameObjectInjectionWorld = serverWorld;
        if (clientWorld != null) World.DefaultGameObjectInjectionWorld = clientWorld;

        Connect();
    }

    private void Connect()
    {
        subScenes = FindObjectsByType<SubScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Setup manual connection - server starts listening to client's requests
        if (serverWorld != null)
        {
            StartCoroutine(LoadSubScenes(serverWorld));

            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.Parse(listenIP, port));
        }

        if (clientWorld != null)
        {
            StartCoroutine(LoadSubScenes(clientWorld));

            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.Parse(connectIP, port));
        }
    }

    private IEnumerator LoadSubScenes(World world)
    {
        while (!world.IsCreated) yield return null;

        if (subScenes != null)
        {
            for (int i = 0; i < subScenes.Length; i++)
            {
                SceneSystem.LoadParameters loadParameters = new() { Flags = SceneLoadFlags.BlockOnStreamIn };
                var sceneEntity = SceneSystem.LoadSceneAsync(world.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                while (!SceneSystem.IsSceneLoaded(world.Unmanaged, sceneEntity))
                {
                    world.Update();
                }
            }
        }
    }
}
