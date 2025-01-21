using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//System to create a gravity object at the mouse position
public partial class CreateGravitySystem : SystemBase {

    protected override void OnCreate() {
        this.Enabled = false; //disabled
        RequireForUpdate<GravitySpawn>();
    }
    protected override void OnUpdate() {
        if (Input.GetMouseButtonUp(0)) {
            float3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            foreach (RefRO<SpawnerConfig> spawnerConfig in
                SystemAPI.Query<RefRO<SpawnerConfig>>().WithAll<GravitySpawn>()) {

                Entity gravityEntity = EntityManager.Instantiate(spawnerConfig.ValueRO.spawnPrefab);

                EntityManager.SetComponentData(gravityEntity, new LocalTransform { Position = mousePosition });

            }
        }
    }
}
