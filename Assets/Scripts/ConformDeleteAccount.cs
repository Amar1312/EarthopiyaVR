using UnityEngine;
using UnityEngine.UI;
using SignInSample;

public class ConformDeleteAccount : MonoBehaviour
{

    public SigninSampleScript _googleLoginIn;
    public Button _yesDeleteBtn;
    public string _passWord;
    private string _type;

    public Button _backBtn;
    public DeleteAccountPanel _deletePane;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _yesDeleteBtn.onClick.AddListener(YesDeleteBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
    }

    void YesDeleteBtnClick()
    {
        ProfileResponce responce = DataManager.Instance._profileData;

        if (!string.IsNullOrWhiteSpace(responce.data.user.apple_id))
        {
            string apple_id = responce.data.user.apple_id;
            string Type = "apple";
            _type = Type;
            APIManager.Instance.DeleteSocialAccount(Type, apple_id, DeleteAccount);
        }
        else if (!string.IsNullOrWhiteSpace(responce.data.user.google_id))
        {
            string google_id = responce.data.user.google_id;
            string Type = "google";
            _type = Type;
            APIManager.Instance.DeleteSocialAccount(Type, google_id, DeleteAccount);
        }
        else
        {
            string Email = DataManager.Instance._profileData.data.user.email;
            int delete = 1;
            _type = "Email";
            APIManager.Instance.DeleteAccount(Email, _passWord, delete, DeleteAccount);
        }
    }


    void DeleteAccount(DeleteAccountResponce responce)
    {
        if (responce.status)
        {
            PlayerPrefs.DeleteKey("ApiToken");
            PlayerPrefs.DeleteKey("Login");
            UIManager.instance.SwitchLoginScreen(0);
            UIManager.instance.SwitchScreen(1);
            if(_type == "google")
            {
                _googleLoginIn.OnSignOut();
            }
        }

    }

    void BackBtnClick()
    {
        _deletePane.OnOnePanel(0);
    }

}
