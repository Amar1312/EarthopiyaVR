//using UnityEngine;
//using UnityEngine.EventSystems;

//public class BlockScrollOnDrag : MonoBehaviour,
//    IBeginDragHandler, IDragHandler, IEndDragHandler
//{
//    //Just implement these so ScrollRect doesn't get the events
//    public void OnBeginDrag(PointerEventData eventData) { }

//    public void OnDrag(PointerEventData eventData) { }

//    public void OnEndDrag(PointerEventData eventData) { }
//}



using UnityEngine;
using UnityEngine.EventSystems;
using WPM;

public class BlockScrollOnDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WorldMapGlobe globe;
    private Transform globeTransform;
    private Camera mainCamera;
    private Vector2 lastPointerPosition;
    private bool isDragging;
    private int activePointerID = -1;

    public float rotationSpeed;

    public void SetData()
    {
        globe = FindFirstObjectByType<WorldMapGlobe>();
        if (globe == null)
        {
            Debug.LogError("WorldMapGlobe not found in scene!");
            return;
        }

        globeTransform = globe.transform;
        mainCamera = globe.mainCamera != null ? globe.mainCamera : Camera.main;

        Debug.Log("BlockScrollOnDrag initialized successfully!");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (globe == null)
        {
            Debug.LogWarning("Globe not initialized! Call SetData first.");
            return;
        }

        if (Input.touchCount > 1)
        {
            return;
        }

        isDragging = true;
        activePointerID = eventData.pointerId;
        lastPointerPosition = eventData.position;

        Debug.Log($"Drag started at: {eventData.position}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || globe == null || globeTransform == null || mainCamera == null)
        {
            return;
        }

        if (Input.touchCount > 1)
        {
            isDragging = false;
            return;
        }

        if (eventData.pointerId != activePointerID)
        {
            return;
        }

        rotationSpeed = Mathf.Lerp(0.001f, 0.15f, globe.GetZoomLevel());

        Vector2 currentPosition = eventData.position;
        Vector2 delta = currentPosition - lastPointerPosition;
        lastPointerPosition = currentPosition;

        float rotationX = -delta.x * rotationSpeed;
        float rotationY = -delta.y * rotationSpeed;

        if (globe.navigationMode == NAVIGATION_MODE.EARTH_ROTATES)
        {
            globeTransform.Rotate(mainCamera.transform.up, rotationX, Space.World);

            Vector3 axisY = Vector3.Cross(globeTransform.position - mainCamera.transform.position, mainCamera.transform.up);
            globeTransform.Rotate(axisY, rotationY, Space.World);
        }
        else
        {
            Transform pivotTransform = globe.pivotTransform;
            if (pivotTransform != null)
            {
                pivotTransform.RotateAround(globeTransform.position, mainCamera.transform.up, -rotationX);
                pivotTransform.RotateAround(globeTransform.position, mainCamera.transform.right, rotationY);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerID)
        {
            isDragging = false;
            activePointerID = -1;
            Debug.Log("Drag ended");
        }
    }
}
