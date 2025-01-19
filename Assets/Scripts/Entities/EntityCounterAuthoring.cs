using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class EntityCounterAuthoring : MonoBehaviour {


    public class Baker : Baker<EntityCounterAuthoring> {
        public override void Bake(EntityCounterAuthoring authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.Dynamic),
                new EntityCounterComponent());
        }
    }
}

public struct EntityCounterComponent : IComponentData {

    public int TypeOneCount;
    public int TypeTwoCount;
    public int TypeThreeCount;
    public int TypeFourCount;

    public int totalSpawnedByCollisions;
    public int totalSpawnedBySimulator;
    public int totalDestroyed;
}

