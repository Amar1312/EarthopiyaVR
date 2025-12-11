
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    AndroidJavaObject unityActivity;
    AndroidJavaClass flashClass;

    void Start()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        flashClass = new AndroidJavaClass("com.flash.FlashController");
        flashClass.CallStatic("init", unityActivity);
    }

    public void ToggleFlash()
    {
        flashClass.CallStatic("toggleFlash", unityActivity);
    }
}
