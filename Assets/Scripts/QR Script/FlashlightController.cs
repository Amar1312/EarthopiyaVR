using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    private bool isOn = false;

    public void ToggleFlashlight()
    {
        isOn = !isOn;

#if UNITY_ANDROID
        AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
        int cameraID = 0;
        AndroidJavaObject camera = cameraClass.CallStatic<AndroidJavaObject>("open", cameraID);
        AndroidJavaObject cameraParams = camera.Call<AndroidJavaObject>("getParameters");

        if (isOn)
        {
            cameraParams.Call("setFlashMode", "torch");
        }
        else
        {
            cameraParams.Call("setFlashMode", "off");
        }

        camera.Call("setParameters", cameraParams);
        camera.Call("startPreview");
#endif

#if UNITY_IOS
        _ToggleFlashlight(iOS_FlashState: isOn);
#endif
    }

#if UNITY_IOS
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _ToggleFlashlight(bool iOS_FlashState);
#endif
}
