using UnityEngine;
using System;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;

public class SelectAnimalScroll : MonoBehaviour
{
    public SimpleScrollSnap _scroll;
    public SimpleScrollSnap _mainScroll;
    public SimpleScrollSnap _dummyScroll;
    public RectTransform _contant;
    public RectTransform _imageContant;

    public List<AnimalScrollBtn> _scrollBtn;
    public List<MainImageScroll> _scrollImage;

    public AirplaneMovement _airplane;
    public List<RectTransform> _destinetionPoint;
    public List<GameObject> _titleName;
    public GameObject _selectTitle;

    public int _pointIndex;

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
    }
    public void SelectImageItem()
    {
        if (_scroll.gameObject.activeInHierarchy)
        {
            SetData();
        }
        else
        {
            _dummyScroll.gameObject.SetActive(false);
            _scroll.gameObject.SetActive(true);
            Invoke(nameof(SetData), 0.05f);
        }
    }

    void SetData()
    {
        int Index = _mainScroll._selectItem;
        _scroll.GoToPanel(Index);
    }

    public void SelectBtnItem()
    {
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
        _airplane.SetDestinetion(_destinetionPoint[Index]);

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

        for (int i = 0; i < _scrollImage.Count; i++)
        {
            _scrollImage[i].DefaultOff();
        }
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

}
