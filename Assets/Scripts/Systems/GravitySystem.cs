using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Transforms;

partial struct GravitySystem : ISystem {

    public const float G = -1f;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
      
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach ((
            RefRO<LocalTransform> localTransform, 
            RefRO<GravitySource> gravitySource, 
            DynamicBuffer<StatefulTriggerEvent> buffer, 
            Entity entity) in
            SystemAPI.Query<
                RefRO<LocalTransform>, 
                RefRO<GravitySource>, 
                DynamicBuffer<StatefulTriggerEvent>>().
                WithEntityAccess()) {

            var gravity = gravitySource.ValueRO;
           // var childEntity = gravity.SOI;

            if (!buffer.IsEmpty) {
                //var triggerBuffer = SystemAPI.GetBuffer<StatefulTriggerEvent>(childEntity);

                foreach (var collisionEvent in buffer) {
                    var otherEntity = collisionEvent.GetOtherEntity(entity);

                    if (state.EntityManager.HasComponent<PhysicsVelocity>(otherEntity)) {
                        var otherVelocity = state.EntityManager.GetComponentData<PhysicsVelocity>(otherEntity);
                        var otherMass = state.EntityManager.GetComponentData<PhysicsMass>(otherEntity);

                        var distance = math.distancesq(localTransform.ValueRO.Position, state.EntityManager.GetComponentData<LocalTransform>(otherEntity).Position);

                        var force = G * gravity.mass * otherMass.InverseMass / distance;

                        var direction = math.normalize(state.EntityManager.GetComponentData<LocalTransform>(otherEntity).Position - localTransform.ValueRO.Position);

                        var acceleration = force * direction;

                        otherVelocity.Linear += acceleration;

                        state.EntityManager.SetComponentData(otherEntity, otherVelocity);
                    }
                }
            }

            
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
