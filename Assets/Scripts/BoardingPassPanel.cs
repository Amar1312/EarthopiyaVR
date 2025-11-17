using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardingPassPanel : MonoBehaviour
{
    public TicketData _ticketData;
    public Button _backBtn, _payPrintBtn;

    [Space]
    [Header("Ticket Data")]
    public TextMeshProUGUI _cityCode;
    public TextMeshProUGUI _cityName, _airportName, _flightCode, _departTime, _arrivalTime, _gate, _timeDuration, _stops;


    public void SetData()
    {
        _cityCode.text = _ticketData.destinationCode;
        _cityName.text = _ticketData.destinationName;
        _airportName.text = _ticketData.destinationAirport;
        _flightCode.text = _ticketData.FlightCode;
        _departTime.text = _ticketData.departTime;
        _arrivalTime.text = _ticketData.arrivalTime;
        _gate.text = _ticketData.GateCode;
        _timeDuration.text = _ticketData.TimeDuration;
        _stops.text = _ticketData.stops + " Stops";

        UIManager.instance.SwitchScreen(7);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _backBtn.onClick.AddListener(BackBtnClick);
        _payPrintBtn.onClick.AddListener(PrintTicketClick);
    }

    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(6);
    }

    void PrintTicketClick()
    {
        Debug.Log("Print Ticket");
    }
}
