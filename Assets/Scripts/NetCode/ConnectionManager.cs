using System.Collections;
using System.Net;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] string _listenIP = "127.0.0.1";
    [SerializeField] string _connectIP = "127.0.0.1";
    [SerializeField] ushort _port = 7979;
     

    public static World serverWorld = null;
    public static World clientWorld = null;
    public enum Role
    {
        ServerClient = 0, Server = 1, Client = 2
    }

    private static Role _role = Role.ServerClient;

    private void Start()
    {
        if(Application.isEditor)//determine the role of application
        {
            _role = Role.ServerClient;
        }
        else if(Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer)
        {
            _role = Role.Server;
        }
        else
        {
            _role = Role.Client;
        }
        //StartCoroutine(Connect());
    }

    public void InitalizeConnection()
    {
        StartCoroutine(Connect());
    }

    private IEnumerator Connect()
    {
        if(_role == Role.ServerClient || _role == Role.Server)//create the worlds
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        }

        if (_role == Role.ServerClient || _role == Role.Client)
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        }

        foreach (var world in World.All)//delete unnecessary worlds
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if(serverWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }
        else if (clientWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        SubScene[] subScenes = FindObjectsByType<SubScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);//find subscenes


        //connecting the worlds together
        if (serverWorld != null)
        {
            while(!serverWorld.IsCreated)//wait for the server world to create
            {
                yield return null;
            }

            if(subScenes != null)//load the subscenes
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(serverWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while (!SceneSystem.IsSceneLoaded(serverWorld.Unmanaged, sceneEntity))
                    {
                        serverWorld.Update();
                    }
                }
            }

            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.Parse(_listenIP, _port));//change server listening port
        }

        if (clientWorld != null)
        {
            while (!clientWorld.IsCreated)//wait for the client world to create
            {
                yield return null;
            }

            if (subScenes != null)//load the subscenes
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(clientWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while (!SceneSystem.IsSceneLoaded(clientWorld.Unmanaged, sceneEntity))
                    {
                        clientWorld.Update();
                    }
                }
            }

            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.Parse(_connectIP, _port));//connect to the server
        }
    }
    
}
