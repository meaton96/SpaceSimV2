using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(HandleObjectSystem))]
[BurstCompile]
public partial class ClearSimulationSystem : SystemBase {
    protected override void OnUpdate() {
        
    }

    public void ClearSimulation() {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<TypeComponent>().WithEntityAccess()) {
            commandBuffer.SetComponentEnabled<Annihilate>(entity, true);
        }
        commandBuffer.Playback(EntityManager);
    }
}
