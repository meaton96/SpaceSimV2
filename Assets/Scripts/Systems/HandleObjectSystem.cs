using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

//Handles keeping the objects in bounds via parallel jobs across all workers
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct HandleObjectSystem : ISystem {


    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        BoundarySettings currentBounds = SystemAPI.GetSingleton<BoundarySettings>();
        float deltaTime = SystemAPI.Time.DeltaTime;


        KeepInBoundsJob job = new KeepInBoundsJob {
            deltaTime = deltaTime,
            boundryX = currentBounds.boundaryX,
            boundryY = currentBounds.boundaryY,
            boundryZ = currentBounds.boundaryZ
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);

    }
    [BurstCompile]
    [WithDisabled(typeof(Annihilate))]
    public partial struct KeepInBoundsJob : IJobEntity {
        public float deltaTime;
        public float boundryX, boundryY, boundryZ;
        public void Execute(ObjectAspect aspect) {
            aspect.KeepInBounds(deltaTime, boundryX, boundryY, boundryZ);
        }
    }


}
