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

  //  public readonly RefRW<Movement> movement;
  //  public readonly RefRO<ACircleCollider> circleCollider;
    public readonly RefRO<TypeComponent> typeComponent;
    //public readonly RefRW<Annihilate> annihilate;
    public readonly RefRW<PhysicsVelocity> physicsVelocity;


    public void KeepInBounds(float deltaTime, float boundryX, float boundryY, float boundryZ) {

        if (localTransform.ValueRO.Position.x > boundryX) {
            localTransform.ValueRW.Position.x = -boundryX;
        }
        if (localTransform.ValueRO.Position.x < -boundryX) {
            localTransform.ValueRW.Position.x = boundryX;
        }
        if (localTransform.ValueRO.Position.z > boundryZ) {
            localTransform.ValueRW.Position.z = -boundryZ;
        }
        if (localTransform.ValueRO.Position.z < -boundryZ) {
            localTransform.ValueRW.Position.z = boundryZ;
        }
        if (math.abs(localTransform.ValueRO.Position.y) > boundryY) {
            physicsVelocity.ValueRW.Linear.y = -physicsVelocity.ValueRO.Linear.y;
            localTransform.ValueRW.Position.y = math.clamp(localTransform.ValueRO.Position.y, -boundryY, boundryY);
        }

    }

}
