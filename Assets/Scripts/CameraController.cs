using Unity.Mathematics;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;      
    public float minZoomDistance = -70f; 
    public float maxZoomDistance = -875; 

    [Header("Pan Settings")]
    public float panSpeed = 0.1f;      
    public KeyCode panKey = KeyCode.Mouse1;

    [SerializeField] private float zoomFactor;
    private Vector3 lastMousePosition; 

    void Update() {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom() {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0) {
            Vector3 direction = transform.forward * scrollInput * zoomSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + direction;

           newPosition.z = Mathf.Clamp(newPosition.z, maxZoomDistance, minZoomDistance);

            transform.position = newPosition;
            
        }


        zoomFactor = (transform.position.z - minZoomDistance) /
            (maxZoomDistance - minZoomDistance) * 10;
    }

    private void HandlePan() {
        if (Input.GetKey(panKey)) {
            if (Input.GetMouseButtonDown(1)) {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1)) {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                float adjustedPanSpeed = panSpeed * Mathf.Max(1f, zoomFactor);
                Vector3 panDirection = new Vector3(-delta.x * adjustedPanSpeed, -delta.y * adjustedPanSpeed, 0);
                transform.Translate(panDirection, Space.Self);

                lastMousePosition = Input.mousePosition;
            }
        }
    }
}
