using UnityEngine;
using UnityEngine.Video;

public class SplashScreen : MonoBehaviour
{
    public VideoPlayer _videoSplash;
    public Animator _animator;

    public void SwithToLogin()
    {
        UIManager.instance.SwitchScreen(1);
    }

    private void Update()
    {
        if(_videoSplash.frame > 2f)
        {
            _animator.enabled = true;
        }
    }

}
