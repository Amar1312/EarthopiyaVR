using UnityEngine;
using System;
using System.Globalization;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SignInSample;

public class ProfilePanel : MonoBehaviour
{
    public TMP_InputField _firstName, _lastName, _dateofBirth, _number, _email;
    public Button _saveBtn, _backBtn, _delateBtn;
    public Image _profileImage;
    public GameObject _updateSuccess;

    private UIManager _uiManager;

    [Header("Login Method")]
    public List<GameObject> _methodLogo;

    public SigninSampleScript _googleLoginIn;
    public ProfileResponce _profileData;

    private void OnEnable()
    {
        SetProfileData(_profileData);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _saveBtn.onClick.AddListener(SaveBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
        _delateBtn.onClick.AddListener(DelateBtnClick);

        _firstName.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
        _lastName.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
        _dateofBirth.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
        _number.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
    }

    void ValueChangeCheck()
    {
        _saveBtn.gameObject.SetActive(true);
    }

    void SetProfileData(ProfileResponce responce)
    {
        if (responce.status)
        {
            _profileData = responce;

            _firstName.text = responce.data.user.firstname;
            _lastName.text = responce.data.user.lastname;
            _dateofBirth.text = /*FormatDateToUpperMonth(*/responce.data.user.dob/*)*/;
            _number.text = responce.data.user.phone_no;
            _email.text = responce.data.user.email;

            UIManager.instance.Loadnewimage(responce.data.user.profile_image_url, _profileImage, 251f);

            if (!string.IsNullOrWhiteSpace(responce.data.user.apple_id))
            {
                MethodLogo(2);
            }
            else if (!string.IsNullOrWhiteSpace(responce.data.user.google_id))
            {
                MethodLogo(1);
            }
            else
            {
                MethodLogo(0);
            }

            _saveBtn.gameObject.SetActive(false);
        }
    }

    void MethodLogo(int num)
    {
        for (int i = 0; i < _methodLogo.Count; i++)
        {
            _methodLogo[i].SetActive(false);
        }
        _methodLogo[num].SetActive(true);
    }

    void BackBtnClick()
    {
        _uiManager.SwitchScreen(11);
        _profileImage.sprite = null;
    }

    void SaveBtnClick()
    {
        Debug.Log(_firstName.text);
        APIManager.Instance.UpdateProfile(_firstName.text, _lastName.text, _dateofBirth.text, _number.text, _profileImage, UpdateProfile);
    }

    void UpdateProfile(UpdateProfileResponce responce)
    {
        if (responce.status)
        {
            UpdateSuccess();
        }
    }

    void DelateBtnClick()
    {
        if (!string.IsNullOrWhiteSpace(_profileData.data.user.google_id))
        {
            _googleLoginIn.OnSignOut();
            PlayerPrefs.DeleteKey("ApiToken");
            PlayerPrefs.DeleteKey("Login");
            _uiManager.SwitchLoginScreen(0);
            _uiManager.SwitchScreen(1);
        }
    }

    public void UpdateSuccess()
    {
        _updateSuccess.SetActive(true);
        _saveBtn.gameObject.SetActive(false);
        Invoke(nameof(OffSuccess), 2f);
    }
    public void OffSuccess()
    {
        _updateSuccess.SetActive(false);
    }

    public string FormatDateToUpperMonth(string input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        // Parse exact with the format year-day-month
        if (DateTime.TryParseExact(
                input,
                "yyyy-dd-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt))
        {
            // Format day and year normally, month as MMM then uppercase
            string formatted = dt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            // Make the month part uppercase — easiest way is to ToUpper the whole thing
            // since day and year are digits
            return formatted.ToUpper();
        }
        else
        {
            // Parsing failed
            return null;
        }
    }
}
