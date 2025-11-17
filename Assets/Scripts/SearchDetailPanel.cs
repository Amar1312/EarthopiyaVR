using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SearchDetailPanel : MonoBehaviour
{
    public FlightData _flightData;
    public Button _backBtn, _ticket1BookBtn, _ticket2BookBtn;
    public TextMeshProUGUI _titleDestinationCode, _titleTimeDuration;

    [Space]
    [Header("Ticket 1")]
    public TextMeshProUGUI _departTime;
    public TextMeshProUGUI _stap, _timeDuration, _destinetionCode, _destinetionName, _arrivalTime;
    [Header("Ticket 2")]
    public TextMeshProUGUI _departTime2;
    public TextMeshProUGUI _stap2, _timeDuration2, _destinetionCode2, _destinetionName2, _arrivalTime2;

    public void SetData()
    {
        _titleDestinationCode.text = _flightData.ticketData[0].destinationCode;
        _titleTimeDuration.text = _flightData.ticketData[0].TimeDuration;

        _departTime.text = _flightData.ticketData[0].departTime;
        _stap.text = _flightData.ticketData[0].stops + " Stops";
        _timeDuration.text = _flightData.ticketData[0].TimeDuration;
        _destinetionCode.text = _flightData.ticketData[0].destinationCode;
        _destinetionName.text = _flightData.ticketData[0].destinationName;
        _arrivalTime.text = _flightData.ticketData[0].arrivalTime;

        _departTime2.text = _flightData.ticketData[1].departTime;
        _stap2.text = _flightData.ticketData[1].stops + " Stops";
        _timeDuration2.text = _flightData.ticketData[1].TimeDuration;
        _destinetionCode2.text = _flightData.ticketData[1].destinationCode;
        _destinetionName2.text = _flightData.ticketData[1].destinationName;
        _arrivalTime2.text = _flightData.ticketData[1].arrivalTime;

        UIManager.instance.SwitchScreen(4);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _backBtn.onClick.AddListener(BackBtnClick);
        _ticket1BookBtn.onClick.AddListener(Ticket1Click);
        _ticket2BookBtn.onClick.AddListener(Ticket2Click);
    }

    void Ticket1Click()
    {
        UIManager.instance._flightDetailScript._ticketData = _flightData.ticketData[0];
        UIManager.instance._flightDetailScript.SetData();
    }

    void Ticket2Click()
    {
        UIManager.instance._flightDetailScript._ticketData = _flightData.ticketData[1];
        UIManager.instance._flightDetailScript.SetData();
    }

    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(3);
    }

}
