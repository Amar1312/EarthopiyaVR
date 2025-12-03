using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;

public class DialCodeBtn : MonoBehaviour
{
    public Image _flagImage;
    public TextMeshProUGUI _codeText, _countryText;
    public Button _meBtn;
    public CountryData _countryData;
    public float _flagWidth;
    private string url;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        url = "https://country-code-au6g.vercel.app/" + _countryData.image;
        _meBtn.onClick.AddListener(MeBtnClick);
        _codeText.text = _countryData.dial_code;
        string trimmed = _countryData.name.Trim();
        _countryText.text = trimmed;
        //Invoke(nameof(LoadImage), 1f);
    }

    void LoadImage()
    {
        UIManager.instance.LoadSVGImage(url, _flagImage, _flagWidth);
    }

    void MeBtnClick()
    {
        UIManager.instance._profilePanel.SetDialCode(_countryData.dial_code);
    }

}
