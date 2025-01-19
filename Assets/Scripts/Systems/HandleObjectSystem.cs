using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

//handles object movement and collision detection
public partial struct HandleObjectSystem : ISystem {

    // private BoundarySettings cachedBounds;
    private bool boundsInitialized;

    

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        //if (!boundsInitialized) {


        //    //Debug.Log($"Entity Exists: {SystemAPI.HasSingleton<BoundarySettings>()}");

        //    //cachedBounds = SystemAPI.GetSingleton<BoundarySettings>();
        //    boundsInitialized = true;

        //    if (boundsInitialized) {
        //        Debug.Log($"Bounds initialized: X={cachedBounds.boundaryX}, Y={cachedBounds.boundaryY}, Z={cachedBounds.boundaryZ}");
        //    }
        //}
        float boundaryX = 10f;
        float boundaryY = 10f;
        float boundaryZ = 10f;

        float deltaTime = SystemAPI.Time.DeltaTime;


        KeepInBoundsJob job = new KeepInBoundsJob {
            deltaTime = deltaTime,
            boundryX = boundaryX,
            boundryY = boundaryY,
            boundryZ = boundaryZ
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);

    }
    [BurstCompile]
    [WithAll(typeof(Simulated))]
    public partial struct KeepInBoundsJob : IJobEntity {
        public float deltaTime;
        public float boundryX, boundryY, boundryZ;
        public void Execute(ObjectAspect aspect) {
            // Debug.Log("Executing job");
            aspect.KeepInBounds(deltaTime, boundryX, boundryY, boundryZ);
        }
    }


}
