using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Text;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;
    private const string APIBASEURL = "";
    private const string LOGIN = APIBASEURL + "/signin";
    private const string SIGNUP = APIBASEURL + "/signup";
    private const string SOCIALLOGIN = APIBASEURL + "/social-login";
    private const string GETPROFILE = APIBASEURL + "/profile";
    private const string UPDATEPROFILE = APIBASEURL + "/profile/update";
    private const string UPDATEPASSPORT = APIBASEURL + "/profile/passport";
    private const string DELETEACCOUNT = APIBASEURL + "/delete-account";
    private const string DELETESOCIALACCOUNT = APIBASEURL + "/delete-social-account";
    private const string LOGOUT = APIBASEURL + "/logout";


    public string ApiToken;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region Check_InternetConnection 

    private void CheckInternet(Action<bool> action)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            action(true);
        }
        else
        {
            action(false);
        }
    }
    #endregion


    #region Login 
    public void LoginIn(string _emailAddress, string _password, Action<LoginResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(LoginInIEnum(_emailAddress, _password, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator LoginInIEnum(string email, string password, Action<LoginResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        LoginClass _rawdata = new LoginClass(email, password);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(LOGIN, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Content-Type", "application/json");
        //request.SetRequestHeader("Accept", "application/json");

        var callback = new LoginResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Login Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<LoginResponce>(request.downloadHandler.text);
            callback.status = true;
            ApiToken = callback.data.token;
            PlayerPrefs.SetString("ApiToken", ApiToken);
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("Login unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
        }
        else
        {
            callback = JsonUtility.FromJson<LoginResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Login Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region SignUP 

    public void SignUp(string _email, string _firstName, string _lastName, string _DateOFBirth, string _password, string _passwordConform, Action<LoginResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(SignUpIEnum(_email, _firstName, _lastName, _DateOFBirth, _password, _passwordConform, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator SignUpIEnum(string email, string firstName, string lastName, string Dob, string password, string password_con, Action<LoginResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        SignUpClass _rawdata = new SignUpClass(email, firstName, lastName, Dob, password, password_con);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(SIGNUP, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Content-Type", "application/json");
        //request.SetRequestHeader("Accept", "application/json");
        //request.SetRequestHeader("platform", "WEB");

        var callback = new LoginResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Sign UP Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<LoginResponce>(request.downloadHandler.text);
            callback.status = true;
            ApiToken = callback.data.token;
            PlayerPrefs.SetString("ApiToken", ApiToken);
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("Sign UP unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
        }
        else
        {
            callback = JsonUtility.FromJson<LoginResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Sign UP Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region SocialLogin

    public void SocialLogin(string provider, string provider_id, string email, string firstname, string lastname, Action<LoginResponce> response)
    {
        // For google Login provider = "google" , provider_id = google_id
        // for apple Login provider = "apple"  , provider_id = apple_id
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(SocialLoginInIEnum(provider, provider_id, email, firstname, lastname, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator SocialLoginInIEnum(string provider, string provider_id, string email, string firstname, string lastname, Action<LoginResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        SocialLoginClass _rawdata = new SocialLoginClass(provider, provider_id, email, firstname, lastname);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(SOCIALLOGIN, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Content-Type", "application/json");
        //request.SetRequestHeader("Accept", "application/json");
        //request.SetRequestHeader("platform", "WEB");
        //request.SetRequestHeader("Authorization", "Bearer " + ApiToken);

        var callback = new LoginResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
        //Debug.Log(request.responseCode);
        if (request.responseCode == 200 || request.responseCode == 201)
        {
            callback = JsonUtility.FromJson<LoginResponce>(request.downloadHandler.text);
            callback.status = true;
            Debug.Log("Social Login Response: " + request.downloadHandler.text);
            ApiToken = callback.data.token;
            PlayerPrefs.SetString("ApiToken", ApiToken);
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("SocialLogin unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<LoginResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Social Login Error: " + request.downloadHandler.text);

        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region Get Profile 
    public void GetProfile(Action<ProfileResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(GetProfileIEnum(response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator GetProfileIEnum(Action<ProfileResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        GetProfileClass _rawdata = new GetProfileClass();
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(GETPROFILE, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Authorization", "Bearer " + ApiToken);
        //request.SetRequestHeader("Accept", "application/json");

        var callback = new ProfileResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Get Profile Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<ProfileResponce>(request.downloadHandler.text);
            callback.status = true;
            PlayerPrefs.SetString("ApiToken", ApiToken);
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("unAuthorized Get Profile");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<ProfileResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Get Profile Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }
    #endregion

    #region Update Profile

    public void UpdateProfile(string _firstName, string _lastName, string _DateOFBirth, string _phone_no, string _profile_image, Action<UpdateProfileResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(UpdateProfileIEnum(_firstName, _lastName, _DateOFBirth, _phone_no, _profile_image, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator UpdateProfileIEnum(string firstName, string lastName, string Dob, string phone_no, string profile_image, Action<UpdateProfileResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        UpdateProfileClass _rawdata = new UpdateProfileClass(firstName, lastName, Dob, phone_no, profile_image);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(UPDATEPROFILE, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Authorization", "Bearer " + ApiToken);
        //request.SetRequestHeader("Accept", "application/json");
        //request.SetRequestHeader("platform", "WEB");

        var callback = new UpdateProfileResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Update Profile Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<UpdateProfileResponce>(request.downloadHandler.text);
            callback.status = true;
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("Update Profile unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<UpdateProfileResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Update Profile Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region Update Passport

    public void UpdatePassport(List<string> passport_ids, Action<UpdatePassportResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(UpdatePassportIEnum(passport_ids, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator UpdatePassportIEnum(List<string> passport_ids, Action<UpdatePassportResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        UpdatePassportClass _rawdata = new UpdatePassportClass(passport_ids);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(UPDATEPASSPORT, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + ApiToken);
        //request.SetRequestHeader("Accept", "application/json");
        //request.SetRequestHeader("platform", "WEB");

        var callback = new UpdatePassportResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Update Passport Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<UpdatePassportResponce>(request.downloadHandler.text);
            callback.status = true;
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("Update Passport unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<UpdatePassportResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Update Passport Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region Delete Account 
    public void DeleteAccount(string _emailAddress, string _password, Action<DeleteAccountResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(DeleteAccountIEnum(_emailAddress, _password, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator DeleteAccountIEnum(string email, string password, Action<DeleteAccountResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        DeleteAccountClass _rawdata = new DeleteAccountClass(email, password);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(DELETEACCOUNT, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", "Bearer " + ApiToken);

        var callback = new DeleteAccountResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Delete Account Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<DeleteAccountResponce>(request.downloadHandler.text);
            callback.status = true;

        }
        else if (request.responseCode == 401)
        {
            Debug.Log("Delete Account unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<DeleteAccountResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Delete Account Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region Delete Social Account 
    public void DeleteSocialAccount(string provider, string provider_id, Action<DeleteAccountResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(DeleteSocialAccountIEnum(provider, provider_id, response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator DeleteSocialAccountIEnum(string provider, string provider_id, Action<DeleteAccountResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        DeleteSocialAccountClass _rawdata = new DeleteSocialAccountClass(provider, provider_id);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(DELETESOCIALACCOUNT, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        //request.SetRequestHeader("Authorization", "Bearer " + ApiToken);

        var callback = new DeleteAccountResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("Delete Social Account Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<DeleteAccountResponce>(request.downloadHandler.text);
            callback.status = true;

        }
        else if (request.responseCode == 401)
        {
            Debug.Log("Delete Social Account unAuthorized");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<DeleteAccountResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Delete Social Account Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion

    #region LogOut
    public void LogOut(Action<LogOutResponce> response)
    {
        CheckInternet(status =>
        {
            if (status)
                StartCoroutine(LogOutIEnum(response));
            else
                Debug.Log("Error No Internet Connection");
        });
    }

    private IEnumerator LogOutIEnum(Action<LogOutResponce> response)
    {
        //UiPanelManager.Instance.ToggleLoadingPanel(true);

        LogOutClass _rawdata = new LogOutClass();
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(LOGOUT, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("api-key", "MAGICPIX");
        request.SetRequestHeader("Authorization", "Bearer " + ApiToken);
        //request.SetRequestHeader("Accept", "application/json");

        var callback = new LogOutResponce();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.responseCode == 200 || request.responseCode == 201)
        {
            Debug.Log("LogOut Response: " + request.downloadHandler.text);
            callback = JsonUtility.FromJson<LogOutResponce>(request.downloadHandler.text);
            callback.status = true;
        }
        else if (request.responseCode == 401)
        {
            Debug.Log("unAuthorized LogOut");
            callback.status = false;
            callback.message = "Unauthorized";
            //UnauthorizedUser();
        }
        else
        {
            callback = JsonUtility.FromJson<LogOutResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("LogOut Error: " + request.downloadHandler.text);
        }

        //UiPanelManager.Instance.ToggleLoadingPanel(false);

        response.Invoke(callback);

        request.Dispose();
    }
    #endregion







    #region DummyClass
    public class LoginClass
    {
        public string email;
        public string password;
        public LoginClass(string Email, string Password)
        {
            email = Email;
            password = Password;
        }
    }

    public class SignUpClass
    {
        public string email;
        public string firstname;
        public string lastname;
        public string dob;
        public string password;
        public string password_confirmation;
        public SignUpClass(string Email, string FirstName, string LastName, string Bod, string Password, string Pass_Con)
        {
            email = Email;
            firstname = FirstName;
            lastname = LastName;
            dob = Bod;
            password = Password;
            password_confirmation = Pass_Con;
        }
    }

    public class SocialLoginClass
    {
        public string provider;
        public string provider_id;
        public string email;
        public string firstname;
        public string lastname;
        public SocialLoginClass(string Provider, string Provider_id, string Email, string Firstname, string Lastname)
        {
            email = Email;
            provider = Provider;
            provider_id = Provider_id;
            firstname = Firstname;
            lastname = Lastname;
        }
    }

    public class GetProfileClass
    {
        public GetProfileClass()
        {

        }
    }

    public class UpdateProfileClass
    {
        public string firstname;
        public string lastname;
        public string dob;
        public string phone_no;
        public string profile_image;
        public UpdateProfileClass(string FirstName, string LastName, string Bod, string Phone_no, string Profile_image)
        {
            firstname = FirstName;
            lastname = LastName;
            dob = Bod;
            phone_no = Phone_no;
            profile_image = Profile_image;
        }
    }

    public class UpdatePassportClass
    {
        public List<string> passport_ids;
        public UpdatePassportClass(List<string> Passport_id)
        {
            passport_ids = Passport_id;
        }
    }

    public class DeleteAccountClass
    {
        public string email;
        public string password;
        public DeleteAccountClass(string Email, string Password)
        {
            email = Email;
            password = Password;
        }
    }

    public class DeleteSocialAccountClass
    {
        public string provider;
        public string provider_id;
        public DeleteSocialAccountClass(string Provider, string Provider_id)
        {
            provider = Provider;
            provider_id = Provider_id;
        }
    }

    public class LogOutClass
    {
        public LogOutClass()
        {

        }
    }

    #endregion
}
