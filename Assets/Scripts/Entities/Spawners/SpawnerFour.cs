using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerFour : MonoBehaviour
{
    public class Baker : Baker<SpawnerFour> {
        public override void Bake(SpawnerFour authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.Dynamic),
                new SpawnerFourComponent()
                );
        }
    }
}

public struct SpawnerFourComponent : IComponentData {

}
