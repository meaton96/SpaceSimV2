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


[UpdateAfter(typeof(CleanupSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial class SpawnerSystem : SystemBase {

    private BoundarySettings cachedBounds;
    private bool boundsInitialized;

    private AutoSpawnData cachedAutoSpawnData;
    private bool autoSpawnInitialized;

    private readonly float[] spawnTimers = new float[4];
    private readonly float[] spawnRates = new float[4];



    protected override void OnCreate() {
        RequireForUpdate<SpawnerConfig>();
    }
    [BurstCompile]
    protected override void OnUpdate() {
        UpdateAutoSpawnData();

        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            Spawn(0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2)) {
            Spawn(1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3)) {
            Spawn(2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4)) {
            Spawn(3);
        }

        if (!boundsInitialized) {
            cachedBounds = SystemAPI.GetSingleton<BoundarySettings>();
            boundsInitialized = true;
        }

        HandleSpawnQueue();
          HandleAutoSpawn(SystemAPI.Time.DeltaTime);


    }
    [BurstCompile]
    public void HandleAutoSpawn(float deltaTime) {

        for (int i = 0; i < spawnTimers.Length; i++) {
            if (cachedAutoSpawnData.GetSpawnStatus(i)) {
                spawnTimers[i] += deltaTime;
                if (spawnTimers[i] >= spawnRates[i]) {
                    Spawn(i);
                    spawnTimers[i] = 0;
                }
            }
        }


    }
    //get the new instance of the AutoSpawnData
    //adjust the spawn rates based on the new data
    public void UpdateAutoSpawnData() {
        cachedAutoSpawnData = SystemAPI.GetSingleton<AutoSpawnData>();

        spawnRates[0] = 1 / cachedAutoSpawnData.spawnRateOne;
        spawnRates[1] = 1 / cachedAutoSpawnData.spawnRateTwo;
        spawnRates[2] = 1 / cachedAutoSpawnData.spawnRateThree;
        spawnRates[3] = 1 / cachedAutoSpawnData.spawnRateFour;

    }
    //handles spawns from collisions
    [BurstCompile]
    public void HandleSpawnQueue() {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(WorldUpdateAllocator);
        Queue<int> entiesToSpawn = new Queue<int>();

        //disable duplication component and add to spawn queue
        foreach (var (entityToSpawn, entity) in
            SystemAPI.Query<RefRO<RequestDuplication>>().WithEntityAccess()) {
            entiesToSpawn.Enqueue(entityToSpawn.ValueRO.index);
            commandBuffer.SetComponentEnabled<RequestDuplication>(entity, false);
            break;

        }

        ////one spawn per frame
        //if (entiesToSpawn.Count > 0) {
        //    int type = entiesToSpawn.Dequeue();
        //    Spawn(type, commandBuffer);
        //}

        //spawn all entities in the queue
        while (entiesToSpawn.Count > 0) {
            int type = entiesToSpawn.Dequeue();
            bool spawned = Spawn(type, commandBuffer);
            if (spawned)
                UpdateEntityCounter(type, true, commandBuffer);
        }
        commandBuffer.Playback(EntityManager);
    }

    //overload for spawning entities based on index
    [BurstCompile]
    public void Spawn(int index) {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(WorldUpdateAllocator);
        bool spawned = Spawn(index, commandBuffer);
        if (spawned) {
            UpdateEntityCounter(index, false, commandBuffer);
        }
        commandBuffer.Playback(EntityManager);
    }
    [BurstCompile]
    //spawn an entity based on the index
    public bool Spawn(int index, EntityCommandBuffer commandBuffer) {

        if (!CanSpawn(index)) {

            return false;
        }


        bool spawned = false;
        foreach (var spawner in SystemAPI.Query<RefRO<SpawnerConfig>>()) {

            switch (index) {
                case 0:

                    foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerOneComponent>()) {
                        SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer);
                        spawned = true;
                    }
                    break;
                case 1:
                    foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerTwoComponent>()) {
                        SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer);
                        spawned = true;
                    }
                    break;
                case 2:
                    foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerThreeComponent>()) {
                        SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer);
                        spawned = true;
                    }
                    break;
                case 3:
                    foreach (var spawnerComponent in SystemAPI.Query<RefRO<SpawnerConfig>>().WithAny<SpawnerFourComponent>()) {
                        SpawnEntity(spawnerComponent.ValueRO.spawnPrefab, commandBuffer);
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

        }
        return false;
    }

    //check if the number of entities of the specified type is less than the max allowed
    [BurstCompile]
    private bool CanSpawn(int index) {
        if (!cachedAutoSpawnData.limitSpawn) return true;

        var counter = SystemAPI.GetSingleton<EntityCounterComponent>();



        bool canSpawn = index switch {
            0 => counter.TypeOneCount < cachedAutoSpawnData.maxOfSingleEntityType,
            1 => counter.TypeTwoCount < cachedAutoSpawnData.maxOfSingleEntityType,
            2 => counter.TypeThreeCount < cachedAutoSpawnData.maxOfSingleEntityType,
            3 => counter.TypeFourCount < cachedAutoSpawnData.maxOfSingleEntityType,
            _ => false
        };
        // Debug.Log($"Type {index} count: {counter.TypeOneCount} < {autoSpawnData.maxOfSingleEntityType}? {canSpawn}");

        return canSpawn;

    }

    //update entity counter component
    [BurstCompile]
    private void UpdateEntityCounter(int index, bool collisionSpawn, EntityCommandBuffer commandBuffer) {

        Entity entityCounter = SystemAPI.GetSingletonEntity<EntityCounterComponent>();

        EntityCounterComponent counter = SystemAPI.GetComponent<EntityCounterComponent>(entityCounter);
        switch (index) {
            case 0:
                counter.TypeOneCount++;
                break;
            case 1:
                counter.TypeTwoCount++;
                break;
            case 2:
                counter.TypeThreeCount++;
                break;
            case 3:
                counter.TypeFourCount++;
                break;
            default:
                break;

        }
        if (collisionSpawn) {
            counter.totalSpawnedByCollisions++;
        }
        else {
            counter.totalSpawnedBySimulator++;
        }
        commandBuffer.SetComponent(entityCounter, counter);
    }

    //spawns the passed in entity using the provided command buffer
    [BurstCompile]
    private void SpawnEntity(Entity entity, EntityCommandBuffer commandBuffer) {

        //calculate a initial velocity for the spawned entity
        float3 randomDirection = math.normalize(new float3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                0f
            )) * UnityEngine.Random.Range(2f, 5f);

        //set the position of the spawned entity to a random position
        float3 randomPosition = new float3(
            UnityEngine.Random.Range(-cachedBounds.boundaryX, cachedBounds.boundaryX),
            UnityEngine.Random.Range(-cachedBounds.boundaryY, cachedBounds.boundaryY),
            0f
        );




        Entity _entity = commandBuffer.Instantiate(entity);

        commandBuffer.SetComponent(_entity, new LocalTransform {
            Position = randomPosition,
            Rotation = quaternion.identity,
            Scale = 1f
        });
        commandBuffer.SetComponent(_entity, new PhysicsVelocity { Linear = randomDirection });

        // Debug.Log($"Spawned entity at {randomPosition} with velocity {randomDirection}");
    }
}
