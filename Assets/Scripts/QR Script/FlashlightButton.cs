using UnityEngine;
using UnityEngine.UI;

public class FlashlightButton : MonoBehaviour
{
    [Header("UI Reference")]
    public Button flashlightButton;
    public Text buttonText;

    [Header("QR Scanner")]
    public ReadQRCode qrScanner;

    private bool isFlashlightOn = false;

    void Start()
    {
        if (flashlightButton != null)
        {
            flashlightButton.onClick.AddListener(OnButtonClick);
        }

        if (qrScanner == null)
        {
            qrScanner = FindObjectOfType<ReadQRCode>();
        }

        UpdateButtonText();
    }

    void OnButtonClick()
    {
#if UNITY_ANDROID
        if (qrScanner != null)
        {
            UpdateButtonText();
        }
        else
        {
            Debug.LogError("QR Scanner reference not found!");
        }
#elif UNITY_IOS
        isFlashlightOn = !isFlashlightOn;
        FlashlightPlugin.SetTorchEnabled(isFlashlightOn);
        UpdateButtonText();
#else
        Debug.LogWarning("Flashlight only supported on Android/iOS");
#endif
    }

    void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = isFlashlightOn ? "Flash OFF" : "Flash ON";
        }
    }

}
