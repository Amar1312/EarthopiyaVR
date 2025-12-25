using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QRScanningPanel : MonoBehaviour
{
    public Button _bookFlightBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _bookFlightBtn.onClick.AddListener(BookFlightBtnClick);
    }

    void BookFlightBtnClick()
    {
        APIQRRead.Instance._qRReadScript.CheckFlashOFF();
        SceneController.Instance._gameOn = 4;
        SceneManager.LoadScene(0);
    }
}
