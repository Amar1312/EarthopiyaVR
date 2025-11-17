using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class AirplaneMovement2 : MonoBehaviour
{
    [Header("References")]
    public RectTransform mainImage;
    public RectTransform airplane;
    public List<RectTransform> waypoints;

    [Header("Scroll Control")]
    public Scrollbar locationScroll;
    public bool isInfiniteScroll = false; //  toggle for infinite scroll

    [Header("Movement Settings")]
    public float rotateDuration = 0.3f;

    [Header("Script Reference")]
    public SelectAnimalScroll _animalScroll;

    private RectTransform canvasRect;
    private float lastScrollValue;
    private int loopCount = 0;

    void Start()
    {
        if (mainImage == null || airplane == null || locationScroll == null || waypoints.Count < 2)
        {
            Debug.LogError("AirplaneMovementInfinite: Missing references!");
            enabled = false;
            return;
        }

        canvasRect = mainImage.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        locationScroll.onValueChanged.AddListener(OnScrollChanged);

        airplane.anchoredPosition = waypoints[0].anchoredPosition;
        RotateToward(waypoints[1].anchoredPosition);
    }

    void OnScrollChanged(float value)
    {
        if (isInfiniteScroll)
            HandleInfiniteScroll(value );
        else
            MovePlaneAccordingToScroll(value);

        lastScrollValue = value;
    }

    void HandleInfiniteScroll(float value)
    {
        // Detect loop wrap (1 -> 0 or 0 -> 1)
        if (value < lastScrollValue && Mathf.Abs(lastScrollValue - value) > 0.5f)
            loopCount++;
        else if (value > lastScrollValue && Mathf.Abs(lastScrollValue - value) > 0.5f)
            loopCount--;

        float effectiveValue = value + loopCount;
        MovePlaneAccordingToScroll(effectiveValue % 1f); // mod 1f keeps within 0–1 range
    }

    void MovePlaneAccordingToScroll(float value)
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

        int totalPoints = 11; // 11
        int segmentCount = totalPoints - 1; // 10 segments between 11 points
        float stepSize = 1f / segmentCount; // each step = 0.1 if 11 waypoints

        // ✅ Determine which segment (between which two waypoints) we are in
        int currentIndex = Mathf.FloorToInt(value / stepSize);
        currentIndex = Mathf.Clamp(currentIndex, 0, totalPoints - 2);
        int nextIndex = currentIndex + 1;

        // ✅ Find interpolation (0–1) between these two points
        float segmentStartValue = currentIndex * stepSize;
        float segmentEndValue = segmentStartValue + stepSize;
        float t = Mathf.InverseLerp(segmentStartValue, segmentEndValue, value);

        // ✅ Calculate airplane position between currentIndex and nextIndex
        Vector2 start = waypoints[currentIndex].anchoredPosition;
        Vector2 end = waypoints[nextIndex].anchoredPosition;
        Vector2 newPos = Vector2.Lerp(start, end, t);
        airplane.anchoredPosition = newPos;

        // ✅ Rotate airplane smoothly toward direction
        Vector2 dir = (end - start).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        airplane.rotation = Quaternion.Euler(0, 0, angle); // instant rotation


        // ✅ Optional: detect when reaching destination
        if (t >= 0.99f && nextIndex == totalPoints - 1)
        {
            _animalScroll?.OnDestination();
        }

        // Debug info
        Debug.Log($"Scroll Value: {value:F2} | Segment: {currentIndex} → {nextIndex} | t: {t:F2}");
    }


    void RotateToward(Vector2 targetPos)
    {
        Vector2 dir = targetPos - airplane.anchoredPosition;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        airplane.rotation = Quaternion.Euler(0, 0, angle);
    }
}
