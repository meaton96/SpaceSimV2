
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine.Rendering;

//Main system for handling entity cleanup (deletion and collided entities)
//uses object pooling and a native queue to smooth deletion out per frame
//Queries for entities to delete are done on all threads, structural changes are done onthe main thread
//this comfortably deleted 40000 entities all at once with no noticeable frame drop
[BurstCompile]
[UpdateAfter(typeof(HandleObjectSystem))]
public partial struct CleanupSystem : ISystem {

    public const float PUSH_APART_FORCE = .5f;
    public const int MAX_DELETIONS_PER_FRAME = 50;
    private const float MAX_DELETION_PERCENT = .1f;
    public NativeQueue<(Entity, int)> entitiesToDelete;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        entitiesToDelete = new NativeQueue<(Entity, int)>(Allocator.Persistent); //allocate queue memory
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        //parallelize the query for entities to delete
        var collectJob = new CollectEntitiesJob {
            EntitiesToDestroy = entitiesToDelete.AsParallelWriter()
        };
        state.Dependency = collectJob.ScheduleParallel(state.Dependency);

        state.Dependency.Complete();

        HandleDeletionsFromQueue(ref state);
        CheckForCollisions(ref state);

    }
    [BurstCompile]
    //Handle the entities in the queue to be deleted
    //smooths out deletion per frame
    public void HandleDeletionsFromQueue(ref SystemState state) {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        int processedCount = 0;

        Entity entityCounter = SystemAPI.GetSingletonEntity<EntityCounterComponent>();
        EntityCounterComponent counter = SystemAPI.GetComponent<EntityCounterComponent>(entityCounter);


        NativeArray<int> deletes = new NativeArray<int>(4, Allocator.Temp);

        //delete entities from the queue until the max number of deletions per frame is reached
        while (entitiesToDelete.Count > 0 &&
            (processedCount < (MAX_DELETION_PERCENT * entitiesToDelete.Count)
            || processedCount < MAX_DELETIONS_PER_FRAME
            )) {
            var (entity, type) = entitiesToDelete.Dequeue();
            deletes[type]++;
            commandBuffer.DestroyEntity(entity);
            processedCount++;
        }

        //update the entity counter
        counter.TypeOneCount = math.max(0, counter.TypeOneCount - deletes[0]);
        counter.TypeTwoCount = math.max(0, counter.TypeTwoCount - deletes[1]);
        counter.TypeThreeCount = math.max(0, counter.TypeThreeCount - deletes[2]);
        counter.TypeFourCount = math.max(0, counter.TypeFourCount - deletes[3]);

        counter.totalDestroyed += processedCount;
        deletes.Dispose();

        commandBuffer.SetComponent(entityCounter, counter);

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        entitiesToDelete.Dispose(); //no memory leaks :)
    }

    //Check for enabled "Collided" components and handle the collision
    //this is enabled from collision checks in HandleObjectSystem
    //applies a small push apart force to entities that have collided, this prevents them from colliding over and over again rapidly
    [BurstCompile]
    public void CheckForCollisions(ref SystemState state) {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (aspect, physVelocity, physMass, collision, entity) in
            SystemAPI.Query<ObjectAspect, RefRW<PhysicsVelocity>, RefRW<PhysicsMass>, RefRO<Collided>>().
                 WithEntityAccess()) {


            Entity other = collision.ValueRO.otherEntity;

            if (!state.EntityManager.Exists(other)) {
                ecb.SetComponentEnabled<Collided>(entity, false);
                continue;
            }

            var otherTransform = state.EntityManager.GetComponentData<LocalTransform>(other);



            var transform = state.EntityManager.GetComponentData<LocalTransform>(entity);

            //small push apart force
            float3 direction = math.normalize(transform.Position - otherTransform.Position);
            float3 force = PUSH_APART_FORCE * SystemAPI.Time.DeltaTime * direction;

            physVelocity.ValueRW.ApplyLinearImpulse(physMass.ValueRW, force);

            ecb.SetComponentEnabled<Collided>(entity, false);
        }
    }

    //Job for collecting entities to delete
    [BurstCompile]
    partial struct CollectEntitiesJob : IJobEntity {
        public NativeQueue<(Entity, int)>.ParallelWriter EntitiesToDestroy;

        public void Execute(Entity entity, in TypeComponent type, in Annihilate annihilate) {

            EntitiesToDestroy.Enqueue((entity, (int)type.type));
        }
    }


}

