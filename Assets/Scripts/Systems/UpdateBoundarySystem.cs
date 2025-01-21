using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;

//Handles changes to the boundary settings from the user input
//invokes a system action to tell the other systems to update their boundary settings
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

