using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;

[UpdateBefore(typeof(HandleObjectSystem))]
public partial class UpdateBoundarySystem : SystemBase {

    public static event Action OnBoundarySettingsChange;
    public Entity boundarySettingsEntity;

    private float currentBoundaryX;
    protected override void OnCreate() {
        RequireForUpdate<BoundarySettings>();
    }
    protected override void OnUpdate() {
        foreach (var (boundarySettings, entity) in SystemAPI.Query<RefRO<BoundarySettings>>().WithEntityAccess()) {
            if (boundarySettings.ValueRO.boundaryX != currentBoundaryX) {
                currentBoundaryX = boundarySettings.ValueRO.boundaryX;
                OnBoundarySettingsChange?.Invoke();
            }
        }
    }
}
[UpdateBefore(typeof(HandleObjectSystem))]
public partial class UpdateSpawnSettingsSystem : SystemBase {

    public static event Action OnMaxSpawnRateChange;
    public Entity autoSpawnData;

    private float currentBoundaryX;
    private float currentMaxSpawnRate;
    protected override void OnCreate() {
        RequireForUpdate<AutoSpawnData>();
    }
    protected override void OnUpdate() {
        foreach (var (autoSpawnData, entity) in SystemAPI.Query<RefRO<AutoSpawnData>>().WithEntityAccess()) {
            if (autoSpawnData.ValueRO.maxSpawnRate != currentMaxSpawnRate) {
                currentMaxSpawnRate = autoSpawnData.ValueRO.maxSpawnRate;
                OnMaxSpawnRateChange?.Invoke();
            }
        }
    }
}
