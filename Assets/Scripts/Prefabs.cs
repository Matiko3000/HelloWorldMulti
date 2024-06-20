using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public GameObject playerRed = null;
    public GameObject playerBlue = null;
}

public struct PrefabsData : IComponentData
{
    public Entity playerRed;
    public Entity playerBlue;
}

public class PrefabsBaker : Baker<Prefabs>
{
    public override void Bake(Prefabs authoring)
    {
        if (authoring.playerRed != null && authoring.playerBlue != null)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabsData
            {
                playerRed = GetEntity(authoring.playerRed, TransformUsageFlags.Dynamic),
                playerBlue = GetEntity(authoring.playerBlue, TransformUsageFlags.Dynamic)
            });
        }
    }
}