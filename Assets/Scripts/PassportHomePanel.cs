using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

public class PassportHomePanel : MonoBehaviour
{
    public Button _settingBtn, _editBtn, _qrBtn;
    public TextMeshProUGUI _surName, _givenName, _doBirth;
    public Image _passportImage;
    public GameObject _passportImageParent;
    public float _passportWidth;

    private UIManager _uiManager;

    private void OnEnable()
    {
        GetProfileAPiCall();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _settingBtn.onClick.AddListener(SettingBtnClick);
        _editBtn.onClick.AddListener(EditBtnClick);
        _qrBtn.onClick.AddListener(QRBtnClick);
    }


    void SettingBtnClick()
    {

    }

    void EditBtnClick()
    {
        _uiManager.SwitchScreen(12);
    }

    void QRBtnClick()
    {

    }
    public void GetProfileAPiCall()
    {
        APIManager.Instance.GetProfile(GetProfileResponce);
    }
    void GetProfileResponce(ProfileResponce responce)
    {
        if (responce.status)
        {
            responce.data.user.dob = UIManager.instance.ConvertIsoToDateOnly(responce.data.user.dob);
            _surName.text = responce.data.user.lastname;
            _givenName.text = responce.data.user.firstname;
            _doBirth.text = FormatDate(responce.data.user.dob);

            if (!string.IsNullOrWhiteSpace(responce.data.user.profile_image_url))
            {
                _passportImageParent.gameObject.SetActive(true);
                UIManager.instance.Loadnewimage(responce.data.user.profile_image_url, _passportImage, _passportWidth);
            }
            else
            {
                _passportImageParent.gameObject.SetActive(false);
            }

            UIManager.instance._profilePanel._profileData = responce;
        }
    }

    public string FormatDate(string input)
    {

        if (string.IsNullOrWhiteSpace(input))
            return null;

        // parse the input. Use TryParseExact for safety.
        // Format: year = yyyy, then day = dd, then month = MM
        if (DateTime.TryParseExact(
            input,
            "yyyy-dd-MM",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime dt))
        {
            // Now format it. We want "dd MMM/MMM yyyy".
            // MMM gives abbreviated month name, MMMM gives full month name.
            string shortMonth = dt.ToString("MMM", CultureInfo.InvariantCulture);
            string fullMonth = dt.ToString("MMM", CultureInfo.InvariantCulture);
            // (Here both are same because you said "Oct/Oct", but you could use "MMMM" for full name.)

            return $"{dt:dd} {shortMonth}/{fullMonth} {dt:yyyy}";
        }
        else
        {
            // fallback: input not in expected format
            return null;
        }
    }
}
