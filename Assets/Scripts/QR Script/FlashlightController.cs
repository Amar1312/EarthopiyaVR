using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class FlashlightController : MonoBehaviour
{
    [Header("UI Reference")]
    public Button flashlightButton;

    [Header("Button Text (Optional)")]
    public Text buttonText;

    private bool isFlashlightOn = false;

#if UNITY_ANDROID
    private AndroidJavaObject flashlightPlugin;
    private bool isInitialized = false;
#endif

    void Start()
    {
        if (flashlightButton != null)
        {
            flashlightButton.onClick.AddListener(OnFlashlightButtonClicked);
        }

        UpdateButtonText();

#if UNITY_ANDROID
        InitializeFlashlight();
#endif
    }

#if UNITY_ANDROID
    void InitializeFlashlight()
    {
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            flashlightPlugin = new AndroidJavaObject("com.unity.flashlight.FlashlightPlugin", currentActivity);
            isInitialized = flashlightPlugin.Call<bool>("initializeFlashlight");

            if (isInitialized)
            {
                Debug.Log("Flashlight plugin initialized successfully.");
            }
            else
            {
                Debug.LogWarning("Device does not support flashlight.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize flashlight plugin: " + e.Message);
            isInitialized = false;
        }
    }
#endif

    void OnFlashlightButtonClicked()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            Debug.Log("Camera permission required for flashlight.");
            return;
        }

        if (!isInitialized)
        {
            InitializeFlashlight();
        }

        if (isInitialized)
        {
            ToggleFlashlight();
        }
        else
        {
            Debug.LogWarning("Flashlight not available on this device.");
        }
#elif UNITY_IOS
        ToggleFlashlight();
#else
        Debug.LogWarning("Flashlight is only supported on Android and iOS devices.");
#endif
    }

    public void ToggleFlashlight()
    {
#if UNITY_ANDROID
        ToggleAndroidFlashlight();
#elif UNITY_IOS
        ToggleIOSFlashlight();
#endif

        UpdateButtonText();
    }

#if UNITY_ANDROID
    void ToggleAndroidFlashlight()
    {
        if (flashlightPlugin == null)
        {
            Debug.LogError("Flashlight plugin not initialized.");
            return;
        }

        try
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightPlugin.Call("setTorchMode", isFlashlightOn);
            Debug.Log("Flashlight turned " + (isFlashlightOn ? "ON" : "OFF"));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to toggle flashlight: " + e.Message);
            isFlashlightOn = !isFlashlightOn;
        }
    }
#endif

#if UNITY_IOS
    void ToggleIOSFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        
        if (isFlashlightOn)
        {
            FlashlightPlugin.SetTorchEnabled(true);
            Debug.Log("Flashlight turned ON");
        }
        else
        {
            FlashlightPlugin.SetTorchEnabled(false);
            Debug.Log("Flashlight turned OFF");
        }
    }
#endif

    void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = isFlashlightOn ? "Flash OFF" : "Flash ON";
        }
    }

    void OnDestroy()
    {
#if UNITY_ANDROID
        if (flashlightPlugin != null)
        {
            try
            {
                flashlightPlugin.Call("cleanup");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error cleaning up flashlight: " + e.Message);
            }
        }
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isFlashlightOn)
        {
            ToggleFlashlight();
        }
    }

    void OnApplicationQuit()
    {
        if (isFlashlightOn)
        {
            ToggleFlashlight();
        }
    }
}
