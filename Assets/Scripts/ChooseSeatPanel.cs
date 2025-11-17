using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChooseSeatPanel : MonoBehaviour
{
    public Toggle _seatTog;
    public Image _seatPlaneImage;
    public Sprite _selectSeat, _unSelectSeat;
    public TicketData _ticketData;

    public Button _backBtn, _chooseSetBtn;

    [Space]
    [Header("Data")]
    public TextMeshProUGUI _destinationCode;

    public void SetData()
    {
        _destinationCode.text = _ticketData.destinationCode;
        UIManager.instance.SwitchScreen(6);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _seatTog.onValueChanged.AddListener(delegate
        {
            SeatToggleChange();
        });

        _chooseSetBtn.onClick.AddListener(ChooseSetClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void SeatToggleChange()
    {
        if (_seatTog.isOn)
        {
            _seatPlaneImage.sprite = _selectSeat;
        }
        else
        {
            _seatPlaneImage.sprite = _unSelectSeat;
        }
    }

    void ChooseSetClick()
    {
        if (_seatTog.isOn)
        {
            UIManager.instance._boardingPassScript._ticketData = _ticketData;
            UIManager.instance._boardingPassScript.SetData();

        }

    }

    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(5);
    }
}
