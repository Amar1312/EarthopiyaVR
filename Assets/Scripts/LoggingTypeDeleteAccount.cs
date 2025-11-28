using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoggingTypeDeleteAccount : MonoBehaviour
{
    public Button _confirmBtn, _googleBtn, _appleBtn, _backBtn;
    public TextMeshProUGUI _loggingText;
    public DeleteAccountPanel _deletePanel;

    private void OnEnable()
    {
        ProfileResponce responce = DataManager.Instance._profileData;

        if (!string.IsNullOrWhiteSpace(responce.data.user.apple_id))
        {
            _loggingText.text = "Apple";
            _appleBtn.gameObject.SetActive(true);
            _googleBtn.gameObject.SetActive(false);
            _confirmBtn.gameObject.SetActive(false);
        }
        else if (!string.IsNullOrWhiteSpace(responce.data.user.google_id))
        {
            _loggingText.text = "Google";
            _googleBtn.gameObject.SetActive(true);
            _appleBtn.gameObject.SetActive(false);
            _confirmBtn.gameObject.SetActive(false);
        }
        else
        {
            _loggingText.text = "Email";
            _confirmBtn.gameObject.SetActive(true);
            _googleBtn.gameObject.SetActive(false);
            _appleBtn.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        _confirmBtn.onClick.AddListener(ConfirmBtnClick);
        _googleBtn.onClick.AddListener(GoogleBtnClick);
        _appleBtn.onClick.AddListener(AppleBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void ConfirmBtnClick()
    {
        _deletePanel.OnOnePanel(1);
    }

    void GoogleBtnClick()
    {
        _deletePanel.OnOnePanel(2);
    }

    void AppleBtnClick()
    {
        _deletePanel.OnOnePanel(2);
    }

    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(12);
    }
}
