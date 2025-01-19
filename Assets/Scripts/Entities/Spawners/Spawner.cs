using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject spawnPrefab;

    public class Baker : Baker<Spawner> {
        public override void Bake(Spawner authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.None),
                new SpawnerConfig {
                    spawnPrefab = GetEntity(authoring.spawnPrefab, TransformUsageFlags.Dynamic)
                });
        }
    }



}

public struct SpawnerConfig : IComponentData {
    public Entity spawnPrefab;
}
