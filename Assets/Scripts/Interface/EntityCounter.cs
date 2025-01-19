using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class EntityCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI entityCountText;
    [SerializeField]
    private TextMeshProUGUI entitySpawnDestroyText;
    //[SerializeField]
    //private TextMeshProUGUI entityDestroyedText;

    private float updateTimer;
    [SerializeField] private float updateInterval = .02f;

    private float totalTime;
    private float totalFrames;

    private int[] previousAmounts = new int[4];

    private EntityManager entityManager;
    private Entity counterEntity;
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        counterEntity = entityManager.CreateEntityQuery(typeof(EntityCounterComponent)).GetSingletonEntity();
    }
    // Update is called once per frame
    void Update()
    {
        updateTimer += Time.deltaTime;
        totalTime += Time.deltaTime;
        totalFrames++;
        

        

        if (updateTimer > updateInterval) {

            EntityCounterComponent counterComponent = entityManager.GetComponentData<EntityCounterComponent>(counterEntity);



            entityCountText.text = $"Object 1: {counterComponent.TypeOneCount}\n" +
                                   $"Object 2: {counterComponent.TypeTwoCount}\n" +
                                   $"Object 3: {counterComponent.TypeThreeCount}\n" +
                                   $"Object 4: {counterComponent.TypeFourCount}\n";

            entitySpawnDestroyText.text = $"Sim Spawns: \n{counterComponent.totalSpawnedBySimulator}\n" +
                                          $"Col Spawns: \n{counterComponent.totalSpawnedByCollisions}\n" +
                                          $"Destroyed: \n{counterComponent.totalDestroyed}";

            //entitySpawnedText.text = $"Spawns:\n" +
            //                         $"{0} / frame\n" +
            //                         $"{0} / second\n" +
            //                         $"{0} / minute";

            //entityDestroyedText.text = $"Destroys:\n" +
            //                         $"{0} / frame\n" +
            //                         $"{0} / second\n" +
            //                         $"{0} / minute";
        }
    }
}
