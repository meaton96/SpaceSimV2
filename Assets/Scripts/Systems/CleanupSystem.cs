
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine.Rendering;

[BurstCompile]
[UpdateAfter(typeof(HandleObjectSystem))]
public partial struct CleanupSystem : ISystem {

    public const float PUSH_APART_FORCE = .5f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        CheckForDeletions(ref state);

        CheckForCollisions(ref state);

    }

    //Check for enabled "Collided" components and handle the collision
    //this is enabled from collision checks in HandleObjectSystem
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



    //checks for enabled "Annihilate" components and destroys the entities they are attached to
    //this is enabled from collision checks in HandleObjectSystem
    [BurstCompile]
    public void CheckForDeletions(ref SystemState state) {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
       // Entity entityCounter = SystemAPI.GetSingletonEntity<EntityCounterComponent>();
        //int[] entityChanges = new int[4];
        //EntityCounterComponent counter = SystemAPI.GetComponent<EntityCounterComponent>(entityCounter);
        foreach (var (annihilateEnabled, type, entity) in
                 SystemAPI.Query<EnabledRefRO<Annihilate>, RefRO<TypeComponent>>().WithEntityAccess()) {
            if (annihilateEnabled.ValueRO) {
                switch ((int)type.ValueRO.type) {
                    case 0:
                       // counter.TypeOneCount--;
                        break;
                    case 1:
                       // counter.TypeTwoCount--;
                        break;
                    case 2:
                       // counter.TypeThreeCount--;
                        break;
                    case 3:
                     //   counter.TypeFourCount--;
                        break;
                    default:
                        break;
                }
              //  counter.totalDestroyed++;
                commandBuffer.DestroyEntity(entity);
            }
        }
       // commandBuffer.SetComponent(entityCounter, counter);
        commandBuffer.Playback(state.EntityManager);
    }



}

