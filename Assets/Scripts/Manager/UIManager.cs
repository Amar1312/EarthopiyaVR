using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Globalization;

public class UIManager : MonoBehaviour
{

    #region Variables
    public static UIManager instance;
    public List<GameObject> screens;
    public List<GameObject> loginScreens;
    public SignupBirthday _signupBirthScript;
    public GameObject _loadingPnel;
    public VideoPlayer _splaceScreenVideo;
    [Space]
    [Header("Ticket Booking Script;")]
    public BookFlightScreen _bookFlightScript;
    public SearchDetailPanel _searchDetailScript;
    public FlightDetailPanel _flightDetailScript;
    public ChooseSeatPanel _chooseSeatScript;
    public BoardingPassPanel _boardingPassScript;

    [Space]
    [Header("Bottom Bar Button")]
    public Button _ticketsBtnSafari;
    public Button _passportBtnSafari, _safariBtnTicket, _passportBtnTicket, _ticketBtnPassport, _safariBtnPassport;

    [Space]
    [Header("Bottom Bar Button")]
    public PassportHomePanel _passportPanel;
    public ProfilePanel _profilePanel;

    #endregion



    // Ensure only one instance of UIManager exists
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 30;

        _ticketsBtnSafari.onClick.AddListener(TicketBtnClick);
        _ticketBtnPassport.onClick.AddListener(TicketBtnClick);
        _safariBtnTicket.onClick.AddListener(SafariBtnClick);
        _safariBtnPassport.onClick.AddListener(SafariBtnClick);
        _passportBtnSafari.onClick.AddListener(PassportBtnClick);
        _passportBtnTicket.onClick.AddListener(PassportBtnClick);
    }

    void TicketBtnClick()
    {
        SwitchScreen(2);
    }

    void SafariBtnClick()
    {
        SwitchScreen(8);
    }

    void PassportBtnClick()
    {
        SwitchScreen(11);
    }

    // Switch to a specific screen by index
    public void SwitchScreen(int screen)
    {
        for (int i = 0; i < screens.Count; i++)
        {
            if (i == screen)
            {
                screens[i].SetActive(true);
            }
            else
            {
                screens[i].SetActive(false);
            }
        }
    }

    public void SwitchLoginScreen(int screen)
    {
        for (int i = 0; i < loginScreens.Count; i++)
        {
            if (i == screen)
            {
                loginScreens[i].SetActive(true);
            }
            else
            {
                loginScreens[i].SetActive(false);
            }
        }
    }

    public void ToggleLoadingPanel(bool status)
    {
        _loadingPnel.SetActive(status);
    }

    public void ButtonClick()
    {
        Debug.Log("Button Click");
    }


    public void Loadnewimage(string ImageUrl, Image _image, float maxWidth)
    {
        //Debug.Log("first Load Image............................................................................");
        string url = ImageUrl.Replace(@"\", "");
        //Debug.Log("URL : " + url);
        Davinci.get()
       .load(url)
       .into(_image)
       .withStartAction(() =>
       {
           //statusTxt.text = "Download has been started.";
           //Debug.Log("Download Is Started..................");
           //UIManager.UIInstance.ToggleLoadingPanel(true);
       })
       .withDownloadProgressChangedAction((progress) =>
       {
           //statusTxt.text = "Download progress: " + progress;
       })
       .withDownloadedAction(() =>
       {
           //statusTxt.text = "Download has been completed.";
           //Debug.Log("Download Is Completed..................");
       })
       .withLoadedAction(() =>
       {
           // statusTxt.text = "Image has been loaded.";
           //Debug.Log("Image Is Downloaded.........................");
       })
       .withErrorAction((error) =>
       {
           // statusTxt.text = "Got error : " + error;
           Debug.Log("Error : " + error);
           //UIManager.UIInstance.ToggleLoadingPanel(false);
       })
       .withEndAction(() =>
       {
           //print("Operation has been finished.");
           //UIManager.UIInstance.ToggleLoadingPanel(false);


           SetMaxSize(maxWidth, _image);
       })

       .setFadeTime(0.1f)
       .setCached(true)
       .start();
        //_loadOnce = true;
    }



    public void SetMaxSize(float maxWidth, Image _meImage)
    {
        // Get the aspect ratio of the image
        float aspectRatio = _meImage.sprite.rect.width / _meImage.sprite.rect.height;

        // Calculate the new size based on the maximum width and height
        float newWidth = Mathf.Min(maxWidth, _meImage.sprite.rect.width);

        if (newWidth < maxWidth)
        {
            newWidth = maxWidth;
        }

        float newHeight = newWidth / aspectRatio;
        //Debug.Log(newWidth + " new Width " + _sliderDataList.id);

        //if (newHeight > maxHeight)
        //{
        //    newHeight = maxHeight;
        //    newWidth = newHeight * aspectRatio;
        //    //Debug.Log(newHeight + " new Hight " + _sliderDataList.id);
        //}


        // Set the size
        RectTransform rectTransform = _meImage.rectTransform;
        rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }


    public string ConvertIsoToDateOnly(string isoString)
    {
        if (string.IsNullOrEmpty(isoString))
            return null;
        if (DateTimeOffset.TryParseExact(
                        isoString,
                        "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTimeOffset dto))
        {
            return dto.ToString("yyyy-MM-dd");
        }
        else
        {
            if (DateTime.TryParse(isoString, out DateTime dt))
            {
                return dt.ToString("yyyy-MM-dd");
            }
        }

        return null;
    }
}
