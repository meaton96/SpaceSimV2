using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
public struct SimulationSize {
    public float width;
    public float height;
    public float depth;
    public string name;
    public float maxSpawnRate;

    public override string ToString() {
        return $"{name} - Width: {width} Height: {height} Depth: {depth} MaxSpawnRate: {maxSpawnRate}";
    }
}
public class SimulationController : MonoBehaviour {
    public static SimulationController Instance { get; private set; }

    #region ECS Data
    private World defaultWorld;
    private SimulationSystemGroup simulationSystemGroup;
    private SpawnerSystem spawnerSystem;
    private ClearSimulationSystem clearSimSystem;
    private EntityManager entityManager;
    private Entity spawnDataEntity;
    private Entity counterEntity;
    private Entity boundaryEntity;

    #endregion

    #region Simulation Data
    private bool isPaused = false;
    public readonly bool[] spawnFlags = new bool[4];
    public readonly float[] spawnRates = new float[4];
    public readonly List<SimulationSize> simulationSizes = new List<SimulationSize>() {
        new SimulationSize {
            width = 1000,
            height = 500,
            depth = 1,
            maxSpawnRate = 40,
            name = "Small"
        },
        new SimulationSize {
            width = 2000,
            height = 1000,
            depth = 1,
            maxSpawnRate = 80,
            name = "Medium"
        },
        new SimulationSize {
            width = 3000,
            height = 1500,
            depth = 2,
            maxSpawnRate = 160,
            name = "Large"
        },
        new SimulationSize {
            width = 4000,
            height = 2000,
            depth = 5,
            maxSpawnRate = 400,
            name = "Huge"
        }
    };

    public int currentSimulationSizeIndex = 0;
    #endregion

    #region Interface Fields
    private float updateTimer;
    private int currentRadioIndex = -1;
    [SerializeField] private float updateInterval = .02f;


    [SerializeField]
    private TextMeshProUGUI entityCountText;
    [SerializeField]
    private TextMeshProUGUI entitySpawnDestroyText;
    [SerializeField]
    private TextMeshProUGUI boundarySizeText;

    [SerializeField]
    private Slider[] sliders = new Slider[4];

    [SerializeField]
    private Toggle[] sizeRadios = new Toggle[4];

    [SerializeField]
    private TextMeshProUGUI[] spawnRateTexts = new TextMeshProUGUI[4];

    [SerializeField]
    private TextMeshProUGUI pauseText;
    #endregion

    #region Initialization
    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        Init();
    }
    public void Init() {
        spawnerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnerSystem>();
        clearSimSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ClearSimulationSystem>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        simulationSystemGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();

        //get data entities
        spawnDataEntity = entityManager.CreateEntityQuery(typeof(AutoSpawnData)).GetSingletonEntity();
        counterEntity = entityManager.CreateEntityQuery(typeof(EntityCounterComponent)).GetSingletonEntity();
        boundaryEntity = entityManager.CreateEntityQuery(typeof(BoundarySettings)).GetSingletonEntity();

        //get initial spawn data
        var spawnData = entityManager.GetComponentData<AutoSpawnData>(spawnDataEntity);

        spawnFlags[0] = spawnData.spawnOne;
        spawnFlags[1] = spawnData.spawnTwo;
        spawnFlags[2] = spawnData.spawnThree;
        spawnFlags[3] = spawnData.spawnFour;

        spawnRates[0] = spawnData.spawnRateOne;
        spawnRates[1] = spawnData.spawnRateTwo;
        spawnRates[2] = spawnData.spawnRateThree;
        spawnRates[3] = spawnData.spawnRateFour;

        //maxOfSingleEntityType = spawnData.maxOfSingleEntityType;


        float minSpawnRate = spawnData.minSpawnRate;
        float maxSpawnRate = spawnData.maxSpawnRate;



        // Initialize UI elements
        for (int i = 0; i < 4; i++) {
            sliders[i].value = spawnRates[i];
            sliders[i].minValue = minSpawnRate;
            sliders[i].maxValue = maxSpawnRate;

            spawnRateTexts[i].text = spawnRates[i].ToString();

            //add listeners to radio toggle buttons
            int index = i;
            sizeRadios[i].onValueChanged.AddListener(isOn => {
                if (isOn) {

                    HandleSizeRadioClick(index);
                }
                else if (currentRadioIndex == index) {
                    sizeRadios[index].SetIsOnWithoutNotify(true);
                }
            });
        }

        //setup listeners for the sliders 
        for (int i = 0; i < 4; i++) {
            int index = i;
            sliders[index].onValueChanged.AddListener(value => {
                spawnRates[index] = value;
                spawnRateTexts[index].text = value.ToString();
                UpdateAutoSpawnData();
            });
        }
        //hasInit = true;
    }
    #endregion

    #region Interface Updates

    private void Update() {
        UpdateInterface();
    }
    private void UpdateInterface() {
        updateTimer += Time.deltaTime;
        if (updateTimer > updateInterval) {

            EntityCounterComponent counterComponent = entityManager.GetComponentData<EntityCounterComponent>(counterEntity);
           // BoundarySettings boundarySettings = entityManager.GetComponentData<BoundarySettings>(boundaryEntity);


            entityCountText.text = $"Object 1: {counterComponent.TypeOneCount}\n" +
                                   $"Object 2: {counterComponent.TypeTwoCount}\n" +
                                   $"Object 3: {counterComponent.TypeThreeCount}\n" +
                                   $"Object 4: {counterComponent.TypeFourCount}\n";

            entitySpawnDestroyText.text = $"Sim Spawns: {counterComponent.totalSpawnedBySimulator}\n" +
                                          $"Col Spawns: {counterComponent.totalSpawnedByCollisions}\n" +
                                          $"Destroyed: {counterComponent.totalDestroyed}";

            var simulationSize = simulationSizes[currentSimulationSizeIndex];

            boundarySizeText.text = $"Width: {simulationSize.width}\n" +
                                    $"Height: {simulationSize.height}\n" +
                                    $"Depth: {simulationSize.depth}";
        }
    }
    


    //handle clicking onthe ui toggle button
    public void HandleSizeRadioClick(int index) {
        if (currentRadioIndex != index) {
            currentRadioIndex = index;
            currentSimulationSizeIndex = index;
            UpdateECSSimulationBoundary(index);
            UpdateSliderMax();
            // Deselect all other toggles
            for (int i = 0; i < sizeRadios.Length; i++) {
                if (i != index) {
                    sizeRadios[i].SetIsOnWithoutNotify(false);
                }
            }
        }
    }
    //update the maximum value allowed on the slider
    private void UpdateSliderMax() {
        for (int i = 0; i < sliders.Length; i++) {
            float maxRate = simulationSizes[currentSimulationSizeIndex].maxSpawnRate;
            sliders[i].maxValue = maxRate;
            if (sliders[i].value > maxRate)
                sliders[i].value = maxRate;
        }
    }
    #endregion

    #region Update ECS Data Functions

    //public void UpdateMaxEntityAmount(string num) {
    //    if (int.TryParse(num, out int result)) {
    //        maxOfSingleEntityType = result;
    //        UpdateAutoSpawnData();
    //    }
    //}

    public void ClearSimulation() {
        clearSimSystem.ClearSimulation();
    }

    //update the ECS boundary component with the new values from the UI changes
    private void UpdateECSSimulationBoundary(int index) {
        var simulationSize = simulationSizes[index];
        //update boundary settings
        var boundarySettings = entityManager.GetComponentData<BoundarySettings>(boundaryEntity);
        boundarySettings.boundaryX = simulationSize.width;
        boundarySettings.boundaryY = simulationSize.height;
        boundarySettings.boundaryZ = simulationSize.depth;
        entityManager.SetComponentData(boundaryEntity, boundarySettings);

        //update max spawn rate settings
        var autoSpawnData = entityManager.GetComponentData<AutoSpawnData>(spawnDataEntity);
        autoSpawnData.maxSpawnRate = simulationSize.maxSpawnRate;
        entityManager.SetComponentData(spawnDataEntity, autoSpawnData);
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
        //spawnData.maxOfSingleEntityType = this.maxOfSingleEntityType;
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

    #endregion
}
