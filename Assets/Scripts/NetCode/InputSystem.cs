using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

//get the input from the player and put it into PlayerInputData component

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InputSystem : SystemBase
{
    private Actions actions;

    protected override void OnCreate()
    {
        actions = new Actions();
        actions.Enable();
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<PlayerInputData>();
        RequireForUpdate(GetEntityQuery(builder));
    }

    protected override void OnDestroy()
    {
        actions.Disable();
    }

    protected override void OnUpdate()
    {
        Vector2 playerMove = actions.Player.Move.ReadValue<Vector2>();
        foreach (RefRW<PlayerInputData> input in SystemAPI.Query<RefRW<PlayerInputData>>().WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW.move = playerMove;
        }
    }
}
