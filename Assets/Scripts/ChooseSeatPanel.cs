using UnityEngine;
using UnityEngine.UI;

public class ChooseSeatPanel : MonoBehaviour
{
    public Toggle _seatTog;
    public Image _seatPlaneImage;
    public Sprite _selectSeat, _unSelectSeat;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _seatTog.onValueChanged.AddListener(delegate
        {
            SeatToggleChange();
        });
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
}
