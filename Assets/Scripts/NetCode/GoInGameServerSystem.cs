using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

//adding the NetworkStreamInGame so player is visible in game (Receive RPC from GoInGameClientSystem)

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAll<ReceiveRpcCommandRequest, GoInGameCommand>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoInGameCommand>>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(request.ValueRO.SourceConnection);
            commandBuffer.DestroyEntity(entity);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}