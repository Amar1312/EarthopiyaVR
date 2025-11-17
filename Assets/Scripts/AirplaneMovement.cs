using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;

public class AirplaneMovement : MonoBehaviour
{
    [Header("References")]
    public RectTransform mainImage;      // Parent container (map)
    public RectTransform airplane;       // Airplane RectTransform
    public Image _airplaneImage;

    public RectTransform pointB;         // End point
    public RectTransform MapcenterDestination;
    public RectTransform privacePoint;

    public float _nearDistance;
    public RectTransform jeep;
    public Image _jeepImage;

    [Header("Settings")]
    public float speed = 300f;           // Units per second, 300 = 1 Second 
    public float rotateDuration = 0.3f;  // Rotation speed

    [Header("Floating Animation")]
    public float floatDistance = 30f;     // How far past PointB the airplane floats
    public float floatDuration = 0.5f;

    public float floatJeepDistance = 30f;     // How far past PointB the airplane floats
    public float floatJeepDuration = 0.5f;

    [Header("Script Refference")]
    public SelectAnimalScroll _animalScroll;

    private Coroutine planeOff;
   
    private RectTransform currentMapDestination;
    private Vector2 mapStartPosition;
    private Vector2 mapTargetPosition;

    public List<RectTransform> _anglechange;

    public List<RectTransform> _firstPoint;
    public List<RectTransform> _secondPoint;

    void Start()
    {
        
    }

    public void SetDestinetion(RectTransform transform, RectTransform mapDestinetion)
    {
        StopAllAnimations();
        pointB = transform;
        currentMapDestination = mapDestinetion;
        StartAnimation();
    }

    void StopAllAnimations()
    {
        airplane.DOKill();
        jeep.DOKill();
        mainImage.DOKill();

        CancelInvoke(nameof(OffPlane));
        CancelInvoke(nameof(offJeep));

        if (planeOff != null)
        {
            StopCoroutine(planeOff);
            planeOff = null;
        }

        airplane.gameObject.SetActive(false);
        jeep.gameObject.SetActive(false);
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

        if (_anglechange[0] == privacePoint || _anglechange[1] == privacePoint)
        {
            adjustedAngle = adjustedAngle - 8f;
        }

        if ((_firstPoint[0] == privacePoint || _firstPoint[1] == privacePoint) && _secondPoint[0] == pointB)
        {
            adjustedAngle = adjustedAngle - 10f;
        }
        else if (_firstPoint[2] == privacePoint && _secondPoint[1] == pointB)
        {
            adjustedAngle = adjustedAngle + 20f;
        }

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
        if (_airplaneImage != null)
        {
            _airplaneImage.DOFade(1f, 0.4f).From(0f).SetEase(Ease.Linear);
        }
        //CalculatePadding();
        float distance = Vector2.Distance(airplane.anchoredPosition, targetPos);
        float moveDuration = distance / speed;
        MoveMapToCenter();
        airplane.DOAnchorPos(targetPos, 0.5f)
            .SetEase(Ease.Linear)
            //.OnUpdate(CheckVisibility2)
            .OnComplete(() =>
            {
                //airplane.gameObject.SetActive(true);
                StartFloatingAnimation();
            });
    }

    void MoveMapToCenter()
    {
        if (currentMapDestination == null || MapcenterDestination == null)
        {
            Debug.Log("Map destination or center destination is null. Skipping map movement.");
            return;
        }

        // Convert both points to world positions
        Vector3 worldDest = currentMapDestination.position;
        Vector3 worldCenter = MapcenterDestination.position;

        Vector3 offset = worldCenter - worldDest;

        // Move map smoothly so that mapDest aligns with mapCenter
        mainImage.DOMove(mainImage.position + offset, 0.5f).SetEase(Ease.Linear);

    }


    void StartFloatingAnimation()
    {
        // Get airplane's forward direction based on rotation
        Vector2 forward = Quaternion.Euler(0, 0, airplane.eulerAngles.z /*+ 90f*/) * Vector2.up;

        // Compute the forward float target point
        Vector2 floatTarget = (Vector2)airplane.anchoredPosition + forward * floatDistance;


        float floatDuration1 = 0.05f;

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
        privacePoint = pointB;
    }

    IEnumerator OffPlane1()
    {
        yield return new WaitForSeconds(0.5f);
        airplane.gameObject.SetActive(false);
    }

    public void StartJeepAnimation()
    {
        //CancelInvoke(nameof(offJeep));
        Vector2 forward = Quaternion.Euler(0, 0, jeep.eulerAngles.z) * Vector2.left;

        jeep.anchoredPosition = pointB.anchoredPosition + forward * floatJeepDistance;

        jeep.gameObject.SetActive(true);
        if (_jeepImage != null)
        {
            _jeepImage.DOFade(1f, 0.5f).From(0f).SetEase(Ease.Linear);
        }
        MoveMapToCenter();
        // Create ping-pong movement between PointB and floatTarget
        Sequence floatSeq = DOTween.Sequence();
        floatSeq.Append(jeep.DOAnchorPos(pointB.anchoredPosition, floatJeepDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            Invoke(nameof(offJeep), 0.4f);
            Debug.Log("jeep reached destination!");
        }));
    }

    void offJeep()
    {
        jeep.gameObject.SetActive(false);
        _animalScroll.OnDestination();
        privacePoint = pointB;
    }

}
