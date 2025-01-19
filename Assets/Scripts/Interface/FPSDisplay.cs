using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI fpsText;

    private float deltaTime;
    private float updateInterval = 0.1f;
    private float updateTimer;

    private float totalTime;
    private int frameCount;

    private float low = float.MaxValue, high;

    void Update() {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        updateTimer += Time.deltaTime;
        totalTime += Time.deltaTime;
        frameCount++;

        if (updateTimer > updateInterval) {

            float fps = 1.0f / deltaTime;
            if (fps < low) {
                low = fps;
            }
            if (fps > high) {
                high = fps;
            }
            updateTimer = 0;
            if (totalTime != 0) {
                float avgFps = frameCount / totalTime;
                fpsText.text = $"FPS: {Mathf.Ceil(fps)}\nAvg: {Mathf.Ceil(avgFps)}\n" +
                               $"Low: {Mathf.Ceil(low)}\nHigh: {Mathf.Ceil(high)}";
            }
            

        }






    }
}
