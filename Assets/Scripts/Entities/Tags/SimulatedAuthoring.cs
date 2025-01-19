using Unity.Entities;
using UnityEngine;

class SimulatedAuthoring : MonoBehaviour
{
    class SimulatedAuthoringBaker : Baker<SimulatedAuthoring> {
        public override void Bake(SimulatedAuthoring authoring) {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Simulated());
        }
    }
}

public struct Simulated : IComponentData { }


