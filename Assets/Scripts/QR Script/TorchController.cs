using UnityEngine;
using UnityEngine.UI;


public class TorchController : MonoBehaviour
{
    AndroidJavaObject torchPlugin;
    bool isOn = false;
    public Text buttonText;
    public Button _flashBtn;

    void Start()
    {
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            torchPlugin = new AndroidJavaObject("com.unity.torchonly.FlashTorchOnly", activity);
        }

        _flashBtn.onClick.AddListener(ToggleFlashlight);
    }

    public void ToggleFlashlight()
    {
        isOn = !isOn;
        torchPlugin.Call("setTorch", isOn);
        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = isOn ? "Flash OFF" : "Flash ON";
        }
    }
}
