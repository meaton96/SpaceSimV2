using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Interface : MonoBehaviour {
    public TextMeshProUGUI frameTimeWarningText;
    public TextMeshProUGUI pauseButtonText;
    public GameObject crashMessageParent;
    public TextMeshProUGUI entityCountText;
    public TextMeshProUGUI entitySpawnDestroyText;
    public TextMeshProUGUI boundarySizeText;

    public Slider[] sliders = new Slider[4];

    public Toggle[] sizeRadios = new Toggle[4];

    public TextMeshProUGUI[] spawnRateTexts = new TextMeshProUGUI[4];

    

    public TextMeshProUGUI velocityText;
    public Slider velocitySlider;
}
