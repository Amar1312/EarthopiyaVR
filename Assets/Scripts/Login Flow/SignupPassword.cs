using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class SignupPassword : MonoBehaviour
{

    public TMP_InputField _passwordInput;
    public Button _nextBtn, _backBtn;

    private UIManager _uiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _nextBtn.onClick.AddListener(NextBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(6);
    }

    void NextBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_passwordInput.text))
        {
            Debug.Log("Enter Valid password");
        }
        else if (ValidatePassword(_passwordInput.text) == false)
        {
            Debug.Log("Enter Valid Password");
            //PasswordError();
        }
        else
        {
            _uiManager._signupBirthScript._password = _passwordInput.text;
            _uiManager.SwitchLoginScreen(8);
        }

    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            //PasswordError();
            Debug.Log("Please Enter password");
            return false;
        }

        if (password.Length < 8)
        {

            Debug.Log("Password must be at least 8 characters");
            return false;
        }

        bool containsUppercase = false;
        bool containsLowercase = false;
        bool containsDigit = false;
        bool containsSpecialChar = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c))
                containsUppercase = true;
            else if (char.IsLower(c))
                containsLowercase = true;
            else if (char.IsDigit(c))
                containsDigit = true;
            else if (char.IsSymbol(c) || char.IsPunctuation(c))
                containsSpecialChar = true;
        }

        if (!containsUppercase || !containsLowercase || !containsDigit || !containsSpecialChar)
        {
            Debug.Log("password must be alphanumeric");
            //PasswordError();
        }

        return containsUppercase && containsLowercase && containsDigit && containsSpecialChar;
    }
}
