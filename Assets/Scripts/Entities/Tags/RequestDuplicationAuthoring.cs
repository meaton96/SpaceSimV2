using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RequestDuplicationAuthoring : MonoBehaviour {
    public class Baker : Baker<RequestDuplicationAuthoring> {
        public override void Bake(RequestDuplicationAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RequestDuplication());
            SetComponentEnabled<RequestDuplication>(entity, false);
        }
    }
}
public struct RequestDuplication : IComponentData, IEnableableComponent {
    public int index;
}
