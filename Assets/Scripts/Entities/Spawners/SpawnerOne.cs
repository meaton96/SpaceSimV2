using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerOne : MonoBehaviour
{
    public class Baker : Baker<SpawnerOne> {
        public override void Bake(SpawnerOne authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.Dynamic),
                new SpawnerOneComponent()
                );
        }
    }
}

public struct SpawnerOneComponent : IComponentData {

}
