using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Physics.Extensions;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSimulationGroup))]
public partial struct CollisionEventsSystem : ISystem {



    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var entityManager = state.EntityManager;

        // Query entities with a StatefulCollisionEvent buffer
        foreach (var (physVelocity, physMass, buffer, entity) in SystemAPI.Query<PhysicsVelocity, PhysicsMass, DynamicBuffer<StatefulCollisionEvent>>().WithEntityAccess()) {

            foreach (var collisionEvent in buffer) {
                // Check collision state (Enter, Stay, Exit)
                switch (collisionEvent.State) {
                    case StatefulEventState.Enter:
                        var otherEntity = collisionEvent.GetOtherEntity(entity);

                        if (entityManager.HasComponent<TypeComponent>(entity) &&
                            entityManager.HasComponent<TypeComponent>(otherEntity)) {
                            var typeA = entityManager.GetComponentData<TypeComponent>(entity).type;
                            var typeB = entityManager.GetComponentData<TypeComponent>(otherEntity).type;

                            if (typeA == typeB) {
                                // Enable "Collided" component if types match
                                entityManager.SetComponentEnabled<Collided>(entity, true);
                                entityManager.SetComponentData(entity, new Collided { otherEntity = otherEntity });

                                //mark one of them to duplicate (picked up by spawner system)
                                //entityManager.AddComponent<RequestDuplication>(entity);
                                //entityManager.SetComponentData(entity, new RequestDuplication { index = (int)typeA });
                                entityManager.SetComponentEnabled<RequestDuplication>(entity, true);
                                entityManager.SetComponentData(entity, new RequestDuplication { index = (int)typeA });

                                entityManager.SetComponentEnabled<Collided>(otherEntity, true);
                                entityManager.SetComponentData(otherEntity, new Collided { otherEntity = entity });

                                //apply impulse force?
                                //  PushApart(entityManager, entity, otherEntity);

                            }
                            else {
                                // Enable "Annihilate" component if types differ
                                entityManager.SetComponentEnabled<Annihilate>(entity, true);
                                entityManager.SetComponentEnabled<Annihilate>(otherEntity, true);
                            }
                        }
                        break;
                    case StatefulEventState.Stay:
                        break;
                    case StatefulEventState.Exit:
                        break;

                }
            }
        }
    }

}
