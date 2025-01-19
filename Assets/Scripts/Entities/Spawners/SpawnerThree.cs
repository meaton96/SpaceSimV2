using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerThree : MonoBehaviour
{
    public class Baker : Baker<SpawnerThree> {
        public override void Bake(SpawnerThree authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.Dynamic),
                new SpawnerThreeComponent()
                );
        }
    }
}

public struct SpawnerThreeComponent : IComponentData {

}