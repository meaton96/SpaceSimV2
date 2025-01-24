using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Physics.Systems;
using Unity.Collections;

//Main system for spawning in new entities
//Handles spawning from auto spawning (ui sliders/toggles) and collision spawning
[UpdateAfter(typeof(CleanupSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial class SpawnerSystem : SystemBase {

    private BoundarySettings cachedBounds;
    private bool boundsInitialized;



    private AutoSpawnData cachedAutoSpawnData;
    private bool autoSpawnInitialized;

    public const int MAX_SPAWNS_PER_FRAME = 30;
    //private const float MAX_SPAWN_PERCENT = .1f;

    private readonly float[] spawnTimers = new float[4];
    private readonly float[] spawnsPerSecond = new float[4];

    private bool spawnCollisionsRandomly = true;

    private NativeQueue<(int, bool, float3)> spawnQueue;

    float timer;
    float numSpawned;



    protected override void OnCreate() {
        RequireForUpdate<SpawnerConfig>();
        RequireForUpdate<AutoSpawnData>();
        RequireForUpdate<EntityCounterComponent>();
        //allocate memory for queue
        spawnQueue = new NativeQueue<(int, bool, float3)>(Allocator.Persistent);

        //subscribe to the boundary settings change event
        UpdateBoundarySystem.OnBoundarySettingsChange += HandleBoundarySettingsChange;

    }

    //flips the initialized flag when the boundary settings change to force a re-fetch
    private void HandleBoundarySettingsChange() {
        boundsInitialized = false;
    }

    //clear the spawn queue
    //used to clear spawns when the simulation is cleared
    public void ClearSpawnQueue() {
        spawnQueue.Clear();
    }
    public void ToggleRandomCollisionSpawns() {
        spawnCollisionsRandomly = !spawnCollisionsRandomly;
    }

    protected override void OnUpdate() {
        UpdateAutoSpawnData();

        if (!boundsInitialized) {
            cachedBounds = SystemAPI.GetSingleton<BoundarySettings>();
            boundsInitialized = true;
        }
        HandleAutoSpawn(SystemAPI.Time.DeltaTime);

        HandleSpawnQueue(SystemAPI.Time.DeltaTime);



    }
    //get the new instance of the AutoSpawnData
    //adjust the spawn rates based on the new data
    public void UpdateAutoSpawnData() {
        cachedAutoSpawnData = SystemAPI.GetSingleton<AutoSpawnData>();

        spawnsPerSecond[0] = cachedAutoSpawnData.spawnRateOne;
        spawnsPerSecond[1] = cachedAutoSpawnData.spawnRateTwo;
        spawnsPerSecond[2] = cachedAutoSpawnData.spawnRateThree;
        spawnsPerSecond[3] = cachedAutoSpawnData.spawnRateFour;

    }
    //handles auto spawning based on the spawn rates set on the interface
    public void HandleAutoSpawn(float deltaTime) {
        for (int i = 0; i < spawnTimers.Length; i++) {
            if (cachedAutoSpawnData.GetSpawnStatus(i)) {

                spawnTimers[i] += deltaTime;

                // Calculate how many spawns are needed based on spawnsPerSecond
                float spawnInterval = 1f / spawnsPerSecond[i];
                int spawnsToEnqueue = Mathf.FloorToInt(spawnTimers[i] / spawnInterval);

                if (spawnsToEnqueue > 0) {
                    for (int j = 0; j < spawnsToEnqueue; j++) {

                        spawnQueue.Enqueue((i, true, new float3(0, 0, 0)));
                    }

                    // Reduce the accumulated timer by the time accounted for spawns
                    spawnTimers[i] -= spawnsToEnqueue * spawnInterval;
                }
            }
        }
        //Debug.Log(spawnQueue.Count);
    }

    [BurstCompile]
    //handles all entity spawning by pulling them from the queue
    public void HandleSpawnQueue(float deltaTime) {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(WorldUpdateAllocator);
        EnqueueCollisionSpawns(commandBuffer, deltaTime);

        int processedCount = 0;
        timer += deltaTime;
        //spawn all entities in the queue up to a maximum per frame
        int[] typesSpawned = new int[4];
        bool spawnedAny = false;
        while (spawnQueue.Count > 0 && processedCount < MAX_SPAWNS_PER_FRAME) {

            var (type, spawnRandom, spawnLocation) = spawnQueue.Dequeue();



            bool spawned = spawnCollisionsRandomly ?
                Spawn(type, commandBuffer, true, spawnLocation) : //force random spawn for collisions
                Spawn(type, commandBuffer, spawnRandom, spawnLocation); //allow the spawn type to determine location


            if (spawned) {
                spawnedAny = true;
                processedCount++;
                typesSpawned[type]++;

            }

        }
        if (spawnedAny) {
            UpdateEntityCounter(typesSpawned, commandBuffer);
        }




        numSpawned += processedCount;
        if (timer > 1f) {
            //Debug.Log($"Spawned {numSpawned} over the last 1 second");
            foreach (var (spawnRateData, entity) in SystemAPI.Query<RefRW<SpawnRateData>>().WithEntityAccess()) {
                commandBuffer.SetComponent(entity, new SpawnRateData {
                    currentSpawnRate = numSpawned
                });
            }
            numSpawned = 0;
            timer = 0;
        }
        commandBuffer.Playback(EntityManager);

    }
    //searches for the request duplication component added by the collision system
    //adds the entity to the spawn queue and disables the request duplication component
    [BurstCompile]
    public void EnqueueCollisionSpawns(EntityCommandBuffer commandBuffer, float deltaTime) {
        //disable duplication component and add to spawn queue
        int numCollisionDetections = 0;
        foreach (var (entityToSpawn, entity) in
            SystemAPI.Query<RefRO<RequestDuplication>>().WithEntityAccess()) {



            numCollisionDetections++;

            //enqueue a spawn request marked as a collision spawn
            spawnQueue.Enqueue((
                entityToSpawn.ValueRO.index,
                false,
                entityToSpawn.ValueRO.collisionLocation
                ));


            commandBuffer.SetComponentEnabled<RequestDuplication>(entity, false);
            //break;
        }

    }
    [BurstCompile]
    //spawn an entity based on the index
    public bool Spawn(int index, EntityCommandBuffer commandBuffer, bool spawnRandom = true, float3 spawnLocation = default) {

        bool spawned = false;

        switch (index) {
            case 0:

                foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerOneComponent>()) {
                    SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer, 1f, spawnRandom, spawnLocation);
                    spawned = true;
                }
                break;
            case 1:
                foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerTwoComponent>()) {
                    SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer, 4f, spawnRandom, spawnLocation);
                    spawned = true;
                }
                break;
            case 2:
                foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerThreeComponent>()) {
                    SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer, 2f, spawnRandom, spawnLocation);
                    spawned = true;
                }
                break;
            case 3:
                foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerFourComponent>()) {
                    SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer, 1.5f, spawnRandom, spawnLocation);
                    spawned = true;
                }
                break;
            default:
                Debug.LogError("Invalid index for spawner!");
                break;
        }
        if (spawned) {
            return true;
        }



        return false;
    }



    //update entity counter component
    [BurstCompile]
    private void UpdateEntityCounter(int[] typeAmounts, EntityCommandBuffer commandBuffer) {

        Entity entityCounter = SystemAPI.GetSingletonEntity<EntityCounterComponent>();

        EntityCounterComponent counter = SystemAPI.GetComponent<EntityCounterComponent>(entityCounter);
        for (int i = 0; i < typeAmounts.Length; i++) {
            switch (i) {
                case 0:
                    counter.TypeOneCount += typeAmounts[i];
                    break;
                case 1:
                    counter.TypeTwoCount += typeAmounts[i];
                    break;
                case 2:
                    counter.TypeThreeCount += typeAmounts[i];
                    break;
                case 3:
                    counter.TypeFourCount += typeAmounts[i];
                    break;
                default:
                    break;

            }
        }
        counter.totalSpawnedBySimulator++;
        commandBuffer.SetComponent(entityCounter, counter);
    }

    //spawns the passed in entity using the provided command buffer
    [BurstCompile]
    private void SpawnEntity(Entity entity, EntityCommandBuffer commandBuffer, float scale = 1f, bool spawnRandom = true, float3 location = default) {

        float maxVelocity = cachedAutoSpawnData.velocityMax;
        float minVelocity = maxVelocity * .4f;
        //calculate a initial velocity for the spawned entity
        float3 randomDirection = math.normalize(new float3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                0f
            )) * UnityEngine.Random.Range(minVelocity, maxVelocity);

        //set the position of the spawned entity to a random position

        float3 spawnPos = spawnRandom ?
        new float3(
            UnityEngine.Random.Range(-cachedBounds.boundaryX, cachedBounds.boundaryX),
            UnityEngine.Random.Range(-cachedBounds.boundaryY, cachedBounds.boundaryY),
            0f
        ) :
        location;




        Entity _entity = commandBuffer.Instantiate(entity);

        commandBuffer.SetComponent(_entity, new LocalTransform {
            Position = spawnPos,
            Rotation = quaternion.identity,
            Scale = scale
        });
        commandBuffer.SetComponent(_entity, new PhysicsVelocity { Linear = randomDirection });

        // Debug.Log($"Spawned entity at {randomPosition} with velocity {randomDirection}");
    }

    protected override void OnDestroy() {
        UpdateBoundarySystem.OnBoundarySettingsChange -= HandleBoundarySettingsChange;
        spawnQueue.Dispose();
    }


}
