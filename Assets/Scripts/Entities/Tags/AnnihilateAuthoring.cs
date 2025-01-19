using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AnnihilateAuthoring : MonoBehaviour {
    public class Baker : Baker<AnnihilateAuthoring> {
        public override void Bake(AnnihilateAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Annihilate());
            SetComponentEnabled<Annihilate>(entity, false);
        }
    }
}
public struct Annihilate : IComponentData, IEnableableComponent {

}
