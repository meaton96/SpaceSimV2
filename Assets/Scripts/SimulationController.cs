using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    private Entity spawnRateEntity;

    #endregion

    #region Simulation Data


    
    private float numDestroyed;
    private int destroyedSmoothingFrames = 60;
    private Queue<float> numDestroyedPerFrame = new Queue<float>();
    private float maxVelocity = 5f;
    private bool isPaused = false;
    public readonly bool[] spawnFlags = new bool[4];
    public readonly float[] spawnRates = new float[4];

    //stores the sizes of the simulation
    //ideally this would be a file or scriptable asset asset maybe
    //the initial size should also be set in CreateSingletonSystem.cs which is not great
    public readonly List<SimulationSize> simulationSizes = new List<SimulationSize>() {
        new SimulationSize {
            width = 100,
            height = 50,
            depth = 1,
            maxSpawnRate = 40,
            name = "Small"
        },
        new SimulationSize {
            width = 1000,
            height = 5000,
            depth = 1,
            maxSpawnRate = 50,
            name = "Medium"
        },
        new SimulationSize {
            width = 2000,
            height = 1000,
            depth = 2,
            maxSpawnRate = 50,
            name = "Large"
        },
        new SimulationSize {
            width = 4000,
            height = 2000,
            depth = 5,
            maxSpawnRate = 50,
            name = "Huge"
        }
    };

    public int currentSimulationSizeIndex = 0;
    #endregion

    #region Interface Fields

    private float updateTimer;
    private int currentRadioIndex = -1;
    [SerializeField] private float updateInterval = .02f;
    [SerializeField] private Interface pcInterface;
    [SerializeField] private Interface mobileInterface;
    private bool isMobile =>  Application.isMobilePlatform;



    #endregion

    #region Frame Time Tracking
    private const float targetFramerate = 30f;
    private const float warningFramerate = 15f;
    private const float pauseFramerate = 10f;
    private float waitTime = 2f;
    private float waitTimer;
    private const float smoothingFrames = 10;
    private Queue<float> frameTimes = new Queue<float>();

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
        spawnRateEntity = entityManager.CreateEntityQuery(typeof(SpawnRateData)).GetSingletonEntity();

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

            var slider = isMobile ? mobileInterface.sliders[i] : pcInterface.sliders[i];

            slider.value = spawnRates[i];
            slider.minValue = minSpawnRate;
            slider.maxValue = maxSpawnRate;

            var spawnRateText = isMobile ? mobileInterface.spawnRateTexts[i] : pcInterface.spawnRateTexts[i];

            spawnRateText.text = spawnRates[i].ToString();

            //add listeners to radio toggle buttons
            int index = i;

            var sizeRadio = isMobile ? mobileInterface.sizeRadios[i] : pcInterface.sizeRadios[i];

            sizeRadio.onValueChanged.AddListener(isOn => {
                if (isOn) {

                    HandleSizeRadioClick(index);
                }
                else if (currentRadioIndex == index) {
                    sizeRadio.SetIsOnWithoutNotify(true);
                }
            });
        }

        //setup listeners for the sliders 
        for (int i = 0; i < 4; i++) {
            int index = i;

            var slider = isMobile ? mobileInterface.sliders[index] : pcInterface.sliders[index];
            var spawnRateText = isMobile ? mobileInterface.spawnRateTexts[index] : pcInterface.spawnRateTexts[index];
            slider.onValueChanged.AddListener(value => {
                spawnRates[index] = value;
                spawnRateText.text = value.ToString();
                UpdateAutoSpawnData();
            });
        }
        var velSlider = isMobile ? mobileInterface.velocitySlider : pcInterface.velocitySlider;
        //add listener to velocity slider
        velSlider.onValueChanged.AddListener(value => HandleVelocitySliderChange(value));

        //add listener for toggle stats
        if (isMobile) {
            mobileInterface.toggleStats.onValueChanged.AddListener(val => mobileInterface.statsTextParent.SetActive(val));
            mobileInterface.gameObject.SetActive(true);
            pcInterface.gameObject.SetActive(false);
        }
        else {
            pcInterface.gameObject.SetActive(true);
            mobileInterface.gameObject.SetActive(false);
        }

    }
    #endregion

    #region Frametime Tracking
    private void TrackFrameTime() {
        //  Debug.Log(Time.deltaTime);
        if (waitTimer < waitTime) {
            waitTimer += Time.deltaTime;
        }
        else {
            if (frameTimes.Count > smoothingFrames) {
                frameTimes.Dequeue();
            }
            frameTimes.Enqueue(Time.deltaTime);
            float averageFrameTime = frameTimes.Average();

            if (averageFrameTime > 1 / pauseFramerate) {
                HandleEmergencyPause();
            }
            var frameTimeText = isMobile ? mobileInterface.frameTimeWarningText : pcInterface.frameTimeWarningText;
            if (averageFrameTime > 1 / warningFramerate) {

                frameTimeText.gameObject.SetActive(true);
            }
            else {
                frameTimeText.gameObject.SetActive(false);
            }
        }


    }
    #endregion

    #region Pause
    //handle pausing the simulation to avoid crashing the game
    private void HandleEmergencyPause() {
        var messageParent = isMobile ? mobileInterface.crashMessageParent : pcInterface.crashMessageParent;
        messageParent.SetActive(true);
        HandlePause();
    }
    public void HandleEmergencySimulationClear() {
        HandlePause();
        ClearSimulation();
        var messageParent = isMobile ? mobileInterface.crashMessageParent : pcInterface.crashMessageParent;
        messageParent.SetActive(false);

    }
    //toggle pausing the simulation
    public void HandlePause() {
        isPaused = !isPaused;
        var simulationGroup = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();
        simulationGroup.Enabled = !isPaused;
        var pauseButtonText = isMobile ? mobileInterface.pauseButtonText : pcInterface.pauseButtonText;
        pauseButtonText.text = isPaused ? "Resume" : "Pause";
        Time.timeScale = isPaused ? 0 : 1;


    }

    #endregion

    #region Interface Updates

    private void Update() {
        TrackFrameTime();
        UpdateSimulationInfoText();

    }
    


    //update the info text onthe ui for entity count, number of entities spawned and destroyed, and boundary size
    private void UpdateSimulationInfoText() {
        updateTimer += Time.deltaTime;



        if (updateTimer > updateInterval) {

            EntityCounterComponent counterComponent = entityManager.GetComponentData<EntityCounterComponent>(counterEntity);
            SpawnRateData spawnRateComponent = entityManager.GetComponentData<SpawnRateData>(spawnRateEntity);
            float spawnRate = spawnRateComponent.currentSpawnRate;
            float destroyRate = (counterComponent.totalDestroyed - numDestroyed) / updateInterval;

            if (numDestroyedPerFrame.Count > destroyedSmoothingFrames) {
                numDestroyedPerFrame.Dequeue();
            }

            numDestroyedPerFrame.Enqueue(destroyRate);

            float smoothedDestroyRate = numDestroyedPerFrame.Average();
            numDestroyed = counterComponent.totalDestroyed;

            var updateText = isMobile ? mobileInterface.entityCountText : pcInterface.entityCountText;

            updateText.text = $"Object 1: {counterComponent.TypeOneCount}\n" +
                                   $"Object 2: {counterComponent.TypeTwoCount}\n" +
                                   $"Object 3: {counterComponent.TypeThreeCount}\n" +
                                   $"Object 4: {counterComponent.TypeFourCount}\n";

            updateText = isMobile ? mobileInterface.entitySpawnDestroyText : pcInterface.entitySpawnDestroyText;

            updateText.text = $"Spawns: {counterComponent.totalSpawnedBySimulator}\n " +
                                          $"/sec: {Mathf.Ceil(spawnRate)}\n" +
                                          $"Deletes: {counterComponent.totalDestroyed}\n" +
                                          $"/sec: {Mathf.Ceil(smoothedDestroyRate)}";

            var simulationSize = simulationSizes[currentSimulationSizeIndex];

            updateText = isMobile ? mobileInterface.boundarySizeText : pcInterface.boundarySizeText;

            updateText.text = $"Width: {simulationSize.width}\n" +
                                    $"Height: {simulationSize.height}\n" +
                                    $"Depth: {simulationSize.depth}";


        }
    }
    //handle changing the velocity slider
    public void HandleVelocitySliderChange(float value) {
        maxVelocity = value;
        var updateText = isMobile ? mobileInterface.velocityText : pcInterface.velocityText;
        updateText.text = $"{maxVelocity} m/s";
        UpdateAutoSpawnData();
    }


    //handle clicking onthe ui toggle button
    public void HandleSizeRadioClick(int index) {
        if (currentRadioIndex != index) {
            ClearSimulation();
            currentRadioIndex = index;
            currentSimulationSizeIndex = index;
            UpdateECSSimulationBoundary(index);
            UpdateSliderMax();
            // Deselect all other toggles
            for (int i = 0; i < 4; i++) {
                if (i != index) {
                    if (isMobile) {
                        mobileInterface.sizeRadios[i].SetIsOnWithoutNotify(false);
                    }
                    else {
                        pcInterface.sizeRadios[i].SetIsOnWithoutNotify(false);
                    }

                }
            }
        }
    }
    //update the maximum value allowed on the slider
    private void UpdateSliderMax() {
        for (int i = 0; i < 4; i++) {
            float maxRate = simulationSizes[currentSimulationSizeIndex].maxSpawnRate;
            var slider = isMobile ? mobileInterface.sliders[i] : pcInterface.sliders[i];
            slider.maxValue = maxRate;
            if (slider.value > maxRate)
                slider.value = maxRate;
        }
    }
    #endregion

    #region Update ECS Data Functions
    //clear the simulation
    public void ClearSimulation() {
        spawnerSystem.Enabled = false;
        spawnerSystem.ClearSpawnQueue();
        clearSimSystem.ClearSimulation();

        StartCoroutine(WaitForAndThen(() => {
            spawnerSystem.Enabled = true;
        }, 1f));
    }
    private IEnumerator WaitForAndThen(Action action, float time) {
        yield return new WaitForSeconds(time);
        action();
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
        spawnData.spawnOne = spawnFlags[0];
        spawnData.spawnTwo = spawnFlags[1];
        spawnData.spawnThree = spawnFlags[2];
        spawnData.spawnFour = spawnFlags[3];

        spawnData.spawnRateOne = spawnRates[0];
        spawnData.spawnRateTwo = spawnRates[1];
        spawnData.spawnRateThree = spawnRates[2];
        spawnData.spawnRateFour = spawnRates[3];

        spawnData.velocityMax = maxVelocity;
        entityManager.SetComponentData(spawnDataEntity, spawnData);
    }
    //toggles on or off a spawner via toggle button on UI
    public void ChangeAutoSpawn(int index) {
        spawnFlags[index] = !spawnFlags[index];
        UpdateAutoSpawnData();
    }

    #endregion
}
