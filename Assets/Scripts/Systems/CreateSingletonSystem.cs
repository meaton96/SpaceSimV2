using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct CreateSingletonSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        if (!SystemAPI.HasSingleton<BoundarySettings>()) {
            EntityManager entityManager = state.EntityManager;
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new BoundarySettings {
                boundaryX = 1000,
                boundaryY = 500,
                boundaryZ = 1
            });
        }
        if (!SystemAPI.HasSingleton<EntityCounterComponent>()) {
            EntityManager entityManager = state.EntityManager;
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new EntityCounterComponent { });
        }
        //if (!SystemAPI.HasSingleton<AutoSpawnData>()) {
        //    EntityManager entityManager = state.EntityManager;
        //    Entity entity = entityManager.CreateEntity();
        //    entityManager.AddComponentData(entity, new AutoSpawnData {
        //        spawnOne = false,
        //        spawnTwo = false,
        //        spawnThree = false,
        //        spawnFour = false,
        //        spawnRateOne = 20,
        //        spawnRateTwo = 20,
        //        spawnRateThree = 20,
        //        spawnRateFour = 20,
        //        minSpawnRate = .1f,
        //        maxSpawnRate = 40,
        //        maxOfSingleEntityType = 15000,
        //        limitSpawn = true
        //    });

        //}

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
