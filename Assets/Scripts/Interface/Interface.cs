using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Interface : MonoBehaviour {

    
    

    [Header("Simulation Controls")]


    public Slider[] sliders = new Slider[4];
    public Toggle[] sizeRadios = new Toggle[4];

    public TextMeshProUGUI[] spawnRateTexts = new TextMeshProUGUI[4];

    public TextMeshProUGUI velocityText;
    public Slider velocitySlider;
    public TextMeshProUGUI pauseButtonText;

    [Header("Stats Text")]
    public TextMeshProUGUI entityCountText;
    public TextMeshProUGUI entitySpawnDestroyText;
    public TextMeshProUGUI boundarySizeText;

    [Header("Crash and Lag")]
    public TextMeshProUGUI frameTimeWarningText;
    public GameObject crashMessageParent;

    [Header("Mobile Only")]
    public GameObject statsTextParent;
    public Toggle toggleStats;
}
