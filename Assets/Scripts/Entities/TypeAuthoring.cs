using Unity.Entities;
using UnityEngine;

class TypeAuthoring : MonoBehaviour
{
    [SerializeField]
    private TypeComponent.Type type;

    class TypeAuthoringBaker : Baker<TypeAuthoring> {
        public override void Bake(TypeAuthoring authoring) {
            AddComponent(
                    GetEntity(TransformUsageFlags.Dynamic),
                    new TypeComponent {
                        type = authoring.type
                    });
        }
    }

}


public struct TypeComponent : IComponentData {

    public enum Type {
        One, Two, Three, Four
    }
    public Type type;


}
