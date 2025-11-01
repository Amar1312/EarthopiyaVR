using UnityEngine;
using UnityEngine.UI;

public class DestinetionPoint : MonoBehaviour
{
    public Image _passportImage;
    public Animation _animation;

    private void OnEnable()
    {
        //Invoke(nameof(OnpassPort), 0.5f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnpassPort()
    {
        _passportImage.gameObject.SetActive(true);
        _animation.enabled = true;
    }

    public void OffPassPort()
    {
        _passportImage.gameObject.SetActive(false);
        _animation.enabled = false;
    }

    private void OnDisable()
    {
        
    }
}
