// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using WPM.Poly2Tri;

namespace WPM {
    public partial class WorldMapGlobe : MonoBehaviour {

        #region Universe setup and initialization

        const string MOON_NAME = "Moon";
        const float MOON_DISTANCE_TO_EARTH = 384467f; // distance in km from center of Moon to center of Earth
        const float MOON_RADIUS_KM = 1737.4f; // radius of Moon in km

        void UpdateMoon() {
            Moon moon = Misc.FindObjectOfType<Moon>();
            Transform t;
            if (moon != null) {
                t = moon.transform;
            } else {
                t = transform.Find(MOON_NAME);
                if (t != null) {
                    t.SetParent(null, true);
                    if (moon == null) {
                        t.gameObject.AddComponent<Moon>();
                    }
                }
            }
            if (!_showMoon) {
                if (t != null)
                    DestroyImmediate(t.gameObject);
            } else {
                if (t == null) {
                    GameObject mgo = Instantiate(Resources.Load<GameObject>("Prefabs/Moon"));
                    mgo.name = MOON_NAME;
                    t = mgo.transform;
                } else {
                    t.gameObject.SetActive(true);
                }
                // Move and scale moon according to Earth dimensions
                if (_moonAutoScale) {
                    t.transform.localScale = transform.lossyScale * MOON_RADIUS_KM / EARTH_RADIUS_KM;
                    t.transform.localPosition = Misc.Vector3left * radius * MOON_DISTANCE_TO_EARTH / EARTH_RADIUS_KM;
                }
            }

        }

        void UpdateSkybox() {
            switch (_skyboxStyle) {
                case SKYBOX_STYLE.Basic:
                    RenderSettings.skybox = Resources.Load<Material>("Skybox/Starfield Basic/Starfield");
                    break;
                case SKYBOX_STYLE.MilkyWay:
                    RenderSettings.skybox = Resources.Load<Material>("Skybox/Starfield Tycho/Starfield Tycho");
                    break;
                case SKYBOX_STYLE.DualSkybox:
                    Material mat = Resources.Load<Material>("Skybox/DualSkybox/DualSkybox");
                    if (mat != null) {
                        mat = Instantiate(mat);
                        mat.SetTexture("_Environment", _skyboxEnvironmentTextureHDR);
                        RenderSettings.skybox = mat;
                    }

                    break;
            }
        }

        #endregion
    }

}