//-----------------------------------------------------------------------
// <copyright file="CardboardStartup.cs" company="Google LLC">
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using Google.XR.Cardboard;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes Cardboard XR Plugin.
/// </summary>
public class CardboardStartup : MonoBehaviour
{
    private bool isExitingVR = false;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    public void Start()
    {
        // Configures the app to not shut down the screen and sets the brightness to maximum.
        // Brightness control is expected to work only in iOS, see:
        // https://docs.unity3d.com/ScriptReference/Screen-brightness.html.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;

        // Checks if the device parameters are stored and scans them if not.
        if (!Api.HasDeviceParams())
        {
            Api.ScanDeviceParams();
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    public void Update()
    {
        if (isExitingVR)
        {
            return;
        }

        if (Api.IsGearButtonPressed)
        {
            Api.ScanDeviceParams();
        }

        if (Api.IsCloseButtonPressed)
        {
            //Application.Quit();
            ExitVR();
        }

        if (Api.IsTriggerHeldPressed)
        {
            Api.Recenter();
        }

        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }

        Api.UpdateScreenParams();
    }

    public void ExitVR()
    {
        StartCoroutine(StopCardboardVR());
    }

    private IEnumerator StopCardboardVR()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        //  First disable XR rendering
        UnityEngine.XR.XRSettings.enabled = false;
        yield return null;

        var xrManager = XRGeneralSettings.Instance.Manager;

        xrManager.StopSubsystems();
        yield return null;

        xrManager.DeinitializeLoader();
        yield return null;

        //  Give Unity time to fully destroy stereo render pipeline
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        //var cardboardLoader = xrManager.loaders.FirstOrDefault(l => l.name.Contains("Cardboard"));
        //xrManager.TryRemoveLoader(cardboardLoader);

        //foreach (var l in xrManager.loaders.ToArray())
        //    xrManager.TryRemoveLoader(l);

        //yield return new WaitForSeconds(0.2f);

        //  Clear Cardboard leftover objects
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        isExitingVR = true;
        SceneManager.LoadScene(0);

    }

    
}
