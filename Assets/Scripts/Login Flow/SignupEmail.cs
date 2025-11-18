using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
public class SignupEmail : MonoBehaviour
{
    public TMP_InputField _emailInput;
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
        _uiManager.SwitchLoginScreen(0);
    }

    void NextBtnClick()
    {
        bool value = validateEmail(_emailInput.text);
        if (string.IsNullOrWhiteSpace(_emailInput.text))
        {
            Debug.Log("Enter Valid Email");
            return;
        }
        else if (!value)
        {
            //_emailErrorMessage.SetActive(true);
            Debug.Log("Enter Valid Email");

        }
        else
        {
            //_emailErrorMessage.SetActive(false);
            Debug.Log("Email is valid");
            _uiManager._signupBirthScript._email = _emailInput.text;
            _uiManager.SwitchLoginScreen(7);
        }

    }


    public static bool validateEmail(string email)
    {
        if (email != null)
            return Regex.IsMatch(email, MatchEmailPattern);
        else
            return false;
    }

    public const string MatchEmailPattern =
        @"^([0-9a-zA-Z]" + //Start with a digit or alphabetical
        @"([\+\-_\.][0-9a-zA-Z]+)*" + // No continuous or ending +-_. chars in email
        @")+" +
        @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

}
