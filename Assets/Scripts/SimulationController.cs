using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour {
    private SpawnerSystem spawnerSystem;
    private EntityManager entityManager;
    private Entity spawnDataEntity;
    private bool hasInit = false;

    private int maxOfSingleEntityType = 15000;


  //  [SerializeField] private TMP_InputField maxEntityInput;

    public static SimulationController Instance { get; private set; }

    public readonly bool[] spawnFlags = new bool[4];
    public readonly float[] spawnRates = new float[4];

    [SerializeField]
    private Slider[] sliders = new Slider[4];


    [SerializeField]
    private TextMeshProUGUI[] spawnRateTexts = new TextMeshProUGUI[4];

    [SerializeField]
    private TextMeshProUGUI pauseText;

    private bool isPaused = false;
    private World defaultWorld;
    private SimulationSystemGroup simulationSystemGroup;

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        Init();

    }
    private void Update() {
        //if (!hasInit) {
        //    try {
        //        Init();
        //    }
        //    catch (Exception e) {
        //     //   Debug.Log(e);
        //    }

        //}
    }
    public void Init() {
        spawnerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnerSystem>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        simulationSystemGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();
        spawnDataEntity = entityManager.CreateEntityQuery(typeof(AutoSpawnData)).GetSingletonEntity();

        //get initial data
        var spawnData = entityManager.GetComponentData<AutoSpawnData>(spawnDataEntity);

        spawnFlags[0] = spawnData.spawnOne;
        spawnFlags[1] = spawnData.spawnTwo;
        spawnFlags[2] = spawnData.spawnThree;
        spawnFlags[3] = spawnData.spawnFour;

        spawnRates[0] = spawnData.spawnRateOne;
        spawnRates[1] = spawnData.spawnRateTwo;
        spawnRates[2] = spawnData.spawnRateThree;
        spawnRates[3] = spawnData.spawnRateFour;

        maxOfSingleEntityType = spawnData.maxOfSingleEntityType;
      //  maxEntityInput.text = maxOfSingleEntityType.ToString();


        float minSpawnRate = spawnData.minSpawnRate;
        float maxSpawnRate = spawnData.maxSpawnRate;


        for (int i = 0; i < 4; i++) {
            sliders[i].value = spawnRates[i];
            sliders[i].minValue = minSpawnRate;
            sliders[i].maxValue = maxSpawnRate;

            spawnRateTexts[i].text = spawnRates[i].ToString();
        }

        for (int i = 0; i < 4; i++) {
            int index = i;
            sliders[index].onValueChanged.AddListener(value => {
                spawnRates[index] = value;
                spawnRateTexts[index].text = value.ToString();
                UpdateAutoSpawnData();
            });
        }
        hasInit = true;
    }


    public void UpdateMaxEntityAmount(string num) {
        if (int.TryParse(num, out int result)) {
            maxOfSingleEntityType = result;
            UpdateAutoSpawnData();
        }
    }
    //updates the ECS component data with the new values from the UI changes
    private void UpdateAutoSpawnData() {

        var spawnData = entityManager.GetComponentData<AutoSpawnData>(spawnDataEntity);
        //   Debug.Log("updating ecs data");
        spawnData.spawnOne = spawnFlags[0];
        spawnData.spawnTwo = spawnFlags[1];
        spawnData.spawnThree = spawnFlags[2];
        spawnData.spawnFour = spawnFlags[3];

        spawnData.spawnRateOne = spawnRates[0];
        spawnData.spawnRateTwo = spawnRates[1];
        spawnData.spawnRateThree = spawnRates[2];
        spawnData.spawnRateFour = spawnRates[3];
        spawnData.maxOfSingleEntityType = this.maxOfSingleEntityType;
        entityManager.SetComponentData(spawnDataEntity, spawnData);
    }

    public void Spawn(int index) {
        spawnerSystem.Spawn(index);
    }
    public void ToggleSimulation() {
        isPaused = !isPaused;
        simulationSystemGroup.Enabled = !isPaused;
        pauseText.text = isPaused ? "Resume" : "Pause";

    }
    public void ChangeAutoSpawn(int index) {
        spawnFlags[index] = !spawnFlags[index];
        //   Debug.Log(spawnFlags[index] ? "Spawning" : "Not Spawning");
        UpdateAutoSpawnData();
    }


}
