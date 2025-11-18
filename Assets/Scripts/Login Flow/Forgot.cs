using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Forgot : MonoBehaviour
{
    public TMP_InputField _emailInput;
    public Button _resetBtn, _backBtn;

    private UIManager _uiManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _backBtn.onClick.AddListener(BackBtnClick);
        _resetBtn.onClick.AddListener(ResetBtnClick);
    }

    void ResetBtnClick()
    {
        Debug.Log("Forget API Call");
    }

    void BackBtnClick()
    {
        _uiManager.SwitchLoginScreen(0);
    }
}
