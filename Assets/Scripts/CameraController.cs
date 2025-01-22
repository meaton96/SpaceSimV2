using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class CameraController : MonoBehaviour {

    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction zoomAction;
    private InputAction panAction;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;      
    public float minZoomDistance = -70f; 
    public float maxZoomDistance = -875; 

    [Header("Pan Settings")]
    public float panSpeed = 0.1f;      
   // public KeyCode panKey = KeyCode.Mouse1;

    [SerializeField] private float zoomFactor;
    private Vector3 lastPointerPos;
    private void Awake() {
        if (!Mouse.current.enabled) {
            InputSystem.EnableDevice(Mouse.current);
            Debug.Log("Mouse enabled.");
        }
    }
    private void Start() {
        EnhancedTouchSupport.Enable();
        var playerMap = inputActions.FindActionMap("Player");
        zoomAction = playerMap.FindAction("Zoom");
        zoomAction?.Enable();
        panAction?.Enable();
        lastPointerPos = Vector3.zero;
        //zoomAction.performed += ctx => {
        //    Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        //    Debug.Log($"Scroll wheel delta: {scrollDelta}");
        //};

        panAction = playerMap.FindAction("Pan");
    }
    private void OnDisable() {
        zoomAction?.Disable();
        panAction?.Disable();
    }

    void Update() {
        HandleZoom();
        HandlePan();
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
       // Debug.Log(delta.sqrMagnitude);
        if (delta.sqrMagnitude > 0.001f) {

            float adjustedPanSpeed = panSpeed * Mathf.Max(1f, zoomFactor);
            Vector3 panDirection = new Vector3(-delta.x * adjustedPanSpeed,
                                               -delta.y * adjustedPanSpeed,
                                                0f);

            transform.Translate(panDirection, Space.Self);
        }
    }
    // Basic pinch logic using EnhancedTouch for multiple touches
    private void HandlePinch() {
        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 2)
            return; 

        
        var touch0 = activeTouches[0];
        var touch1 = activeTouches[1];

        float currentDist = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

        Vector2 prevPos0 = touch0.screenPosition - touch0.delta;
        Vector2 prevPos1 = touch1.screenPosition - touch1.delta;
        float prevDist = Vector2.Distance(prevPos0, prevPos1);

        float pinchDelta = currentDist - prevDist;

        if (Mathf.Abs(pinchDelta) > 0.001f) {
            Vector3 direction = transform.forward * (pinchDelta * 0.01f) * zoomSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + direction;
            newPosition.z = Mathf.Clamp(newPosition.z, maxZoomDistance, minZoomDistance);
            transform.position = newPosition;
        }
    }
}
