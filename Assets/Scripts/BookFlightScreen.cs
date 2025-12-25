using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BookFlightScreen : MonoBehaviour
{
    public List<string> _DestinationString;
    public FlightData _flightData;
    public Button _searchFlightBtn, _backBtn;
    public bool _destinetionOn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _searchFlightBtn.onClick.AddListener(SearchFlightBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void SearchFlightBtnClick()
    {
        if (_flightData.ticketData.Count == 0)
        {
            return;
        }

        if (_destinetionOn)
        {
            UIManager.instance._searchDetailScript._flightData = _flightData;
            UIManager.instance._searchDetailScript.SetData();
        }
        else
        {
            UIManager.instance._noFlightDetailScript._flightData = _flightData;
            UIManager.instance.SwitchScreen(14);
        }

    }
    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(2);
    }
}
