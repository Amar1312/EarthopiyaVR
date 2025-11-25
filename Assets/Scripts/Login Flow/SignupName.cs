using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignupName : MonoBehaviour
{
    public Button _nextbtn, _backBtn;
    public TMP_InputField _firstNameInput, _lastNameInput;
    public TextMeshProUGUI _errorMessage;

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
            ErrorMessage("Enter Valid First Name");
        }
        else if (string.IsNullOrWhiteSpace(_lastNameInput.text))
        {
            Debug.Log("Enter Last Name");
            ErrorMessage("Enter Valid Last Name");
        }
        else
        {
            _uiManager._signupBirthScript._firstName = _firstNameInput.text;
            _uiManager._signupBirthScript._lastName = _lastNameInput.text;
            _uiManager.SwitchLoginScreen(9);
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
