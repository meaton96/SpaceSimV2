using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CollidedAuthoring : MonoBehaviour {
    public class Baker : Baker<CollidedAuthoring> {
        public override void Bake(CollidedAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Collided());
            SetComponentEnabled<Collided>(entity, false);
        }
    }
}
public struct Collided : IComponentData, IEnableableComponent {
    public Entity otherEntity;
}
