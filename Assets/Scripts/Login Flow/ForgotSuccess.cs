using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForgotSuccess : MonoBehaviour
{
    public Button _okBtn, _backBtn;

    private UIManager _uiManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _backBtn.onClick.AddListener(BackBtnClick);
        _okBtn.onClick.AddListener(OkBtnClick);
    }

    void OkBtnClick()
    {
        _uiManager.SwitchLoginScreen(0);
    }

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(3);
    }
}
