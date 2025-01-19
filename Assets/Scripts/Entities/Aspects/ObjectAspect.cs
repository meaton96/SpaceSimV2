using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public readonly partial struct ObjectAspect : IAspect {
    public readonly RefRW<LocalTransform> localTransform;
    public readonly RefRO<TypeComponent> typeComponent;
    public readonly RefRW<PhysicsVelocity> physicsVelocity;

    [BurstCompile]
    public void KeepInBounds(float deltaTime, float boundryX, float boundryY, float boundryZ) {
        if (localTransform.ValueRO.Position.x > boundryX) {
            localTransform.ValueRW.Position.x = -boundryX;
        }
        if (localTransform.ValueRO.Position.x < -boundryX) {
            localTransform.ValueRW.Position.x = boundryX;
        }
        if (localTransform.ValueRO.Position.y > boundryY) {
            localTransform.ValueRW.Position.y = -boundryY;
        }
        if (localTransform.ValueRO.Position.y < -boundryY) {
            localTransform.ValueRW.Position.y = boundryY;
        }
        if (math.abs(localTransform.ValueRO.Position.z) > boundryZ) {
            physicsVelocity.ValueRW.Linear.z = -physicsVelocity.ValueRO.Linear.z;
            localTransform.ValueRW.Position.z = math.clamp(localTransform.ValueRO.Position.z, -boundryZ, boundryZ);
        }

    }

}
