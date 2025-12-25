using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QRErrorPanel : MonoBehaviour
{
    public Button _backBtn, _rescanBtn;

    private APIQRRead _apiQRRead;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _apiQRRead = APIQRRead.Instance;
        _backBtn.onClick.AddListener(BackBtnClick);
        _rescanBtn.onClick.AddListener(RescanBtnClick);
    }

    void BackBtnClick()
    {
        _apiQRRead._qRReadScript.StartScanning();
        _apiQRRead._qRScanningScript.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    void RescanBtnClick()
    {
        _apiQRRead._qRReadScript.StartScanning();
        _apiQRRead._qRScanningScript.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
