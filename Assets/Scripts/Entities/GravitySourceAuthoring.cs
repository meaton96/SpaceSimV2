using Unity.Entities;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

class GravitySourceAuthoring : MonoBehaviour {
    // public float SOI_Radius = 10f;
    //public GameObject SOI;
    public float mass = 1f;
    class GravitySourceAuthoringBaker : Baker<GravitySourceAuthoring> {
        public override void Bake(GravitySourceAuthoring authoring) {
            var parentEntity = GetEntity(TransformUsageFlags.Dynamic);
          //  var childEntity = GetEntity(authoring.SOI, TransformUsageFlags.Dynamic);

            AddComponent(parentEntity, new GravitySource {
                mass = authoring.mass,
               // SOI = childEntity
            });

            //AddComponent(childEntity, new Parent { Value = parentEntity });
        }
    }

}

public struct GravitySource : IComponentData {
    //public float SOI_Radius;
    //public Entity SOI;
    public float mass;
}

