//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Android;

//public class FlashlightWithCameraControl : MonoBehaviour
//{
//    [Header("UI Reference")]
//    public Button flashlightButton;
//    public Text buttonText;

//    [Header("QR Scanner Reference")]
//    public ReadQRCode qrScanner;

//    private bool isFlashlightOn = false;

//#if UNITY_ANDROID
//    private AndroidJavaObject flashlightPlugin;
//    private bool isInitialized = false;
//#endif

//    void Start()
//    {
//        if (flashlightButton != null)
//        {
//            flashlightButton.onClick.AddListener(OnFlashlightButtonClicked);
//        }

//        //if (qrScanner == null)
//        //{
//        //    qrScanner = FindObjectOfType<ReadQRCode>();
//        //}

//        UpdateButtonText();

//#if UNITY_ANDROID
//        StartCoroutine(InitializeAfterDelay());
//#endif
//    }

//    System.Collections.IEnumerator InitializeAfterDelay()
//    {
//        yield return new WaitForSeconds(0.5f);
//        InitializeFlashlight();
//    }

//#if UNITY_ANDROID
//    void InitializeFlashlight()
//    {
//        try
//        {
//            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

//            flashlightPlugin = new AndroidJavaObject("com.unity.flashlight.FlashlightPlugin", currentActivity);
//            isInitialized = flashlightPlugin.Call<bool>("initializeFlashlight");

//            if (isInitialized)
//            {
//                Debug.Log("Flashlight plugin initialized successfully.");
//            }
//            else
//            {
//                Debug.LogWarning("Device does not support flashlight or API level too low.");
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("Failed to initialize flashlight plugin: " + e.Message);
//            isInitialized = false;
//        }
//    }
//#endif

//    void OnFlashlightButtonClicked()
//    {
//#if UNITY_ANDROID
//        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
//        {
//            Permission.RequestUserPermission(Permission.Camera);
//            Debug.Log("Camera permission required for flashlight.");
//            return;
//        }

//        if (!isInitialized)
//        {
//            InitializeFlashlight();
//            if (!isInitialized)
//            {
//                Debug.LogWarning("Flashlight not available on this device.");
//                return;
//            }
//        }

//        ToggleFlashlightWithCameraControl();
//#elif UNITY_IOS
//        ToggleFlashlight();
//#else
//        Debug.LogWarning("Flashlight is only supported on Android and iOS devices.");
//#endif
//    }

//#if UNITY_ANDROID
//    void ToggleFlashlightWithCameraControl()
//    {
//        //if (flashlightPlugin == null)
//        //{
//        //    Debug.LogError("Flashlight plugin not initialized.");
//        //    return;
//        //}

//        //try
//        //{
//        //    isFlashlightOn = !isFlashlightOn;

//        //    if (isFlashlightOn)
//        //    {
//        //        //if (qrScanner != null)
//        //        //{
//        //        //    qrScanner.StopScanning();
//        //        //}

//        //        StartCoroutine(TurnOnFlashlightAfterDelay());
//        //    }
//        //    else
//        //    {
//        //        flashlightPlugin.Call("setTorchMode", false);

//        //        //if (qrScanner != null)
//        //        //{
//        //        //    StartCoroutine(RestartCameraAfterDelay());
//        //        //}

//        //        Debug.Log("Flashlight turned OFF");
//        //    }

//        //    UpdateButtonText();
//        //}
//        //catch (System.Exception e)
//        //{
//        //    Debug.LogError("Failed to toggle flashlight: " + e.Message);
//        //    isFlashlightOn = !isFlashlightOn;
//        //    UpdateButtonText();
//        //}

//    }

//    System.Collections.IEnumerator TurnOnFlashlightAfterDelay()
//    {
//        yield return new WaitForSeconds(0.3f);

//        try
//        {
//            flashlightPlugin.Call("setTorchMode", true);
//            Debug.Log("Flashlight turned ON");
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("Failed to turn on flashlight: " + e.Message);
//            isFlashlightOn = false;
//            UpdateButtonText();

//            //if (qrScanner != null)
//            //{
//            //    qrScanner.ResumeScanning();
//            //}
//        }
//    }

//    System.Collections.IEnumerator RestartCameraAfterDelay()
//    {
//        yield return new WaitForSeconds(0.3f);
//        //qrScanner.ResumeScanning();
//    }
//#endif

//    public void ToggleFlashlight()
//    {
//#if UNITY_IOS
//        isFlashlightOn = !isFlashlightOn;
//        FlashlightPlugin.SetTorchEnabled(isFlashlightOn);
//        Debug.Log("Flashlight turned " + (isFlashlightOn ? "ON" : "OFF"));
//        UpdateButtonText();
//#endif
//    }

//    void UpdateButtonText()
//    {
//        if (buttonText != null)
//        {
//            buttonText.text = isFlashlightOn ? "Flash OFF" : "Flash ON";
//        }
//    }

//    void OnDestroy()
//    {
//#if UNITY_ANDROID
//        if (flashlightPlugin != null && isFlashlightOn)
//        {
//            try
//            {
//                flashlightPlugin.Call("setTorchMode", false);
//                flashlightPlugin.Call("cleanup");
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogError("Error cleaning up flashlight: " + e.Message);
//            }
//        }
//#endif
//    }

//    void OnApplicationPause(bool pauseStatus)
//    {
//        if (pauseStatus && isFlashlightOn)
//        {
//#if UNITY_ANDROID
//            if (flashlightPlugin != null)
//            {
//                flashlightPlugin.Call("setTorchMode", false);
//            }
//#endif
//            isFlashlightOn = false;
//            UpdateButtonText();
//        }
//    }
//}



using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class FlashlightWithCameraControl : MonoBehaviour
{
    public Button flashlightButton;
    public Text buttonText;
    //public ReadQRCode qrScanner;

    private bool isFlashlightOn = false;


    void Start()
    {
        flashlightButton.onClick.AddListener(ToggleFlashlight);

        UpdateButtonText();
    }

    void ToggleFlashlight()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            return;
        }

        isFlashlightOn = !isFlashlightOn;

#if UNITY_ANDROID
        if (isFlashlightOn)
        {
            //FlashLight.On();
        }
        else
        {
            //FlashLight.Off();
        }
#endif

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        buttonText.text = isFlashlightOn ? "Flash OFF" : "Flash ON";
    }
}


//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Android;
//using System.Collections;

//public class FlashlightWithCameraControl : MonoBehaviour
//{
//    public Button flashlightButton;
//    public Text buttonText;

//    private bool isFlashlightOn = false;
//    private bool isInitialized = false;

//#if UNITY_ANDROID
//    private AndroidJavaObject camera2Plugin;
//    private AndroidJavaObject cameraManager;
//    private string cameraId;
//#endif

//    void Start()
//    {
//        if (flashlightButton != null)
//        {
//            flashlightButton.onClick.AddListener(ToggleFlashlight);
//        }

//        UpdateButtonText();

//#if UNITY_ANDROID
//        StartCoroutine(InitializeAfterDelay());
//#endif
//    }

//#if UNITY_ANDROID
//    IEnumerator InitializeAfterDelay()
//    {
//        yield return new WaitForSeconds(2.5f);

//        InitializeCamera2();
//    }

//    void InitializeCamera2()
//    {
//        try
//        {
//            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

//            if (activity == null)
//            {
//                Debug.LogError("[Flashlight] Activity is null");
//                return;
//            }

//            AndroidJavaClass camera2Class = new AndroidJavaClass("com.unity.camera2qr.Camera2QRPlugin");

//            bool initialized = camera2Class.CallStatic<bool>("initialize", activity, 0);

//            if (initialized)
//            {
//                isInitialized = true;
//                Debug.Log("[Flashlight] ✓ Camera2 Plugin initialized - torch will work with scanning!");
//            }
//            else
//            {
//                Debug.LogError("[Flashlight] Camera2 initialization failed");
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("[Flashlight] Initialization error: " + e.Message + "\n" + e.StackTrace);
//        }
//    }

//    void ToggleFlashlight()
//    {
//        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
//        {
//            Permission.RequestUserPermission(Permission.Camera);
//            Debug.Log("[Flashlight] Requesting camera permission");
//            return;
//        }

//        if (!isInitialized)
//        {
//            Debug.LogWarning("[Flashlight] Not initialized, attempting initialization...");
//            InitializeCamera2();

//            if (!isInitialized)
//            {
//                Debug.LogError("[Flashlight] Failed to initialize");
//                return;
//            }
//        }

//        isFlashlightOn = !isFlashlightOn;

//        try
//        {
//            AndroidJavaClass camera2Class = new AndroidJavaClass("com.unity.camera2qr.Camera2QRPlugin");
//            bool success = camera2Class.CallStatic<bool>("setTorchMode", isFlashlightOn);

//            if (success)
//            {
//                Debug.Log("[Flashlight] ✓✓✓ Torch " + (isFlashlightOn ? "ON" : "OFF") + " while camera is scanning! ✓✓✓");
//            }
//            else
//            {
//                Debug.LogError("[Flashlight] Failed to toggle torch");
//                isFlashlightOn = !isFlashlightOn;
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("[Flashlight] Error: " + e.Message);
//            isFlashlightOn = !isFlashlightOn;
//        }

//        UpdateButtonText();
//    }

//    void TurnOffFlashlight()
//    {
//        if (isInitialized && isFlashlightOn)
//        {
//            try
//            {
//                AndroidJavaClass camera2Class = new AndroidJavaClass("com.unity.camera2qr.Camera2QRPlugin");
//                camera2Class.CallStatic<bool>("setTorchMode", false);
//                isFlashlightOn = false;
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogError("[Flashlight] Error turning off: " + e.Message);
//            }
//        }
//    }
//#else
//    void ToggleFlashlight()
//    {
//        Debug.LogWarning("[Flashlight] Only works on Android");
//    }
//#endif

//    void UpdateButtonText()
//    {
//        if (buttonText != null)
//        {
//            buttonText.text = isFlashlightOn ? "Flash OFF" : "Flash ON";
//        }
//    }

//    void OnDestroy()
//    {
//#if UNITY_ANDROID
//        TurnOffFlashlight();
//        if (isInitialized)
//        {
//            try
//            {
//                AndroidJavaClass camera2Class = new AndroidJavaClass("com.unity.camera2qr.Camera2QRPlugin");
//                camera2Class.CallStatic("cleanup");
//            }
//            catch { }
//        }
//#endif
//    }

//    void OnApplicationPause(bool pauseStatus)
//    {
//#if UNITY_ANDROID
//        if (pauseStatus)
//        {
//            TurnOffFlashlight();
//            UpdateButtonText();
//        }
//#endif
//    }

//    void OnApplicationQuit()
//    {
//#if UNITY_ANDROID
//        TurnOffFlashlight();
//#endif
//    }
//}

