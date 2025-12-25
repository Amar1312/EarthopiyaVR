using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using System.Text;
using Jaimin;


public class APIQRRead : Singleton<APIQRRead>
{
    private const string APIBASEURL = "https://app.earthopya.com/api";
    private const string UPDATEPASSPORT = APIBASEURL + "/profile/passport";
    public string ApiToken;

    public GameObject _loadingPanel;
    public Vuforiya_QR_Read _qRReadScript;
    public QRScanningPanel _qRScanningScript;
    public QRErrorPanel _qRErrorScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.HasKey("ApiToken"))
        {
            ApiToken = PlayerPrefs.GetString("ApiToken");
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
        _loadingPanel.SetActive(true);

        UpdatePassportClass _rawdata = new UpdatePassportClass(passport_ids);
        string rawstring = JsonUtility.ToJson(_rawdata);

        UnityWebRequest request = new UnityWebRequest(UPDATEPASSPORT, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(rawstring);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + ApiToken);

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
        }
        else
        {
            callback = JsonUtility.FromJson<UpdatePassportResponce>(request.downloadHandler.text);
            callback.status = false;
            if (callback.message == "")
                callback.message = "Something went worng";
            Debug.Log("Update Passport Error: " + request.downloadHandler.text);
        }

        _loadingPanel.SetActive(false);

        response.Invoke(callback);

        request.Dispose();
    }

    #endregion







    #region DummyClass
    public class UpdatePassportClass
    {
        public List<string> passport_ids;
        public UpdatePassportClass(List<string> Passport_id)
        {
            passport_ids = Passport_id;
        }
    }
    #endregion
}
