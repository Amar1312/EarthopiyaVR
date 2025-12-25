using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainImageScroll : MonoBehaviour
{
    public List<GameObject> _lockObject;
    public List<GameObject> _unLockObject;

    public Button _startBtn;
    public AnimalData _animmalData;
    private UIManager _uiManager;

    [Space]
    [Header("Selection Point Image")]
    public Image _selectionPointImage;
    public Sprite _rightSprite, _wrongSprite;

    [Space]
    public bool _showNotificationPage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _uiManager = UIManager.instance;
        _startBtn.onClick.AddListener(StartBtnClick);
    }

    public void LockComponent(bool isLock)
    {
        for (int i = 0; i < _lockObject.Count; i++)
        {
            _lockObject[i].SetActive(isLock);
        }
        for (int i = 0; i < _unLockObject.Count; i++)
        {
            _unLockObject[i].SetActive(!isLock);
        }

        if (_selectionPointImage != null)
        {
            if (isLock)
            {
                _selectionPointImage.sprite = _wrongSprite;
            }
            else
            {
                _selectionPointImage.sprite = _rightSprite;
            }
        }
    }

    void StartBtnClick()
    {
        if (_showNotificationPage)
        {
            _uiManager._notificationNameScript._animalData = _animmalData;
            _uiManager._notificationNameScript._animalData.notificationPanel = _showNotificationPage;
            _uiManager.SwitchScreen(9);
        }
        else
        {
            SceneController.Instance._animalData = _animmalData;
            SceneController.Instance._animalData.notificationPanel = _showNotificationPage;
            _uiManager.SwitchScreen(10);
        }
    }

}
