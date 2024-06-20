using UnityEngine;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInputData : IInputComponentData
{
    public Vector2 move;
}

    
