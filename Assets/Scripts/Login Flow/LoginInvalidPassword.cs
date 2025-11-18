using UnityEngine;
using UnityEngine.UI;

public class LoginInvalidPassword : MonoBehaviour
{
    public Button _okBtn, _backBtn;

    private UIManager _uiManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _okBtn.onClick.AddListener(OKBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void OKBtnClick()
    {
        _uiManager.SwitchLoginScreen(0);
    }

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(0);
    }
}
