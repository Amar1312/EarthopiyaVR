using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class HardwareFlashlightToggle : MonoBehaviour
{
    [Header("UI References")]
    public Button flashButton;
    public Text flashButtonText;

    [Header("QR Scanner")]
    public ReadQRCode qrScanner;

    private bool isFlashOn = false;

#if UNITY_ANDROID
    private AndroidJavaObject cameraObject;
    private AndroidJavaObject parametersObject;
    private bool isInitialized = false;
#endif

    void Start()
    {
        if (flashButton != null)
        {
            flashButton.onClick.AddListener(ToggleFlashlight);
        }

        if (qrScanner == null)
        {
            qrScanner = FindObjectOfType<ReadQRCode>();
        }

        UpdateButtonText();
    }

    public void ToggleFlashlight()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            Debug.Log("Camera permission required");
            return;
        }

        if (!isFlashOn)
        {
            TurnOnFlashlight();
        }
        else
        {
            TurnOffFlashlight();
        }
#else
        Debug.LogWarning("Hardware flashlight only supported on Android");
#endif
    }

#if UNITY_ANDROID
    void TurnOnFlashlight()
    {
        if (qrScanner != null)
        {
            qrScanner.StopScanning();
        }

        StartCoroutine(InitializeAndTurnOnFlash());
    }

    System.Collections.IEnumerator InitializeAndTurnOnFlash()
    {
        yield return new WaitForSeconds(0.5f);

        try
        {
            if (cameraObject == null)
            {
                AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
                cameraObject = cameraClass.CallStatic<AndroidJavaObject>("open", 0);
            }

            if (cameraObject != null)
            {
                parametersObject = cameraObject.Call<AndroidJavaObject>("getParameters");

                AndroidJavaObject supportedFlashModes = parametersObject.Call<AndroidJavaObject>("getSupportedFlashModes");

                if (supportedFlashModes != null)
                {
                    parametersObject.Call("setFlashMode", "torch");
                    cameraObject.Call("setParameters", parametersObject);
                    cameraObject.Call("startPreview");

                    isFlashOn = true;
                    isInitialized = true;
                    Debug.Log("Hardware flashlight turned ON");
                }
                else
                {
                    Debug.LogError("Device does not support flash");
                    ReleaseCameraResources();

                    if (qrScanner != null)
                    {
                        qrScanner.ResumeScanning();
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to turn on flashlight: " + e.Message);
            ReleaseCameraResources();

            if (qrScanner != null)
            {
                qrScanner.ResumeScanning();
            }
        }

        UpdateButtonText();
    }

    void TurnOffFlashlight()
    {
        try
        {
            if (cameraObject != null && parametersObject != null)
            {
                parametersObject.Call("setFlashMode", "off");
                cameraObject.Call("setParameters", parametersObject);
                cameraObject.Call("stopPreview");

                Debug.Log("Hardware flashlight turned OFF");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error turning off flashlight: " + e.Message);
        }
        finally
        {
            ReleaseCameraResources();
            isFlashOn = false;

            if (qrScanner != null)
            {
                StartCoroutine(ResumeQRScannerAfterDelay());
            }

            UpdateButtonText();
        }
    }

    System.Collections.IEnumerator ResumeQRScannerAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (qrScanner != null)
        {
            qrScanner.ResumeScanning();
        }
    }

    void ReleaseCameraResources()
    {
        if (cameraObject != null)
        {
            try
            {
                cameraObject.Call("release");
                cameraObject.Dispose();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error releasing camera: " + e.Message);
            }

            cameraObject = null;
            parametersObject = null;
        }

        isInitialized = false;
    }
#endif

    void UpdateButtonText()
    {
        if (flashButtonText != null)
        {
            flashButtonText.text = isFlashOn ? "Flash OFF" : "Flash ON";
        }
    }

    void OnDestroy()
    {
        if (isFlashOn)
        {
#if UNITY_ANDROID
            TurnOffFlashlight();
#endif
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isFlashOn)
        {
#if UNITY_ANDROID
            TurnOffFlashlight();
#endif
        }
    }

    void OnApplicationQuit()
    {
        if (isFlashOn)
        {
#if UNITY_ANDROID
            TurnOffFlashlight();
#endif
        }
    }
}
