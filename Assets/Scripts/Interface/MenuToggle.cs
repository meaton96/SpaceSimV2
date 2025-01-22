using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class MenuToggle : MonoBehaviour {
    [Header("Positions")]
    [SerializeField] private Transform openPosition;   // Assign from inspector
    [SerializeField] private Transform closedPosition; // Assign from inspector

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI buttonText;          // Assign the Text component of your button

    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.3f; // How long it takes to move

    private bool isOpen = true;  // Menu starts in open state
    private Coroutine moveRoutine;

    private void Start() {
        // Ensure the menu starts at the open position
        transform.position = openPosition.position;
        buttonText.text = "<"; // Indicate that pressing will close (arrow pointing left)
    }

    /// <summary>
    /// Call this function from your button's OnClick event in the inspector.
    /// It toggles the menu between the open and closed positions.
    /// </summary>
    public void ToggleMenu() {
        // If the menu is mid-move, stop it
        if (moveRoutine != null) {
            StopCoroutine(moveRoutine);
        }

        // Decide target positions based on current state
        Vector3 startPos = transform.position;
        Vector3 endPos = isOpen ? closedPosition.position : openPosition.position;

        // Swap the button text right away for snappy feedback
        isOpen = !isOpen;
        buttonText.text = isOpen ? "<" : ">";

        // Start the movement coroutine
        moveRoutine = StartCoroutine(MoveMenu(startPos, endPos));
    }

    private IEnumerator MoveMenu(Vector3 startPos, Vector3 endPos) {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            // Cubic smoothing (ease in-out)
            float smoothT = t * t * (3f - 2f * t);

            // Move
            transform.position = Vector3.Lerp(startPos, endPos, smoothT);

            yield return null;
        }

        // Finalize position
        transform.position = endPos;
        moveRoutine = null;
    }
}
