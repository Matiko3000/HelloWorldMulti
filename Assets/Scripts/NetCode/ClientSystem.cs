using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

public struct ClientMessageRpcCommand : IRpcCommand
{
    public FixedString64Bytes message;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<NetworkId>();
    }
    protected override void OnUpdate()
    {
            
    }

    public void SendMassageRpc(string text, World world)
    {
        if (world == null || world.IsCreated == false) return;

        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ClientMessageRpcCommand));
        world.EntityManager.SetComponentData(entity, new ClientMessageRpcCommand()
        {
            message = text
        });
    }
}
