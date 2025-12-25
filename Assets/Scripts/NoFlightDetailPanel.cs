using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoFlightDetailPanel : MonoBehaviour
{
    public FlightData _flightData;
    public Button _backBtn, _okBtn;
    public TextMeshProUGUI _toCityCode;

    private UIManager _uiManager;

    private void OnEnable()
    {
        _toCityCode.text = _flightData.ticketData[0].destinationCode;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _backBtn.onClick.AddListener(BackBtnClick);
        _okBtn.onClick.AddListener(OkBtnClick);
    }

    void BackBtnClick()
    {
        _uiManager.SwitchScreen(3);
    }

    void OkBtnClick()
    {
        _uiManager.SwitchScreen(3);
    }
}
