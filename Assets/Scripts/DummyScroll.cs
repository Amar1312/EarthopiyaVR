using UnityEngine;
using System;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;

public class DummyScroll : MonoBehaviour
{

    public SimpleScrollSnap _dummyScroll;
    public SimpleScrollSnap _selectAnimal;

    public GameObject _dummyScrollObject;
    public GameObject _mainScrollObject;

    public SelectAnimalScroll _selectScroll;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void SelctOneItem()
    {
        if (_dummyScroll._selectItem != 0)
        {
            _mainScrollObject.SetActive(true);
            Invoke(nameof(SetData), 0.05f);
            _dummyScrollObject.SetActive(false);
        }
    }

    public void SetData()
    {
        int Index = _dummyScroll._selectItem - 1;
        _selectAnimal.GoToPanel(Index);

        _selectScroll.ScrollSelectBtn(Index);

    }
}
