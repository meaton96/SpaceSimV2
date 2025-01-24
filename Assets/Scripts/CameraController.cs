using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction zoomAction;
    private InputAction panAction;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;      
    public float minZoomDistance = -70f; 
    public float maxZoomDistance = -875;
    [SerializeField] private float zoomFactor;
    [Header("Pan Settings")]
    public float panSpeed = 0.1f;

    [Header("Mobile Slider")]
    [SerializeField] private Slider zoomSlider;

    private bool isMobile => SimulationController.IsMobile;


    private Vector3 lastPointerPos;
    private void Awake() {
        if (!Mouse.current.enabled) {
            InputSystem.EnableDevice(Mouse.current);
            Debug.Log("Mouse enabled.");
        }
    }
    private void Start() {
        var playerMap = inputActions.FindActionMap("Player");
        zoomAction = playerMap.FindAction("Zoom");
        zoomAction?.Enable();
        panAction?.Enable();
        lastPointerPos = Vector3.zero;

        panAction = playerMap.FindAction("Pan");


        if (isMobile) {
            zoomSlider.value = (transform.position.z - minZoomDistance) / (maxZoomDistance - minZoomDistance);
            zoomSlider.onValueChanged.AddListener(val => HandleMobileZoom(val));
        }
    }
    private void OnDisable() {
        zoomAction?.Disable();
        panAction?.Disable();
    }

    void Update() {
        HandlePan();
        if (isMobile) {
            return;
        }
        HandleZoom();
        
    }

    private void HandleMobileZoom(float sliderValue) {

        float cameraPosition = Mathf.Lerp(minZoomDistance, maxZoomDistance, sliderValue);
        transform.position = new Vector3(transform.position.x, transform.position.y, cameraPosition);
        zoomFactor = sliderValue * 10;

    }
    
    //Handle Zoom input from mouse scroll wheel
    private void HandleZoom() {
        Vector2 scrollInput = zoomAction.ReadValue<Vector2>();
        float scrollY = scrollInput.y;

        if (Mathf.Abs(scrollY) > 0.01f) {
            Vector3 direction = transform.forward * scrollY * zoomSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + direction;

            newPosition.z = Mathf.Clamp(newPosition.z, maxZoomDistance, minZoomDistance);

            transform.position = newPosition;
        }
        //set zoom factor to the % zoom distance between min and max
        zoomFactor = (transform.position.z - minZoomDistance) / (maxZoomDistance - minZoomDistance) * 10;
    }


    private void HandlePan() {
        //if windows
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
            if (!Mouse.current.leftButton.isPressed) {
                return;
            }
        }
        
        Vector2 delta = panAction.ReadValue<Vector2>();
        Debug.Log(delta.sqrMagnitude);
        if (delta.sqrMagnitude > 0.001f) {

            float adjustedPanSpeed = panSpeed * Mathf.Max(1f, zoomFactor);
            Vector3 panDirection = new Vector3(-delta.x * adjustedPanSpeed,
                                               -delta.y * adjustedPanSpeed,
                                                0f);

            transform.Translate(panDirection, Space.Self);
        }
    }
    
}
