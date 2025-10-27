using UnityEngine;

public class RectTouchDetector : MonoBehaviour
{
    public RectTransform targetRect; // assign in Inspector
    public Camera uiCamera; // assign your UI camera (usually Canvas camera)

    void Update()
    {
        // Detect mouse click or touch
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            if (RectTransformUtility.RectangleContainsScreenPoint(targetRect, mousePos, uiCamera))
            {
                Debug.Log("Clicked inside the RectTransform!");
            }
            else
            {
                Debug.Log("Clicked outside the RectTransform!");
            }
        }

        // Optional: For mobile touch input
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.touches[0].position;

            if (RectTransformUtility.RectangleContainsScreenPoint(targetRect, touchPos, uiCamera))
            {
                Debug.Log("Touch inside RectTransform!");
            }
            else
            {
                Debug.Log("Touch outside RectTransform!");
            }
        }
    }
}
