using Unity.Entities;
using UnityEngine;


public class GravitySpawner : MonoBehaviour{

    public class Baker : Baker<GravitySpawner> {
        public override void Bake(GravitySpawner authoring) {
            AddComponent(GetEntity(TransformUsageFlags.None),
                new GravitySpawn());
        }
    }
}
public struct GravitySpawn : IComponentData
{
    
}
