using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZXing;
using UnityEngine.Android;

public class ReadQRCode : MonoBehaviour
{
    public RawImage cameraDisplay;
    public TextMeshProUGUI qrText;
    public float scanInterval = 0.5f;

    [Header("Camera Quality Settings")]
    public int requestedWidth = 1920;
    public int requestedHeight = 1080;
    public int requestedFPS = 30;

    public WebCamTexture webCamTexture;
    private bool isScanning = false;
    private float nextScanTime = 0f;
    private bool permissionRequested = false;

#if UNITY_ANDROID
    private AndroidJavaObject cameraFlashlightPlugin;
#endif

    void Start()
    {
        RequestCameraPermission();
    }

    void RequestCameraPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            permissionRequested = true;
            if (qrText != null)
                qrText.text = "Requesting camera permission...";
        }
        else
        {
            StartCamera();
        }
#elif UNITY_IOS
        StartCamera();
#else
        StartCamera();
#endif
    }

    void Update()
    {
#if UNITY_ANDROID
        if (permissionRequested && Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            permissionRequested = false;
            StartCamera();
        }
#endif

        if (isScanning && webCamTexture != null && webCamTexture.isPlaying)
        {
            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + scanInterval;
                ScanQRCode();
            }
        }
    }

    void StartCamera()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.LogError("Camera permission denied!");
            if (qrText != null)
                qrText.text = "Camera permission denied. Please enable camera access in settings.";
            return;
        }
#endif

        StartCoroutine(InitializeCamera());
    }

    IEnumerator InitializeCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Camera permission denied!");
            if (qrText != null)
                qrText.text = "Camera permission denied. Please enable camera access in settings.";
            yield break;
        }

        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected!");
            if (qrText != null)
                qrText.text = "No camera found on this device";
            yield break;
        }

        string backCameraName = "";
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log($"Camera {i}: {devices[i].name}, Front Facing: {devices[i].isFrontFacing}");
            if (!devices[i].isFrontFacing)
            {
                backCameraName = devices[i].name;
                break;
            }
        }

        if (string.IsNullOrEmpty(backCameraName))
        {
            backCameraName = devices[0].name;
            Debug.Log("No back camera found, using first available camera");
        }

        webCamTexture = new WebCamTexture(backCameraName, requestedWidth, requestedHeight, requestedFPS);

        if (cameraDisplay != null)
        {
            cameraDisplay.texture = webCamTexture;
        }

        webCamTexture.Play();

        yield return new WaitForSeconds(1f);

        if (webCamTexture.isPlaying)
        {
            SetupCameraDisplay();
            isScanning = true;
            if (qrText != null)
                qrText.text = "Scanning for QR codes...";

            Debug.Log($"Camera Started Successfully:\n" +
                      $"  Name: {backCameraName}\n" +
                      $"  Requested: {requestedWidth}x{requestedHeight} @ {requestedFPS}fps\n" +
                      $"  Actual: {webCamTexture.width}x{webCamTexture.height}\n" +
                      $"  Rotation: {webCamTexture.videoRotationAngle}°");

        }
        else
        {
            Debug.LogError("Failed to start camera!");
            if (qrText != null)
                qrText.text = "Failed to start camera";
        }
    }

    void SetupCameraDisplay()
    {
        if (webCamTexture == null || cameraDisplay == null) return;

        int videoRotation = webCamTexture.videoRotationAngle;
        cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -videoRotation);

        float cameraWidth = webCamTexture.width;
        float cameraHeight = webCamTexture.height;

        if (videoRotation == 90 || videoRotation == 270)
        {
            float temp = cameraWidth;
            cameraWidth = cameraHeight;
            cameraHeight = temp;
        }

        float aspectRatio = cameraWidth / cameraHeight;

        Debug.Log($"Display Setup:\n" +
                  $"  Texture Size: {webCamTexture.width}x{webCamTexture.height}\n" +
                  $"  Rotation: {videoRotation}°\n" +
                  $"  Display Size: {cameraWidth}x{cameraHeight}\n" +
                  $"  Aspect Ratio: {aspectRatio:F3}");
    }

    void ScanQRCode()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new System.Collections.Generic.List<BarcodeFormat>
                    {
                        BarcodeFormat.QR_CODE
                    }
                }
            };

            var result = barcodeReader.Decode(webCamTexture.GetPixels32(), webCamTexture.width, webCamTexture.height);

            if (result != null)
            {
                Debug.Log($"QR Code detected: {result.Text}");
                if (qrText != null)
                    qrText.text = result.Text;

                OnQRCodeScanned(result.Text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error scanning QR: {ex.Message}");
        }
    }

    void OnQRCodeScanned(string qrContent)
    {
        Debug.Log($"QR Content: {qrContent}");
    }

    void OnDestroy()
    {
        StopCamera();
    }

    void StopCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }

    IEnumerator RestartCamera()
    {
        yield return new WaitForSeconds(1f);
        SetupCameraDisplay();
        isScanning = true;
    }

    public void StopScanning()
    {
        isScanning = false;
        StopCamera();
    }

    public void ResumeScanning()
    {
        if (webCamTexture != null && !webCamTexture.isPlaying)
        {
            webCamTexture.Play();
            StartCoroutine(RestartCamera());
        }
    }

    public void BackScene()
    {
        SceneManager.LoadScene(0);
    }
}


//using UnityEngine.SceneManagement;
//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using ZXing;
//using UnityEngine.Android;

//public class ReadQRCode : MonoBehaviour
//{
//    public RawImage cameraDisplay;
//    public TextMeshProUGUI qrText;
//    public float scanInterval = 0.5f;

//    [Header("Camera Quality Settings")]
//    public int requestedWidth = 1920;
//    public int requestedHeight = 1080;
//    public int requestedFPS = 30;

//    public WebCamTexture webCamTexture;
//    private bool isScanning = false;
//    private float nextScanTime = 0f;
//    private bool permissionRequested = false;

//    void Start()
//    {
//        RequestCameraPermission();
//    }

//    void RequestCameraPermission()
//    {
//#if UNITY_ANDROID
//        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
//        {
//            Permission.RequestUserPermission(Permission.Camera);
//            permissionRequested = true;
//            if (qrText != null)
//                qrText.text = "Requesting camera permission...";
//        }
//        else
//        {
//            StartCamera();
//        }
//#elif UNITY_IOS
//        StartCamera();
//#else
//        StartCamera();
//#endif
//    }

//    void Update()
//    {
//#if UNITY_ANDROID
//        if (permissionRequested && Permission.HasUserAuthorizedPermission(Permission.Camera))
//        {
//            permissionRequested = false;
//            StartCamera();
//        }
//#endif

//        if (isScanning && webCamTexture != null && webCamTexture.isPlaying)
//        {
//            if (Time.time >= nextScanTime)
//            {
//                nextScanTime = Time.time + scanInterval;
//                ScanQRCode();
//            }
//        }
//    }

//    void StartCamera()
//    {
//#if UNITY_ANDROID
//        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
//        {
//            Debug.LogError("Camera permission denied!");
//            if (qrText != null)
//                qrText.text = "Camera permission denied. Please enable camera access in settings.";
//            return;
//        }
//#endif

//        StartCoroutine(InitializeCamera());
//    }

//    IEnumerator InitializeCamera()
//    {
//        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

//        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
//        {
//            Debug.LogError("Camera permission denied!");
//            if (qrText != null)
//                qrText.text = "Camera permission denied. Please enable camera access in settings.";
//            yield break;
//        }

//        WebCamDevice[] devices = WebCamTexture.devices;

//        if (devices.Length == 0)
//        {
//            Debug.LogError("No camera detected!");
//            if (qrText != null)
//                qrText.text = "No camera found on this device";
//            yield break;
//        }

//        string backCameraName = "";
//        for (int i = 0; i < devices.Length; i++)
//        {
//            Debug.Log($"Camera {i}: {devices[i].name}, Front Facing: {devices[i].isFrontFacing}");
//            if (!devices[i].isFrontFacing)
//            {
//                backCameraName = devices[i].name;
//                break;
//            }
//        }

//        if (string.IsNullOrEmpty(backCameraName))
//        {
//            backCameraName = devices[0].name;
//            Debug.Log("No back camera found, using first available camera");
//        }

//        webCamTexture = new WebCamTexture(backCameraName, requestedWidth, requestedHeight, requestedFPS);

//        if (cameraDisplay != null)
//        {
//            cameraDisplay.texture = webCamTexture;
//        }

//        webCamTexture.Play();

//        yield return new WaitForSeconds(1f);

//        if (webCamTexture.isPlaying)
//        {
//            SetupCameraDisplay();
//            isScanning = true;
//            if (qrText != null)
//                qrText.text = "Scanning for QR codes...";

//            Debug.Log($"Camera Started Successfully:\n" +
//                      $"  Name: {backCameraName}\n" +
//                      $"  Requested: {requestedWidth}x{requestedHeight} @ {requestedFPS}fps\n" +
//                      $"  Actual: {webCamTexture.width}x{webCamTexture.height}\n" +
//                      $"  Rotation: {webCamTexture.videoRotationAngle}°");

//        }
//        else
//        {
//            Debug.LogError("Failed to start camera!");
//            if (qrText != null)
//                qrText.text = "Failed to start camera";
//        }
//    }

//    void SetupCameraDisplay()
//    {
//        if (webCamTexture == null || cameraDisplay == null) return;

//        int videoRotation = webCamTexture.videoRotationAngle;
//        cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -videoRotation);

//        float cameraWidth = webCamTexture.width;
//        float cameraHeight = webCamTexture.height;

//        if (videoRotation == 90 || videoRotation == 270)
//        {
//            float temp = cameraWidth;
//            cameraWidth = cameraHeight;
//            cameraHeight = temp;
//        }

//        float aspectRatio = cameraWidth / cameraHeight;

//        Debug.Log($"Display Setup:\n" +
//                  $"  Texture Size: {webCamTexture.width}x{webCamTexture.height}\n" +
//                  $"  Rotation: {videoRotation}°\n" +
//                  $"  Display Size: {cameraWidth}x{cameraHeight}\n" +
//                  $"  Aspect Ratio: {aspectRatio:F3}");
//    }

//    void ScanQRCode()
//    {
//        try
//        {
//            IBarcodeReader barcodeReader = new BarcodeReader
//            {
//                AutoRotate = true,
//                Options = new ZXing.Common.DecodingOptions
//                {
//                    TryHarder = true,
//                    PossibleFormats = new System.Collections.Generic.List<BarcodeFormat>
//                    {
//                        BarcodeFormat.QR_CODE
//                    }
//                }
//            };

//            var result = barcodeReader.Decode(webCamTexture.GetPixels32(), webCamTexture.width, webCamTexture.height);

//            if (result != null)
//            {
//                Debug.Log($"QR Code detected: {result.Text}");
//                if (qrText != null)
//                    qrText.text = result.Text;

//                OnQRCodeScanned(result.Text);
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogWarning($"Error scanning QR: {ex.Message}");
//        }
//    }

//    void OnQRCodeScanned(string qrContent)
//    {
//        Debug.Log($"QR Content: {qrContent}");
//    }

//    void OnDestroy()
//    {
//        StopCamera();
//    }

//    void StopCamera()
//    {
//        if (webCamTexture != null && webCamTexture.isPlaying)
//        {
//            webCamTexture.Stop();
//            Debug.Log("Camera stopped");
//        }
//    }

//    IEnumerator RestartCamera()
//    {
//        yield return new WaitForSeconds(0.5f);

//        if (webCamTexture != null && !webCamTexture.isPlaying)
//        {
//            webCamTexture.Play();
//            yield return new WaitForSeconds(0.5f);
//            SetupCameraDisplay();
//            isScanning = true;
//            Debug.Log("Camera restarted");
//        }
//    }

//    public void StopScanning()
//    {
//        isScanning = false;
//        StopCamera();
//    }

//    public void ResumeScanning()
//    {
//        if (webCamTexture != null)
//        {
//            StartCoroutine(RestartCamera());
//        }
//    }

//    public void BackScene()
//    {
//        SceneManager.LoadScene(0);
//    }
//}
