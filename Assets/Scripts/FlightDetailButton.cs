using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlightDetailButton : MonoBehaviour
{
    private Button _meButton;
    public string _destinationName;
    public TMP_InputField _desinationField;
    public GameObject _nextSafariScroll;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _meButton = GetComponent<Button>();
        _meButton.onClick.AddListener(MeBtnClick);   
    }

    void MeBtnClick()
    {
        _desinationField.text = _destinationName;
        _nextSafariScroll.SetActive(false);
    }
}
