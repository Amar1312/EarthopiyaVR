using UnityEngine;

public class MainImageScroll : MonoBehaviour
{
    public GameObject _defaultObj;
    public GameObject _normalObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DefaultOff()
    {
        if (_defaultObj != null && _normalObj != null)
        {
            _defaultObj.SetActive(false);
            _normalObj.SetActive(true);
        }
        
    }
}
