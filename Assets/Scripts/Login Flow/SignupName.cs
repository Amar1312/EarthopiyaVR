using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignupName : MonoBehaviour
{
    public Button _nextbtn, _backBtn;
    public TMP_InputField _firstNameInput, _lastNameInput;

    private UIManager _uiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _nextbtn.onClick.AddListener(NextBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(7);
    }

    void NextBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_firstNameInput.text))
        {
            Debug.Log("Enter First Name");
        }
        else if (string.IsNullOrWhiteSpace(_lastNameInput.text))
        {
            Debug.Log("Enter Last Name");
        }
        else
        {
            _uiManager._signupBirthScript._firstName = _firstNameInput.text;
            _uiManager._signupBirthScript._lastName = _lastNameInput.text;
            _uiManager.SwitchLoginScreen(9);
        }

    }

}
