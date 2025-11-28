using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPanel : MonoBehaviour
{
    public TMP_InputField _emaliInput, _passwordInput;
    public Button _loginBtn, _forgotBtn, _signupBtn;

    private UIManager _uiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _loginBtn.onClick.AddListener(LoginBtnClick);
        _forgotBtn.onClick.AddListener(ForgotBtnClick);
        _signupBtn.onClick.AddListener(SignUPBtnClick);
    }

    void LoginBtnClick()
    {
        string email = _emaliInput.text;
        string password = _passwordInput.text;

        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            Debug.Log("Login API call");

            APIManager.Instance.LoginIn(email, password, LoginInResponse);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _uiManager.SwitchLoginScreen(2);
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                _uiManager.SwitchLoginScreen(1);
            }
        }
    }

    void ForgotBtnClick()
    {
        _uiManager.SwitchLoginScreen(3);
    }

    void SignUPBtnClick()
    {
        _uiManager.SwitchLoginScreen(6);
    }

    void LoginInResponse(LoginResponce responces)
    {
        if (responces.status)
        {
            _uiManager.SwitchScreen(2);
            DataManager.Instance._userData = responces;
            _uiManager._splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else
        {
            if (responces.message == "Entered email is invalid.")
            {
                _uiManager.SwitchLoginScreen(2);
            }
            else if (responces.message == "Entered password is invalid.")
            {
                _uiManager.SwitchLoginScreen(1);
            }
            else
            {
                _uiManager.SwitchLoginScreen(2);
            }
        }
    }
}
