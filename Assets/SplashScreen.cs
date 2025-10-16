using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    public void SwithToLogin()
    {
        UIManager.instance.SwitchScreen(1);
    }
}
