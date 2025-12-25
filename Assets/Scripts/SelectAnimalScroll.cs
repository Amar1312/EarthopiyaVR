using UnityEngine;
using System;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectAnimalScroll : MonoBehaviour
{
    public SimpleScrollSnap _scroll;
    public SimpleScrollSnap _mainScroll;
    public SimpleScrollSnap _dummyMainScroll;
    public SimpleScrollSnap _dummyScroll;
    public RectTransform _contant;
    public RectTransform _imageContant;
    public GameObject _bottomRightTree;

    public List<AnimalScrollBtn> _scrollBtn;
    public List<MainImageScroll> _scrollImage;

    public AirplaneMovement _airplane;
    public List<RectTransform> _destinetionPoint;
    public List<RectTransform> _mapDestinetionPoint;
    public List<GameObject> _titleName;
    public GameObject _selectTitle;
    public Button _qrBtn, _firstQrBtn;

    public int _pointIndex;

    [Space]
    [Header("First Show Panel")]
    public List<GameObject> _goToSafariPanel;
    public List<GameObject> _safariAllScroll;

    [Space]
    [Header("Scroll All Component")] // Index Must Same As QR Read Vuforiya_QR_Read Script _QRStringData List
    public List<MainImageScroll> _dummyMainScrollComponent;
    public List<MainImageScroll> _mainScrollComponent;

    private void OnEnable()
    {
        if (SceneController.Instance._gameOn == 2)
        {
            SceneController.Instance._gameOn = 0;
            Invoke(nameof(GetProfileAPiCall), 0.5f);
        }
        else
        {
            GetProfileAPiCall();
        }
    }
    public void GetProfileAPiCall()
    {
        APIManager.Instance.GetProfile(GetProfileResponce);
    }
    void GetProfileResponce(ProfileResponce responce)
    {
        if (responce.status)
        {
            if (responce.data.user.passport.Count != 0)
            {
                SafariON(true);
            }
            else
            {
                SafariON(false);
            }

            for (int i = 0; i < _dummyMainScrollComponent.Count; i++)
            {
                _dummyMainScrollComponent[i].LockComponent(true);
            }
            for (int i = 0; i < _mainScrollComponent.Count; i++)
            {
                _mainScrollComponent[i].LockComponent(true);
            }

            List<int> passportID = new List<int>();
            passportID = UIManager.instance.ConvertStringListToIntList(responce.data.user.passport);

            for (int i = 0; i < passportID.Count; i++)
            {
                int num = passportID[i] - 1;
                _mainScrollComponent[num].LockComponent(false);
                _dummyMainScrollComponent[num].LockComponent(false);
            }

        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform item in _contant)
        {
            _scrollBtn.Add(item.gameObject.GetComponent<AnimalScrollBtn>());
        }
        foreach (Transform item in _imageContant)
        {
            _scrollImage.Add(item.gameObject.GetComponent<MainImageScroll>());
        }
        _qrBtn.onClick.AddListener(QRBtnClick);
        _firstQrBtn.onClick.AddListener(QRBtnClick);
    }
    public void SelectImageItem()
    {
        if (_dummyMainScroll.gameObject.activeInHierarchy)
        {
            ChangeImageScroll();
        }
        if (_scroll.gameObject.activeInHierarchy)
        {
            int Index = _mainScroll._selectItem;

            SetData();
        }
        else
        {
            _dummyScroll.gameObject.SetActive(false);
            _bottomRightTree.SetActive(false);
            _scroll.gameObject.SetActive(true);
            Invoke(nameof(SetData), 0.05f);
        }
    }

    public void SelectdummyImageItem()
    {
        if (_dummyMainScroll._selectItem != 0)
        {
            int Index = _dummyMainScroll._selectItem - 1;
            ChangeImageScroll();

            _pointIndex = Index;
            ScrollSelectBtn(Index);
        }
    }

    void SetData()
    {

        int Index = _mainScroll._selectItem;
        _scroll.GoToPanel(Index);
    }

    public void SelectBtnItem()
    {
        if (_dummyMainScroll.gameObject.activeInHierarchy)
        {
            ChangeImageScroll();
        }
        int Index = _scroll._selectItem;
        _pointIndex = Index;

        ScrollSelectBtn(Index);

    }

    public void ScrollSelectBtn(int Index)
    {
        for (int i = 0; i < _destinetionPoint.Count; i++)
        {
            _destinetionPoint[i].gameObject.SetActive(false);
        }
        _destinetionPoint[Index].gameObject.SetActive(true);
        _airplane.SetDestinetion(_destinetionPoint[Index], _mapDestinetionPoint[Index]);

        for (int i = 0; i < _titleName.Count; i++)
        {
            _titleName[i].gameObject.SetActive(false);
        }
        _titleName[Index].gameObject.SetActive(true);
        _selectTitle.SetActive(false);

        for (int i = 0; i < _scrollBtn.Count; i++)
        {
            _scrollBtn[i].UnSelectImage();
        }
        _scrollBtn[Index].SelectImage();

        _mainScroll.GoToPanel(Index);

        Debug.Log(Index);
    }


    public void OnDestination()
    {

        for (int i = 0; i < _destinetionPoint.Count; i++)
        {
            _destinetionPoint[i].GetComponent<DestinetionPoint>().OffPassPort();
        }
        _destinetionPoint[_pointIndex].GetComponent<DestinetionPoint>().OnpassPort();
    }

    public void ChangeImageScroll()
    {
        _dummyMainScroll.gameObject.SetActive(false);
        _bottomRightTree.SetActive(false);
        _mainScroll.gameObject.SetActive(true);
    }

    public void QRBtnClick()
    {
        SceneController.Instance._gameOn = 2;
        SceneManager.LoadScene(2);
    }

    public void SafariON(bool on)
    {
        for (int i = 0; i < _goToSafariPanel.Count; i++)
        {
            _goToSafariPanel[i].SetActive(!on);
        }
        for (int i = 0; i < _safariAllScroll.Count; i++)
        {
            _safariAllScroll[i].SetActive(on);
        }
    }

}
