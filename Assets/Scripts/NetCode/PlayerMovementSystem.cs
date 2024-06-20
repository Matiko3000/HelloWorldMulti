using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAll<PlayerData, PlayerInputData, LocalTransform>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new PlayerMovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);//move the player
    }
}

[BurstCompile]
public partial struct PlayerMovementJob : IJobEntity
{
    public float deltaTime;
    public void Execute (PlayerData player, PlayerInputData input, ref LocalTransform transform) //the parameters are passed in automatically from the components of the entity its exectued on
    {
        Vector3 movement = new Vector3(input.move.x, 0, input.move.y) * player.speed * deltaTime;
        transform.Position = transform.Translate(movement).Position;
    }
}
