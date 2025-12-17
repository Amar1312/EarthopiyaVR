using UnityEngine;
using UnityEngine.UI;


public class ScrollViewVisibilityDetector : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform targetImage;
    public RectTransform targetImage2;
    public RectTransform viewport;

    private bool isVisible = false;
    private bool isVisible2 = false;
    public AnimalSafariPanel _animalScript;
    public BlockScrollOnDrag _blockScroll;

    private void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }


    private void OnDisable()
    {
        if (_animalScript._mapObj.activeInHierarchy)
        {
            _animalScript.OffMapBox();
        }
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnScrollChanged(Vector2 scrollPosition)
    {
        CheckVisibility();
    }

    void CheckVisibility()
    {
        if (IsRectTransformInside(targetImage, viewport))
        {
            if (!isVisible)
            {
                isVisible = true;
                OnBecameVisible1();
            }
        }
        else
        {
            if (isVisible)
            {
                isVisible = false;
            }
        }

        if (IsRectTransformInside(targetImage2, viewport))
        {
            if (!isVisible2)
            {
                isVisible2 = true;
                OnBecameVisible2();
            }
        }
        else
        {
            if (isVisible2)
            {
                isVisible2 = false;
            }
        }
    }

    bool IsRectTransformInside(RectTransform child, RectTransform parent)
    {
        Vector3[] childCorners = new Vector3[4];
        Vector3[] parentCorners = new Vector3[4];

        child.GetWorldCorners(childCorners);
        parent.GetWorldCorners(parentCorners);

        if (childCorners[2].y < parentCorners[0].y ||
            childCorners[0].y > parentCorners[2].y)
        {
            return false;
        }

        if (childCorners[2].x < parentCorners[0].x ||
            childCorners[0].x > parentCorners[2].x)
        {
            return false;
        }

        return true;
    }

    void OnBecameVisible1()
    {
        Debug.Log($"{targetImage.name} Visible");
        if (!_animalScript._mapObj.activeInHierarchy)
        {
            _animalScript.OnMapBox();
            _blockScroll.SetData();
        }
    }

    void OnBecameVisible2()
    {
        Debug.Log($"{targetImage2.name} Visible");
        if (_animalScript._mapObj.activeInHierarchy)
        {
            _animalScript.OffMapBox();
        }
    }
}
