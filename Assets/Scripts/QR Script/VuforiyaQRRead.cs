using UnityEngine;
using TMPro;
using Vuforia;
using UnityEngine.UI;

public class VuforiyaQRRead : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI qrCodeText;
    public Button _lightBtn;

    private BarcodeBehaviour barcodeBehaviour;
    private string currentQRCode = "";

    void Start()
    {
        barcodeBehaviour = GetComponent<BarcodeBehaviour>();

        if (barcodeBehaviour == null)
        {
            Debug.LogError("BarcodeBehaviour component not found on this GameObject!");
            return;
        }

        Debug.Log("QR Scanner initialized. Point camera at a QR code.");

        if (qrCodeText != null)
        {
            qrCodeText.text = "Searching for QR code...";
        }
        _lightBtn.onClick.AddListener(ToggleFlashlight);
    }

    void Update()
    {
        if (barcodeBehaviour != null && barcodeBehaviour.InstanceData != null)
        {
            string detectedText = barcodeBehaviour.InstanceData.Text;

            if (!string.IsNullOrEmpty(detectedText))
            {
                if (detectedText != currentQRCode)
                {
                    currentQRCode = detectedText;
                    OnQRCodeDetected(detectedText);
                }
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(currentQRCode))
            {
                currentQRCode = "";
                OnQRCodeLost();
            }
        }
    }

    void OnQRCodeDetected(string qrText)
    {
        Debug.Log("=== QR CODE DETECTED ===");
        Debug.Log("QR Text: " + qrText);
        Debug.Log("=======================");

        if (qrCodeText != null)
        {
            qrCodeText.text = qrText;
        }
    }

    void OnQRCodeLost()
    {
        Debug.Log("QR code lost from view");

        if (qrCodeText != null)
        {
            qrCodeText.text = "Searching for QR code...";
        }
    }

    public string GetCurrentQRCode()
    {
        return currentQRCode;
    }

    public void GoBack()
    {
        if (isFlashOn)
        {
            ToggleFlashlight();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private bool isFlashOn = false;

    // Cal From Inspactor
    public void ToggleFlashlight()
    {
        isFlashOn = !isFlashOn;

        VuforiaBehaviour.Instance.CameraDevice.SetFlash(isFlashOn);
    }
}
