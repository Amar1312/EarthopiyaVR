using UnityEngine;
using UnityEngine.UI;

public class CameraAspectFitter : MonoBehaviour
{
    private RawImage rawImage;
    private AspectRatioFitter aspectRatioFitter;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();

        UpdateAspectRatio();
    }

    void UpdateAspectRatio()
    {
        if (rawImage.texture != null)
        {
            float textureAspect = (float)rawImage.texture.width / rawImage.texture.height;
            aspectRatioFitter.aspectRatio = textureAspect;
        }
    }

    void Update()
    {
        if (rawImage.texture != null && aspectRatioFitter.aspectRatio != (float)rawImage.texture.width / rawImage.texture.height)
        {
            UpdateAspectRatio();
        }
    }
}
