using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnterPasswordDeleteAccount : MonoBehaviour
{
    public TMP_InputField _passwordInput;
    public Button _confirmBtn,_backBtn;
    public DeleteAccountPanel _deletePanel;
    public GameObject _errorPassword;
    public ConformDeleteAccount _confirmPanel;

    private void OnEnable()
    {
        OffErrorPassword(); 
        _passwordInput.text = "";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _confirmBtn.onClick.AddListener(ConfirmBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void ConfirmBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_passwordInput.text))
        {
            _errorPassword.SetActive(true);
            Invoke(nameof(OffErrorPassword), 2f);
            return;
        }
        string Email = DataManager.Instance._profileData.data.user.email;
        int delete = 0;
        APIManager.Instance.DeleteAccount(Email, _passwordInput.text, delete, DeleteAccount);
    }

    void DeleteAccount(DeleteAccountResponce responce)
    {
        if (responce.status)
        {
            _confirmPanel._passWord = _passwordInput.text;
            _deletePanel.OnOnePanel(2);
            _passwordInput.text = "";
        }
        else
        {
            _errorPassword.SetActive(true);
            Invoke(nameof(OffErrorPassword), 2f);
        }
    }

    void OffErrorPassword()
    {
        _errorPassword.SetActive(false);
    }

    void BackBtnClick()
    {
        _deletePanel.OnOnePanel(0);
    }

}
