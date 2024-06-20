using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

public struct InitializedClient : IComponentData
{

}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ServerSystem : SystemBase
{
    private bool isNextPlayerRed = true;

    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ClientMessageRpcCommand>>().WithEntityAccess())//recieve messages
        {
            Debug.Log(command.ValueRO.message + " from client index " + request.ValueRO.SourceConnection.Index);
            commandBuffer.DestroyEntity(entity);
        }

        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<InitializedClient>().WithEntityAccess())//look for entites with network id but not initialized yet
        {
            commandBuffer.AddComponent<InitializedClient>(entity);
            PrefabsData prefabManager = SystemAPI.GetSingleton<PrefabsData>();
            if(prefabManager.playerRed != null && prefabManager.playerBlue != null)
            {
                Entity player;//spawn the player
                if (isNextPlayerRed)//determine the color of the player
                {
                    player = commandBuffer.Instantiate(prefabManager.playerRed);
                }
                else
                {
                    player = commandBuffer.Instantiate(prefabManager.playerBlue);
                }
                isNextPlayerRed = !isNextPlayerRed;//change the color of next player

                commandBuffer.SetComponent(player, new GhostOwner()//assign the owner of the client to the player
                {
                    NetworkId = id.ValueRO.Value
                });
                commandBuffer.AppendToBuffer(entity, new LinkedEntityGroup()//make sure the cube will dissaper when client disconnects
                {
                    Value = player
                });
                
            }
        }
        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }
}
