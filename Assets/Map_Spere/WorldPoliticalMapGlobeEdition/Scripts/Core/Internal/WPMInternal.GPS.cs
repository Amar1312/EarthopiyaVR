// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System.Collections;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        #region GPS stuff

        IEnumerator CheckGPS() {
            if (!Application.isPlaying)
                yield break;
            while (_followDeviceGPS) {
                if (!flyToActive && input.location.isEnabledByUser) {
                    if (input.location.status == LocationServiceStatus.Stopped) {
                        input.location.Start();
                    } else if (input.location.status == LocationServiceStatus.Running) {
                        float latitude = input.location.lastData.latitude;
                        float longitude = input.location.lastData.longitude;
                        FlyToLocation(latitude, longitude);
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        void OnApplicationPause(bool pauseState) {

            if (!_followDeviceGPS || !Application.isPlaying)
                return;

            if (pauseState) {
                input.location.Stop();
            }

        }

        #endregion

    }

}