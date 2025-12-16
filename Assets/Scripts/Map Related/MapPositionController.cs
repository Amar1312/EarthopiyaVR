using UnityEngine;

public class MapPositionController : MonoBehaviour
{
    [Header("References")]
    public RectTransform mapImageRect;
    public Transform mapAllTransform;
    public Camera mainCamera;

    [Header("Settings")]
    public float depthFromCamera = 10f;
    public Vector3 offset = Vector3.zero;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        SyncMapAllPosition();
    }

    private void SyncMapAllPosition()
    {
        if (mapImageRect == null || mapAllTransform == null || mainCamera == null)
            return;

        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(null, mapImageRect.position);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            depthFromCamera
        ));

        mapAllTransform.position = worldPosition + offset;
    }
}

