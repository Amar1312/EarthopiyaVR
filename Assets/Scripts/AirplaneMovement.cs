using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;

public class AirplaneMovement : MonoBehaviour
{
    [Header("References")]
    public RectTransform mainImage;      // Parent container (map)
    public RectTransform airplane;       // Airplane RectTransform
    public RectTransform pointA;         // Start point
    public RectTransform pointB;         // End point

    public float _nearDistance;
    public RectTransform jeep;

    [Header("Settings")]
    public float speed = 300f;           // Units per second, 300 = 1 Second 
    public float rotateDuration = 0.3f;  // Rotation speed

    [Header("Screen Padding")]
    public float paddingTop = 50f;
    public float paddingBottom = 100f;
    public float paddingLeft = 50f;
    public float paddingRight = 50f;

    [Header("Floating Animation")]
    public float floatDistance = 30f;     // How far past PointB the airplane floats
    public float floatDuration = 0.5f;

    public float floatJeepDistance = 30f;     // How far past PointB the airplane floats
    public float floatJeepDuration = 0.5f;

    [Header("Script Refference")]
    public SelectAnimalScroll _animalScroll;

    private RectTransform canvasRect;
    private Coroutine planeOff;

    void Start()
    {
        canvasRect = mainImage.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        //MoveAirplane();
    }

    public void SetDestinetion(RectTransform transform)
    {
        pointB = transform;
        StartAnimation();
    }

    public void StartAnimation()
    {
        float distance = Vector2.Distance(airplane.anchoredPosition, pointB.anchoredPosition);
        float dis = Mathf.Abs(distance);
        if (dis > _nearDistance)
        {
            MoveAirplane();
        }
        else
        {
            StartJeepAnimation();
        }
    }

    public void MoveAirplane()
    {
        airplane.gameObject.SetActive(false);
        RotateToward(pointB.anchoredPosition, () =>
        {
            airplane.gameObject.SetActive(true);
            MoveAirplane(pointB.anchoredPosition);
        });
    }

    void RotateToward(Vector2 targetPos, TweenCallback onComplete = null)
    {
        // Direction and angle
        Vector2 dir = targetPos - airplane.anchoredPosition;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float adjustedAngle = angle - 90f; // assuming airplane points upward

        airplane.DORotate(new Vector3(0, 0, adjustedAngle), rotateDuration)
                .SetEase(Ease.OutSine)
                .OnComplete(onComplete);
    }

    void MoveAirplane(Vector2 targetPos)
    {
        //airplane.gameObject.SetActive(true);
        //if(planeOff != null)
        //{
        //    StopCoroutine(planeOff);
        //    planeOff = null;
        //}

        float distance = Vector2.Distance(airplane.anchoredPosition, targetPos);
        float moveDuration = distance / speed;

        airplane.DOAnchorPos(targetPos, moveDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(CheckVisibility)
            .OnComplete(() =>
            {
                //airplane.gameObject.SetActive(true);
                StartFloatingAnimation();
            });
    }
    void StartFloatingAnimation()
    {
        // Get airplane's forward direction based on rotation
        Vector2 forward = Quaternion.Euler(0, 0, airplane.eulerAngles.z /*+ 90f*/) * Vector2.up;

        // Compute the forward float target point
        Vector2 floatTarget = (Vector2)airplane.anchoredPosition + forward * floatDistance;


        float floatDuration1 = floatDistance / speed;

        // Create ping-pong movement between PointB and floatTarget
        Sequence floatSeq = DOTween.Sequence();
        floatSeq.Append(airplane.DOAnchorPos(floatTarget, floatDuration1).SetEase(Ease.Linear))
                .Append(airplane.DOAnchorPos(pointB.anchoredPosition, floatDuration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    Invoke(nameof(OffPlane), 0.5f);
                    //planeOff = StartCoroutine(OffPlane1());
                    Debug.Log("Airplane reached destination!");
                }));

    }

    void OffPlane()
    {
        airplane.gameObject.SetActive(false);
        _animalScroll.OnDestination();
    }

    IEnumerator OffPlane1()
    {
        yield return new WaitForSeconds(0.5f);
        airplane.gameObject.SetActive(false);
    }

    void CheckVisibility()
    {
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3[] planeCorners = new Vector3[4];
        airplane.GetWorldCorners(planeCorners);

        // Create screen rect with individual paddings
        Rect screenRect = new Rect(
            canvasCorners[0].x + paddingLeft,
            canvasCorners[0].y + paddingBottom,
            canvasRect.rect.width - (paddingLeft + paddingRight),
            canvasRect.rect.height - (paddingTop + paddingBottom)
        );

        bool isInside = screenRect.Contains(planeCorners[0]) &&
                        screenRect.Contains(planeCorners[2]);

        if (!isInside)
        {
            KeepPlaneOnScreen();
        }
    }

    void KeepPlaneOnScreen()
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, airplane.position);

        Vector2 offset = Vector2.zero;
        if (screenPos.x < paddingLeft) offset.x = paddingLeft - screenPos.x;
        else if (screenPos.x > Screen.width - paddingRight) offset.x = (Screen.width - paddingRight) - screenPos.x;

        if (screenPos.y < paddingBottom) offset.y = paddingBottom - screenPos.y;
        else if (screenPos.y > Screen.height - paddingTop) offset.y = (Screen.height - paddingTop) - screenPos.y;

        mainImage.DOAnchorPos(mainImage.anchoredPosition + offset, 0.2f).SetEase(Ease.OutQuad);
    }

    public void StartJeepAnimation()
    {
        //CancelInvoke(nameof(offJeep));
        Vector2 forward = Quaternion.Euler(0, 0, jeep.eulerAngles.z) * Vector2.left;

        jeep.anchoredPosition = pointB.anchoredPosition + forward * floatJeepDistance;

        jeep.gameObject.SetActive(true);
        // Create ping-pong movement between PointB and floatTarget
        Sequence floatSeq = DOTween.Sequence();
        floatSeq.Append(jeep.DOAnchorPos(pointB.anchoredPosition, floatJeepDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            Invoke(nameof(offJeep), 0.5f);
            Debug.Log("jeep reached destination!");
        }));
    }

    void offJeep()
    {
        jeep.gameObject.SetActive(false);
        _animalScroll.OnDestination();
    }
}
