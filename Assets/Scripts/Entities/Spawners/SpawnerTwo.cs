using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerTwo : MonoBehaviour
{
    public class Baker : Baker<SpawnerTwo> {
        public override void Bake(SpawnerTwo authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.Dynamic),
                new SpawnerTwoComponent()
                );
        }
    }
}

public struct SpawnerTwoComponent : IComponentData {

}
