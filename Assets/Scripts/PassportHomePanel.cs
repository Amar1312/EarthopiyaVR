using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Globalization;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Management;
using UnityEngine.XR;
using System.Collections.Generic;

public class PassportHomePanel : MonoBehaviour
{
    public Button _settingBtn, _editBtn, _qrBtn, _firstQrBtn;
    public TextMeshProUGUI _surName, _givenName, _doBirth, _titleName, _safariComplate, _cretateDate;
    public Image _passportImage;
    public GameObject _passportImageParent;
    public float _passportWidth;
    public ScrollRect _safariScroll;
    public GameObject _startSafari;
    public List<Image> _stampImage; // Index Must Same As QR Read Vuforiya_QR_Read Script _QRStringData List


    private UIManager _uiManager;
    private bool oneTime = false;


    private void OnEnable()
    {

        if (SceneController.Instance._gameOn == 3)
        {
            SceneController.Instance._gameOn = 0;
            Invoke(nameof(GetProfileAPiCall), 0.5f);
        }
        else
        {
            GetProfileAPiCall();
        }

        if (oneTime)
        {
            _startSafari.SetActive(false);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _settingBtn.onClick.AddListener(SettingBtnClick);
        _editBtn.onClick.AddListener(EditBtnClick);
        _qrBtn.onClick.AddListener(QRBtnClick);
        _firstQrBtn.onClick.AddListener(QRBtnClick);
        _safariScroll.onValueChanged.AddListener(OnScrollValueChanged);
    }


    void SettingBtnClick()
    {
        EditBtnClick();
    }

    void EditBtnClick()
    {
        _uiManager.SwitchScreen(12);
    }

    void QRBtnClick()
    {
        SceneController.Instance._gameOn = 3;
        SceneManager.LoadScene(2);
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        if (_startSafari.activeInHierarchy)
        {
            _startSafari.SetActive(false);
        }
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
            responce.data.user.created_at = UIManager.instance.ConvertToReadableDateForCreateDate(responce.data.user.created_at);
            _surName.text = responce.data.user.lastname;
            _givenName.text = responce.data.user.firstname;
            _titleName.text = responce.data.user.firstname + " " + responce.data.user.lastname;
            _doBirth.text = FormatDate(responce.data.user.dob);
            _cretateDate.text = responce.data.user.created_at;
            responce.data.user.dob = ConvertToSlashFormat(responce.data.user.dob);

            if (!string.IsNullOrWhiteSpace(responce.data.user.profile_image_url))
            {
                _passportImageParent.gameObject.SetActive(true);
                UIManager.instance.Loadnewimage(responce.data.user.profile_image_url, _passportImage, _passportWidth);
            }
            else
            {
                _passportImageParent.gameObject.SetActive(false);
            }
            for (int i = 0; i < _stampImage.Count; i++)
            {
                _stampImage[i].color = new Color(1f, 1f, 1f, 0.2f);
            }
            List<int> passportID = new List<int>();
            passportID = UIManager.instance.ConvertStringListToIntList(responce.data.user.passport);
            for (int i = 0; i < passportID.Count; i++)
            {
                int num = passportID[i] - 1;
                _stampImage[num].color = Color.white;
            }

            _safariComplate.text = responce.data.user.passport.Count + " Safaris Completed";
            DataManager.Instance._profileData = responce;
        }
    }

    private void OnDisable()
    {
        if (!oneTime)
        {
            _startSafari.SetActive(false);
            oneTime = true;
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
            "yyyy-MM-dd",
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

    public string ConvertToSlashFormat(string date)
    {
        //DateTime d = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        //return d.ToString("dd'/'MM'/'yyyy");

        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }
        else if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
        {
            return d.ToString("dd'/'MM'/'yyyy");
        }

        return null;
    }

}
