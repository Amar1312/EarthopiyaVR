using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZXing;
using Vuforia;
using System.Collections.Generic;

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
    private APIQRRead _apiQRRead;

    [Space]
    [Header("Orignal QR Data")]
    public List<string> _QRStringData;

    void Start()
    {
        _apiQRRead = APIQRRead.Instance;
        if (qrCodeText != null)
        {
            qrCodeText.text = "Initializing Vuforia...";
        }
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped += OnVuforiaStopped;

        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaUpdated;
        }

        _backBtn.onClick.AddListener(GoBackScene);
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
        Debug.Log("Text: " + qrText);


        if (qrCodeText != null)
        {
            qrCodeText.text = qrText;
        }

        StopScanning();
        int Index = GetStringIndex(qrText); //ETPYA|v=1|eid=1|nonce=C7F73DE8|ts=0
        if (Index >= 0)
        {
            int num = Index + 1;
            List<string> PassportID = new List<string>();
            PassportID.Add(num.ToString());
            _apiQRRead.UpdatePassport(PassportID, PassportUpdate);
        }
        else
        {
            _apiQRRead._qRErrorScript.gameObject.SetActive(true);
        }
    }

    void PassportUpdate(UpdatePassportResponce responce)
    {
        if (responce.status)
        {
            GoBackScene();
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

    public void GoBackScene()
    {
        CheckFlashOFF();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ToggleFlashlight()
    {
        isFlashOn = !isFlashOn;

        VuforiaBehaviour.Instance.CameraDevice.SetFlash(isFlashOn);
    }

    public void CheckFlashOFF()
    {
        if (isFlashOn)
        {
            ToggleFlashlight();
        }
    }

    public int GetStringIndex(string value)
    {
        if (string.IsNullOrEmpty(value))
            return -1;

        // Trim the search string
        string trimmedSearch = value.Trim();

        for (int i = 0; i < _QRStringData.Count; i++)
        {
            if (_QRStringData[i].Trim().Equals(trimmedSearch))
            {
                return i; // Found index
            }
        }

        return -1; // Not found
    }

}
