using UnityEngine;
using DG.Tweening;
using UnityEngine.Video;

public class TicketSplashScreen : MonoBehaviour
{
    [Header("Positions")]
    public float startX = 0f;
    public float leftX = -840f;
    public float rightX = 840f;
    public Transform _airplane;
    public Transform _extraObject;
    public GameObject _whiteScreen;
    public VideoPlayer _tickerVideo;

    [Header("Timings")]
    public float moveDuration = 1.0f; // time to move from one side to the other (linear)

    private Sequence seq;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        UIManager.instance._splaceScreenVideo.SetDirectAudioMute(0,true);
        // Ensure DOTween is initialized
        DOTween.Init(false, true);

        // Set initial position explicitly to startX
        Vector3 pos = _airplane.localPosition;
        pos.x = startX;
        _airplane.localPosition = pos;
        StartLoop();
    }

    public void Update()
    {
        if (_tickerVideo.frame > 2f)
        {
            _whiteScreen.SetActive(false);

        }
    }

    void OnDisable()
    {
        _whiteScreen.SetActive(true);
        // Clean up sequence when the object is disabled/destroyed
        //if (seq != null && seq.IsActive()) seq.Kill();
    }


    void StartLoop()
    {
        // kill existing sequence if any
        if (seq != null && seq.IsActive()) seq.Kill();

        seq = DOTween.Sequence();
        //seq.AppendInterval(2f);
        AppendFloatMotion(seq, 2f);

        seq.Append(_airplane.DOLocalMoveX(leftX, moveDuration).SetEase(Ease.OutQuad));

        //  Immediately jump to 840
        seq.AppendCallback(() =>
        {
            Vector3 p = _airplane.localPosition;
            p.x = rightX;
            _airplane.localPosition = p;
        });

        //  Move 840 -> -840
        seq.Append(_airplane.DOLocalMoveX(startX, moveDuration).SetEase(Ease.OutQuad));
        //seq.AppendInterval(2f);
        AppendFloatMotion(seq, 2f);
        seq.Append(_airplane.DOLocalMoveX(leftX, moveDuration).SetEase(Ease.OutQuad));

        //  Immediately jump to 840 (so loop continues with the same pattern)
        seq.AppendCallback(() =>
        {
            Vector3 p = _airplane.localPosition;
            p.x = rightX;
            _airplane.localPosition = p;
        });

        seq.Append(_airplane.DOLocalMoveX(startX, moveDuration).SetEase(Ease.OutQuad));

        // Loop forever
        seq.SetLoops(-1, LoopType.Restart);
    }

    void AppendFloatMotion(Sequence sequence, float duration)
    {
        float floatAmount = 1.5f;
        float floatSpeed = 0.3f;
        float startY = _airplane.localPosition.y;

        // --- AIRPLANE FLOAT ---
        Sequence airplaneSeq = DOTween.Sequence();
        airplaneSeq.Append(_airplane.DOLocalMoveY(startY + floatAmount, floatSpeed).SetEase(Ease.InOutSine))
                   .Append(_airplane.DOLocalMoveY(startY - floatAmount, floatSpeed).SetEase(Ease.InOutSine))
                   .SetLoops(Mathf.RoundToInt(duration / (floatSpeed * 2)), LoopType.Yoyo);

        // --- EXTRA OBJECT MOTION ---
        Sequence extraSeq = null;
        if (_extraObject != null)
        {
            float extraMoveDistance = 10f; // distance between two points
            float extraSpeed = 0.1f;      // speed of movement
            Vector3 startPos = _extraObject.localPosition;
            Vector3 endPos = startPos + new Vector3(extraMoveDistance, 0f, 0f);

            extraSeq = DOTween.Sequence();
            extraSeq.Append(_extraObject.DOLocalMove(endPos, extraSpeed).SetEase(Ease.InOutSine))
                    .Append(_extraObject.DOLocalMove(startPos, extraSpeed).SetEase(Ease.InOutSine))
                    .SetLoops(Mathf.RoundToInt(duration / (extraSpeed * 2)), LoopType.Yoyo);
        }

        // --- RUN BOTH PARALLEL FOR "duration" SECONDS ---
        if (extraSeq != null)
            sequence.Append(airplaneSeq).Join(extraSeq);  // both play together
        else
            sequence.Append(airplaneSeq); // only airplane float

        // Wait exactly "duration" before continuing main sequence
        sequence.AppendInterval(0f);
    }
}
