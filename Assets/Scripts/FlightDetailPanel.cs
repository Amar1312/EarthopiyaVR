using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlightDetailPanel : MonoBehaviour
{
    public TicketData _ticketData;
    public Button _confirmBtn, _backBtn;

    [Space]
    [Header("Ticket Data")]
    public TextMeshProUGUI _cityCode;
    public TextMeshProUGUI _cityName, _airportName, _time, _durationTime, _stops, _adultPrice, _discountPrice;

    public void SetData()
    {
        _cityCode.text = _ticketData.destinationCode;
        _cityName.text = _ticketData.destinationName;
        _airportName.text = _ticketData.destinationAirport;
        _time.text = _ticketData.departTime;
        _durationTime.text = _ticketData.TimeDuration;
        _stops.text = _ticketData.stops + " Stops";
        _adultPrice.text = "$ " + _ticketData.adultPrice;
        _discountPrice.text = "-$ " + _ticketData.discountPrice;

        UIManager.instance.SwitchScreen(5);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _confirmBtn.onClick.AddListener(ConfirmBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void ConfirmBtnClick()
    {
        UIManager.instance._chooseSeatScript._ticketData = _ticketData;
        UIManager.instance._chooseSeatScript.SetData();

    }

    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(4);
    }
}
