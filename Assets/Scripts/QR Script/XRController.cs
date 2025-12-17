using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class XRController : MonoBehaviour
{
    public IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");

        XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null || xrSettings.Manager == null)
        {
            Debug.LogError("XR General Settings or Manager not found");
            yield break;
        }

        // Initialize XR
        yield return xrSettings.Manager.InitializeLoader();

        if (xrSettings.Manager.activeLoader == null)
        {
            Debug.LogError("XR Initialization failed");
            yield break;
        }

        // Start XR subsystems
        xrSettings.Manager.StartSubsystems();
        Debug.Log("XR Started");
    }

    public void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null || xrSettings.Manager == null)
            return;

        xrSettings.Manager.StopSubsystems();
        xrSettings.Manager.DeinitializeLoader();

        Debug.Log("XR Stopped");
    }

    public void OnStartARButton()
    {
        StartCoroutine(StartXR());
    }

    public void OnExitARButton()
    {
        StopXR();
    }

    public void GoBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
