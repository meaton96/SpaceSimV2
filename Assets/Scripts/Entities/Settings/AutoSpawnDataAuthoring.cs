using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AutoSpawnDataAuthoring : MonoBehaviour {
    [SerializeField] private bool spawnOne;
    [SerializeField] private bool spawnTwo;
    [SerializeField] private bool spawnThree;
    [SerializeField] private bool spawnFour;
    [SerializeField] private int maxOfSingleEntityType = 15000;
    [SerializeField] private bool limitSpawn = false;

    [Range(.1f, 40)]
    [SerializeField]
    private float spawnRateOne = 20f;

    [Range(.1f, 40)]
    [SerializeField]
    private float spawnRateTwo = 20f;

    [Range(.1f, 40)]
    [SerializeField]
    private float spawnRateThree = 20f;

    [Range(.1f, 40)]
    [SerializeField]
    private float spawnRateFour = 20f;

    private float minSpawnRate = .1f;
    private float maxSpawnRate = 40f;

    public class Baker : Baker<AutoSpawnDataAuthoring> {
        public override void Bake(AutoSpawnDataAuthoring authoring) {
            AddComponent(
                GetEntity(TransformUsageFlags.None),
                new AutoSpawnData {
                    spawnOne = authoring.spawnOne,
                    spawnTwo = authoring.spawnTwo,
                    spawnThree = authoring.spawnThree,
                    spawnFour = authoring.spawnFour,
                    spawnRateOne = authoring.spawnRateOne,
                    spawnRateTwo = authoring.spawnRateTwo,
                    spawnRateThree = authoring.spawnRateThree,
                    spawnRateFour = authoring.spawnRateFour,
                    minSpawnRate = authoring.minSpawnRate,
                    maxSpawnRate = authoring.maxSpawnRate,
                    maxOfSingleEntityType = authoring.maxOfSingleEntityType,
                    limitSpawn = authoring.limitSpawn
                });
        }
    }
}

public struct AutoSpawnData : IComponentData {
    public bool spawnOne, spawnTwo, spawnThree, spawnFour;
    public float spawnRateOne, spawnRateTwo, spawnRateThree, spawnRateFour;
    public float minSpawnRate, maxSpawnRate;
    public int maxOfSingleEntityType;
    public bool limitSpawn;
    public float velocityMax;

    public bool GetSpawnStatus(int index) {
        return index switch {
            0 => spawnOne,
            1 => spawnTwo,
            2 => spawnThree,
            3 => spawnFour,
            _ => false
        };
    }
}
