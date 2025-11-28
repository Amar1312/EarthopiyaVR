using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

public class AnimalSafariPanel : MonoBehaviour
{
    public Button _vrSafariBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _vrSafariBtn.onClick.AddListener(VrSafariBtnClick);
    }

    void VrSafariBtnClick()
    {
        StartCoroutine(StartCardboardVR());
        Debug.Log("Vr Btn Cliclk");

    }

    private IEnumerator StartCardboardVR()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        UnityEngine.XR.XRSettings.enabled = false;          //  VERY IMPORTANT FIX
        yield return null;                     // allow pipeline to update

        var xrManager = XRGeneralSettings.Instance.Manager;

        //  FULL CLEANUP before initializing
        xrManager.StopSubsystems();
        xrManager.DeinitializeLoader();
        yield return new WaitForSeconds(0.1f);

        //  Initialize XR
        yield return xrManager.InitializeLoader();
        if (xrManager.activeLoader == null)
        {
            Debug.LogError("Cardboard initialization FAILED");
            yield break;
        }

        xrManager.StartSubsystems();
        UnityEngine.XR.XRSettings.enabled = true;        //  Enable XR after subsystems start

        //  REQUIRED DELAY (fixes Cardboard double panel)
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.15f);

        SceneController.Instance._gameOn = 1;
        SceneManager.LoadScene(1);
    }

}
