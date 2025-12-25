using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Globalization;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

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
    [Header("Ticket Booking Script")]
    public BookFlightScreen _bookFlightScript;
    public SearchDetailPanel _searchDetailScript;
    public FlightDetailPanel _flightDetailScript;
    public ChooseSeatPanel _chooseSeatScript;
    public BoardingPassPanel _boardingPassScript;
    public NoFlightDetailPanel _noFlightDetailScript;
    [Space]
    [Header("Safari Selecter Script")]
    public SelectAnimalScroll _safariSelectScript;
    public NotificationNamePanel _notificationNameScript;
    public AnimalSafariInfoController _allAnimalSafariScript;

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

        int startPanel = SceneController.Instance._gameOn;
        if (startPanel == 1)
        {
            SwitchScreen(10);
            _splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else if (startPanel == 2)
        {
            SwitchScreen(8);
            _splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else if (startPanel == 3)
        {
            SwitchScreen(11);
            _splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else if (startPanel == 4)
        {
            SwitchScreen(2);
            _splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else
        {
            SwitchScreen(0);
        }

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

    public void LoadSVGImage(string imageUrl, Image targetImage, float maxWidth)
    {
        StartCoroutine(LoadSVG(imageUrl, targetImage, maxWidth));
    }

    private IEnumerator LoadSVG(string url, Image targetImage, float maxWidth)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("SVG Download Error: " + req.error);
            yield break;
        }

        byte[] svgBytes = req.downloadHandler.data;

        Texture2D tex = SVGToTexture.ConvertSVGToTexture(svgBytes, 512, 512);

        targetImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        SetMaxSize(maxWidth, targetImage);
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

    public List<int> ConvertStringListToIntList(List<string> stringList)
    {
        List<int> intList = new List<int>();

        if (stringList == null)
            return intList;

        foreach (string s in stringList)
        {
            if (int.TryParse(s, out int value))
            {
                intList.Add(value);
            }
        }

        return intList;
    }

    public string ConvertToReadableDateForCreateDate(string isoDate)
    {
        DateTime dateTime = DateTime.Parse(
            isoDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal
        );

        return dateTime.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
    }
}
