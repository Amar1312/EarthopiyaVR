using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.XR.Management;
/// <summary>
/// Click state of QRButton
/// </summary>
public class QRButton : MonoBehaviour
{
    public static bool clicked = false;
    const string CardboardLoaderName = "CardboardXRLoader";
    const string ARCoreLoaderName = "ARCoreLoader";

    //XRManagerSettings xrManager;

    //void Awake()
    //{
    //    xrManager = XRGeneralSettings.Instance.Manager;
    //}


    void LateUpdate ()
    {
        clicked = false;
    }
    public void Click()
    {
        clicked = true;
        Debug.Log("Click");
    }
    public void ExitARButton()
    {
        //StartCoroutine(StopARModeCoroutine());
        StartCoroutine(StopXRCoroutine());
    }

    private IEnumerator StopXRCoroutine()
    {
        Debug.Log("Stopping XR...");
        UnityEngine.XR.XRSettings.enabled = false;
        yield return null;
        //xrManager.StopSubsystems();
        //xrManager.DeinitializeLoader();
        var xrManager = XRGeneralSettings.Instance.Manager;
        //xrManager.TrySetLoaders(new List<XRLoader>());

        //yield return null;
        xrManager.StopSubsystems();
        yield return null;

        xrManager.DeinitializeLoader();
        yield return null;

        //  Give Unity time to fully destroy stereo render pipeline
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);

        var cardboardLoader = xrManager.loaders.FirstOrDefault(l => l.name.Contains("ARCore"));
        xrManager.TryRemoveLoader(cardboardLoader);

        foreach (var l in xrManager.loaders.ToArray())
            xrManager.TryRemoveLoader(l);

        yield return new WaitForSeconds(0.2f);
        //  Clear Cardboard leftover objects
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("XR OFF — Normal Mode");
        SceneManager.LoadScene(0);


        //yield return new WaitForSeconds(0.5f);
        //CameraSetupManager.Instance.SetupNormalCamera();
    }


}