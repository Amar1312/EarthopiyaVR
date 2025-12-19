using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZXing;
using Vuforia;

public class Vuforiya_QR_Read : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI qrCodeText;
    public Button _backBtn, _lightBtn;

    [Header("Scan Settings")]
    public float scanInterval = 0.5f;

    private const PixelFormat PIXEL_FORMAT = PixelFormat.RGB888;
    private bool formatRegistered = false;
    private bool isScanning = false;
    private float nextScanTime = 0f;
    private string lastQRCode = "";
    private Texture2D cameraTexture;

    private bool isFlashOn = false;

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped += OnVuforiaStopped;

        if (qrCodeText != null)
        {
            qrCodeText.text = "Initializing Vuforia...";
        }
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaUpdated;
        }

        _backBtn.onClick.AddListener(GoBack);
        _lightBtn.onClick.AddListener(ToggleFlashlight);
    }

    void OnDestroy()
    {
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.World.OnStateUpdated -= OnVuforiaUpdated;
        }

        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped -= OnVuforiaStopped;

        if (VuforiaApplication.Instance.IsRunning)
        {
            UnregisterFormat();
        }

        if (cameraTexture != null)
        {
            Destroy(cameraTexture);
        }
    }

    void OnVuforiaStarted()
    {
        RegisterFormat();

        if (qrCodeText != null)
        {
            qrCodeText.text = "Scanning for QR codes...";
        }

        Debug.Log("Vuforia QR Scanner started");
    }

    void OnVuforiaStopped()
    {
        UnregisterFormat();

        if (cameraTexture != null)
        {
            Destroy(cameraTexture);
            cameraTexture = null;
        }
    }

    void RegisterFormat()
    {
        bool success = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, true);

        if (success)
        {
            Debug.Log("Successfully registered pixel format: " + PIXEL_FORMAT);
            formatRegistered = true;
            isScanning = true;
        }
        else
        {
            Debug.LogError("Failed to register pixel format: " + PIXEL_FORMAT);
            formatRegistered = false;

            if (qrCodeText != null)
            {
                qrCodeText.text = "Error: Failed to access camera";
            }
        }
    }

    void UnregisterFormat()
    {
        if (formatRegistered)
        {
            Debug.Log("Unregistering camera pixel format");
            VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, false);
            formatRegistered = false;
            isScanning = false;
        }
    }

    void OnVuforiaUpdated()
    {
        if (!formatRegistered || !isScanning)
            return;

        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        var image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PIXEL_FORMAT);

        if (Vuforia.Image.IsNullOrEmpty(image))
            return;

        if (cameraTexture == null)
        {
            cameraTexture = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);
            Debug.Log($"Created camera texture: {image.Width}x{image.Height}");
        }

        image.CopyToTexture(cameraTexture, false);

        ScanQRCode(cameraTexture);
    }

    void ScanQRCode(Texture2D texture)
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

            var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                if (result.Text != lastQRCode)
                {
                    lastQRCode = result.Text;
                    OnQRCodeDetected(result.Text);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"QR scan error: {ex.Message}");
        }
    }

    void OnQRCodeDetected(string qrText)
    {
        Debug.Log("=================================");
        Debug.Log("QR CODE DETECTED!");
        Debug.Log("Text: " + qrText);
        Debug.Log("=================================");

        if (qrCodeText != null)
        {
            qrCodeText.text = qrText;
        }
    }

    public string GetCurrentQRCode()
    {
        return lastQRCode;
    }

    public void StartScanning()
    {
        isScanning = true;
        lastQRCode = "";

        if (qrCodeText != null)
        {
            qrCodeText.text = "Scanning for QR codes...";
        }

        Debug.Log("QR scanning started");
    }

    public void StopScanning()
    {
        isScanning = false;
        lastQRCode = "";

        if (qrCodeText != null)
        {
            qrCodeText.text = "Scanning stopped";
        }

        Debug.Log("QR scanning stopped");
    }

    public void GoBack()
    {
        if (isFlashOn)
        {
            ToggleFlashlight();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ToggleFlashlight()
    {
        isFlashOn = !isFlashOn;

        VuforiaBehaviour.Instance.CameraDevice.SetFlash(isFlashOn);
    }
}
