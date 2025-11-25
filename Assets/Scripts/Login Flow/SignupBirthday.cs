using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _enterBtn.onClick.AddListener(EnterBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void EnterBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_dateOfBirthInput.text))
        {
            Debug.Log("Enter Date of Birth");
            ErrorMessage("Enter valid Date of Birth");
        }
        else
        {
            APIManager.Instance.SignUp(_email, _firstName, _lastName, _dateOfBirthInput.text, _password, _password, SignUpResponse);
        }
    }

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(8);
    }

    void SignUpResponse(LoginResponce responces)
    {
        if (responces.status)
        {
            _uiManager.SwitchScreen(2);
            DataManager.Instance._userData = responces;
            _uiManager._splaceScreenVideo.SetDirectAudioMute(0, true);
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
