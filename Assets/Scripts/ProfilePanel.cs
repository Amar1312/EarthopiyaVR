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
    public Button _saveBtn, _backBtn, _delateBtn, _dialCodeBtn;
    public GameObject _editPassportText;
    public Image _profileImage;
    public GameObject _maskingImage;
    public GameObject _updateSuccess;
    public TextMeshProUGUI _errorMessage, _dialText;
    public GameObject _dialCodeScroll;

    private UIManager _uiManager;

    [Header("Login Method")]
    public List<GameObject> _methodLogo;

    public SigninSampleScript _googleLoginIn;

    private bool isUpdating = false;
    private string lastDigits = "";
    private void OnEnable()
    {
        SetProfileData(DataManager.Instance._profileData);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _saveBtn.onClick.AddListener(SaveBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
        _delateBtn.onClick.AddListener(DelateBtnClick);
        _dialCodeBtn.onClick.AddListener(DialBtnClick);

        _firstName.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
        _lastName.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
        _dateofBirth.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
        _number.onEndEdit.AddListener(delegate { ValueChangeCheck(); });

        _dateofBirth.characterValidation = TMP_InputField.CharacterValidation.Integer;
        _dateofBirth.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string value)
    {
        if (isUpdating) return;

        string digits = "";
        foreach (char c in value)
            if (char.IsDigit(c)) digits += c;

        if (digits.Length > 8)
            digits = digits.Substring(0, 8);

        string formatted = digits;
        if (digits.Length > 2)
            formatted = digits.Insert(2, "/");
        if (digits.Length > 4)
            formatted = formatted.Insert(5, "/");

        isUpdating = true;
        _dateofBirth.SetTextWithoutNotify(formatted);
        StartCoroutine(FixCaretNextFrame());   // <-- FIX CARET JUMP
        isUpdating = false;
    }

    private IEnumerator FixCaretNextFrame()
    {
        yield return null;   // wait 1 frame so Unity finishes internal update
        _dateofBirth.caretPosition = _dateofBirth.text.Length;  // caret at end (fixed)
    }

    void ValueChangeCheck()
    {
        _saveBtn.gameObject.SetActive(true);
    }

    void SetProfileData(ProfileResponce responce)
    {
        _firstName.text = responce.data.user.firstname;
        _lastName.text = responce.data.user.lastname;
        _dateofBirth.text = responce.data.user.dob;
        _number.text = responce.data.user.phone_no;
        _email.text = responce.data.user.email;
        //Debug.Log(responce.data.user.firstname + " N " + DataManager.Instance._profileData.data.user.firstname);

        if (!string.IsNullOrWhiteSpace(responce.data.user.profile_image_url))
        {
            _profileImage.gameObject.SetActive(true);
            UIManager.instance.Loadnewimage(responce.data.user.profile_image_url, _profileImage, 251f);
            _maskingImage.SetActive(true);
        }
        else
        {
            _profileImage.gameObject.SetActive(false);
            _maskingImage.SetActive(false);
        }

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
        if (!ValidateFinalDate())
        {
            ErrorMessage("Enter Valid Date");
            return;
        }
        string Date = ConverDate(_dateofBirth.text);
        Debug.Log(Date);

        APIManager.Instance.UpdateProfile(_firstName.text, _lastName.text, Date, _number.text, _profileImage, UpdateProfile);
    }

    void DialBtnClick()
    {
        _dialCodeScroll.SetActive(true);
    }

    public void SetDialCode(string code)
    {
        _dialText.text = code;
        _dialCodeScroll.SetActive(false);
    }

    void UpdateProfile(ProfileResponce responce)
    {
        if (responce.status)
        {
            UpdateSuccess();
            responce.data.user.created_at = _uiManager.ConvertToReadableDateForCreateDate(responce.data.user.created_at);
            DataManager.Instance._profileData = responce;
        }
    }

    void DelateBtnClick()
    {
        _uiManager.SwitchScreen(13);
    }

    public void UpdateSuccess()
    {
        _updateSuccess.SetActive(true);
        _backBtn.gameObject.SetActive(false);
        _editPassportText.SetActive(false);

        _saveBtn.gameObject.SetActive(false);
        Invoke(nameof(OffSuccess), 2f);
    }
    public void OffSuccess()
    {
        _updateSuccess.SetActive(false);
        _backBtn.gameObject.SetActive(true);
        _editPassportText.SetActive(true);
    }

    public bool ValidateFinalDate()
    {
        string dateStr = _dateofBirth.text;

        DateTime date;

        bool valid = DateTime.TryParseExact(
            dateStr,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date
        );

        Debug.Log(valid ? " Valid Date" : " Invalid Date");
        return valid;
    }
    public void ErrorMessage(string _message)
    {
        _errorMessage.text = _message;
        _errorMessage.gameObject.SetActive(true);
        Invoke(nameof(OffMessage), 2f);
    }
    void OffMessage()
    {
        _errorMessage.gameObject.SetActive(false);
    }

    public string ConverDate(string inputDate)
    {
        if (string.IsNullOrWhiteSpace(inputDate))
        {
            return null;
        }
        else
        {
            DateTime parsedDate = DateTime.ParseExact(inputDate, "dd/MM/yyyy", null);
            // Convert to yyyy-MM-dd
            string outputDate = parsedDate.ToString("yyyy-MM-dd");
            return outputDate;
        }
    }
}
