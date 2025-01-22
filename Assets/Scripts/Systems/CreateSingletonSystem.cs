using Unity.Burst;
using Unity.Entities;

//Creates the singleton entities that are used in the simulation
//For some reason my Authoring components in my subscene did not ever appear in the build (only singletons??)
//So manually creating them here
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
                boundaryX = 100,
                boundaryY = 50,
                boundaryZ = 1
            });
        }
        if (!SystemAPI.HasSingleton<EntityCounterComponent>()) {
            EntityManager entityManager = state.EntityManager;
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new EntityCounterComponent { });
        }
        if (!SystemAPI.HasSingleton<AutoSpawnData>()) {
            EntityManager entityManager = state.EntityManager;
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new AutoSpawnData {
                spawnOne = false,
                spawnTwo = false,
                spawnThree = false,
                spawnFour = false,
                spawnRateOne = 5,
                spawnRateTwo = 5,
                spawnRateThree = 5,
                spawnRateFour = 5,
                minSpawnRate = .1f,
                maxSpawnRate = 40,
                maxOfSingleEntityType = 4000,
                limitSpawn = true,
                velocityMax = 5f
            });

        }
        if (!SystemAPI.HasSingleton<SpawnRateData>()) {
            EntityManager entityManager = state.EntityManager;
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new SpawnRateData { });
        }

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
