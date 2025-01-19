using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

//handles object movement and collision detection
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct HandleObjectSystem : ISystem {

     private BoundarySettings cachedBounds;
    private bool boundsInitialized;

    

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        if (!boundsInitialized) {


            //Debug.Log($"Entity Exists: {SystemAPI.HasSingleton<BoundarySettings>()}");

            cachedBounds = SystemAPI.GetSingleton<BoundarySettings>();
            boundsInitialized = true;

            if (boundsInitialized) {
                Debug.Log($"Bounds initialized: X={cachedBounds.boundaryX}, Y={cachedBounds.boundaryY}, Z={cachedBounds.boundaryZ}");
            }
        }




       

        float deltaTime = SystemAPI.Time.DeltaTime;

        //foreach (ObjectAspect aspect in SystemAPI.Query<ObjectAspect>()) {  
        //    aspect.KeepInBounds(deltaTime, boundaryX, boundaryY, boundaryZ);
        //}

        


        KeepInBoundsJob job = new KeepInBoundsJob {
            deltaTime = deltaTime,
            boundryX = cachedBounds.boundaryX,
            boundryY = cachedBounds.boundaryY,
            boundryZ = cachedBounds.boundaryZ
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);

    }
    [BurstCompile]
    public partial struct KeepInBoundsJob : IJobEntity {
        public float deltaTime;
        public float boundryX, boundryY, boundryZ;
        public void Execute(ObjectAspect aspect) {
            aspect.KeepInBounds(deltaTime, boundryX, boundryY, boundryZ);
        }
    }


}
