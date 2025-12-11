using System.Runtime.InteropServices;
using UnityEngine;

public static class FlashlightPlugin
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _SetTorchEnabled(bool enabled);
    
    [DllImport("__Internal")]
    private static extern bool _IsTorchAvailable();
#endif

    public static void SetTorchEnabled(bool enabled)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _SetTorchEnabled(enabled);
#else
        Debug.Log($"iOS Torch would be set to: {enabled}");
#endif
    }

    public static bool IsTorchAvailable()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _IsTorchAvailable();
#else
        return false;
#endif
    }
}
