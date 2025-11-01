using UnityEngine;
using UnityEngine.UI;

public class AnimalScrollBtn : MonoBehaviour
{
    public GameObject _unSelectItem, _selectItem;
    public GameObject _defaultItem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SelectImage()
    {
        _unSelectItem.SetActive(false);
        _selectItem.SetActive(true);

    }

    public void UnSelectImage()
    {
        _selectItem.SetActive(false);
        _unSelectItem.SetActive(true);

        if (_defaultItem != null)
        {
            _defaultItem.SetActive(false);
        }
    }
}
