using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotificationNamePanel : MonoBehaviour
{
    public TMP_InputField _nameInputfield;
    public Button _nameBtn;
    private UIManager _uiManager;

    public AnimalData _animalData;
    public List<GameObject> _mainImageObject;
    public List<GameObject> _bottomObject;

    private void OnEnable()
    {
        BottomObjectON(_animalData.animalNumber);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _nameBtn.onClick.AddListener(NameBtnClick);
    }

    void NameBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_nameInputfield.text))
        {

        }
        else
        {
            SceneController.Instance._animalData = _animalData;
            SceneController.Instance._animalData.userAnimalName = _nameInputfield.text;
            _uiManager.SwitchScreen(10);
        }
    }

    void BottomObjectON(int Index)
    {
        for (int i = 0; i < _bottomObject.Count; i++)
        {
            _bottomObject[i].SetActive(false);
        }
        _bottomObject[Index].SetActive(true);

        for (int i = 0; i < _mainImageObject.Count; i++)
        {
            _mainImageObject[i].SetActive(false);
        }
        _mainImageObject[Index].SetActive(true);
    }
}
