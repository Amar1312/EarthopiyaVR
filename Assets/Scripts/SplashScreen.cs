using UnityEngine;
using UnityEngine.Video;

public class SplashScreen : MonoBehaviour
{
    public VideoPlayer _videoSplash;
    public Animator _animator;

    private void Start()
    {

    }

    public void SwithToLogin()
    {
        if (PlayerPrefs.HasKey("Login"))
        {
            UIManager.instance.SwitchScreen(2);
            UIManager.instance._splaceScreenVideo.SetDirectAudioMute(0, true);
        }
        else
        {
            UIManager.instance.SwitchScreen(1);
        }
        
    }

    private void Update()
    {
        if(_videoSplash.frame > 2f)
        {
            _animator.enabled = true;
        }
    }

}
