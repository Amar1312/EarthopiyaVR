using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class DeleteAccountPanel : MonoBehaviour
{
    public List<GameObject> _allpanel;
    public Button _cancelBtn;

    private void OnEnable()
    {
        OnOnePanel(0);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cancelBtn.onClick.AddListener(CancelBtnClick);
    }

    void CancelBtnClick()
    {
        UIManager.instance.SwitchScreen(12);
    }

    public void OnOnePanel(int Index)
    {
        for (int i = 0; i < _allpanel.Count; i++)
        {
            _allpanel[i].SetActive(false);
        }
        _allpanel[Index].SetActive(true);
    }
}
