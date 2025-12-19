using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationNamePanel : MonoBehaviour
{
    public TMP_InputField _nameInputfield;
    public Button _nameBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _nameBtn.onClick.AddListener(NameBtnClick);
    }

    void NameBtnClick()
    {
        if (string.IsNullOrWhiteSpace(_nameInputfield.text))
        {

        }
        else
        {

        }
    }
}
