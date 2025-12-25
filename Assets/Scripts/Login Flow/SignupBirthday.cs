using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Globalization;

public class SignupBirthday : MonoBehaviour
{
    public Button _enterBtn, _backBtn;
    public TMP_InputField _dateOfBirthInput;
    public TextMeshProUGUI _errorMessage;

    public string _email;
    public string _password;
    public string _firstName;
    public string _lastName;

    private UIManager _uiManager;
    private bool isUpdating = false;
    private string lastDigits = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _enterBtn.onClick.AddListener(EnterBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);

        _dateOfBirthInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
        _dateOfBirthInput.onValueChanged.AddListener(OnInputChanged);
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
        _dateOfBirthInput.SetTextWithoutNotify(formatted);
        StartCoroutine(FixCaretNextFrame());   // <-- FIX CARET JUMP
        isUpdating = false;

    }
    private IEnumerator FixCaretNextFrame()
    {
        yield return null;   // wait 1 frame so Unity finishes internal update
        _dateOfBirthInput.caretPosition = _dateOfBirthInput.text.Length;  // caret at end (fixed)
    }

    void EnterBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_dateOfBirthInput.text))
        {
            Debug.Log("Enter Date of Birth");
            ErrorMessage("Enter valid Date of Birth");
        }
        else if (!ValidateFinalDate())
        {
            Debug.Log("Enter Valid Date of Birth");
            ErrorMessage("Enter Valid Date");
        }
        else
        {
            Debug.Log("Sign UP API Call");
            string Date = ConverDate(_dateOfBirthInput.text);
            APIManager.Instance.SignUp(_email, _firstName, _lastName, Date, _password, _password, SignUpResponse);
        }
    }

    public string ConverDate(string inputDate)
    {
        DateTime parsedDate = DateTime.ParseExact(inputDate, "dd/MM/yyyy", null);

        // Convert to yyyy-MM-dd
        string outputDate = parsedDate.ToString("yyyy-MM-dd");
        return outputDate;
    }

    public bool ValidateFinalDate()
    {
        string dateStr = _dateOfBirthInput.text;

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

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(8);
    }

    void SignUpResponse(LoginResponce responces)
    {
        if (responces.status)
        {
            _uiManager.SwitchScreen(8);
            DataManager.Instance._userData = responces;
            _uiManager._splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else
        {
            ErrorMessage(responces.message);
        }
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
}
