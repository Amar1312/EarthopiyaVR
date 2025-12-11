using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private WebCamTexture webCamTexture;
    private bool isFlashOn = false;
    private bool isFrontCamera = false;

    // For Android flashlight
    private AndroidJavaObject cameraManager;
    private string cameraId;
    private bool hasFlash = false;

    // UI Elements
    public RawImage cameraDisplay;
    public Button flashToggleButton;
    public Button switchCameraButton;
    public Text flashButtonText;
    public Text statusText;

    void Start()
    {
        InitializeCamera();
        SetupButtons();
    }

    void InitializeCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No camera device found!");
            return;
        }

        string selectedCamera = "";
        for (int i = 0; i < devices.Length; i++)
        {
            if (isFrontCamera && devices[i].isFrontFacing)
            {
                selectedCamera = devices[i].name;
                break;
            }
            else if (!isFrontCamera && !devices[i].isFrontFacing)
            {
                selectedCamera = devices[i].name;
                break;
            }
        }

        if (string.IsNullOrEmpty(selectedCamera))
        {
            selectedCamera = devices[0].name;
        }

        webCamTexture = new WebCamTexture(selectedCamera);
        cameraDisplay.texture = webCamTexture;
        webCamTexture.Play();

#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeAndroidFlashlight();
#endif
    }

    void SetupButtons()
    {
        if (flashToggleButton != null)
        {
            flashToggleButton.onClick.AddListener(ToggleFlash);
            UpdateFlashButtonText();
        }

        if (switchCameraButton != null)
        {
            switchCameraButton.onClick.AddListener(SwitchCamera);
        }
    }

    public void ToggleFlash()
    {
        isFlashOn = !isFlashOn;

#if UNITY_ANDROID && !UNITY_EDITOR
        ToggleAndroidFlashlight(isFlashOn);
#else
        Debug.Log("Flash toggle requested: " + isFlashOn);
        // Simulate flash effect in editor
        if (isFlashOn)
        {
            cameraDisplay.color = Color.white * 1.2f;
        }
        else
        {
            cameraDisplay.color = Color.white;
        }
#endif

        UpdateFlashButtonText();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void InitializeAndroidFlashlight()
    {
        try
        {
            // Get CameraManager
            AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
            AndroidJavaObject cameraObject = null;
            
            // Try to open back camera
            cameraObject = cameraClass.CallStatic<AndroidJavaObject>("open", 0);
            
            if (cameraObject != null)
            {
                AndroidJavaObject cameraParameters = cameraObject.Call<AndroidJavaObject>("getParameters");
                string flashMode = cameraParameters.Call<string>("getFlashMode");
                
                // Check if flash is supported
                AndroidJavaObject supportedFlashModes = cameraParameters.Call<AndroidJavaObject>("getSupportedFlashModes");
                hasFlash = (supportedFlashModes != null);
                
                // Release camera immediately
                cameraObject.Call("release");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Camera flashlight check failed: " + e.Message);
            hasFlash = false;
        }
    }

    void ToggleAndroidFlashlight(bool turnOn)
    {
        try
        {
            // Using the old Camera API which is more reliable across devices
            AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
            AndroidJavaObject cameraObject = null;
            
            try
            {
                cameraObject = cameraClass.CallStatic<AndroidJavaObject>("open", 0);
                
                if (cameraObject != null)
                {
                    AndroidJavaObject cameraParameters = cameraObject.Call<AndroidJavaObject>("getParameters");
                    
                    if (turnOn)
                    {
                        cameraParameters.Call("setFlashMode", "torch");
                    }
                    else
                    {
                        cameraParameters.Call("setFlashMode", "off");
                    }
                    
                    cameraObject.Call("setParameters", cameraParameters);
                    
                    // For flashlight to stay on, don't release the camera
                    // Instead, store it and release when turning off
                    if (!turnOn)
                    {
                        cameraObject.Call("release");
                    }
                    // Keep cameraObject alive when flash is on
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Flashlight toggle failed: " + e.Message);
                if (cameraObject != null)
                {
                    cameraObject.Call("release");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Flashlight system error: " + e.Message);
        }
    }
#endif

    void UpdateFlashButtonText()
    {
        if (flashButtonText != null)
        {
            flashButtonText.text = isFlashOn ? "FLASH ON" : "FLASH OFF";
        }

        if (statusText != null)
        {
            statusText.text = "Flash: " + (isFlashOn ? "ON" : "OFF");
        }
    }

    public void SwitchCamera()
    {
        isFrontCamera = !isFrontCamera;

        // Turn off flash before switching
        if (isFlashOn)
        {
            ToggleFlash();
        }

        // Stop current camera
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        // Initialize new camera
        InitializeCamera();
    }

    void OnDestroy()
    {
        // Turn off flash when app closes
        if (isFlashOn)
        {
            ToggleFlash();
        }

        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}