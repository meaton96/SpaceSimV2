using Unity.Entities;
using Unity.Burst;


[BurstCompile]
public struct BoundarySettings : IComponentData {
    public float boundaryX;
    public float boundaryZ;
    public float boundaryY;
}