using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;
using System;
using WPM;

public class AnimalSafariPanel : MonoBehaviour
{
    public Button _vrSafariBtn;
    public GameObject _mapObj;

    public List<GameObject> _marker;
    public Button _zoomInBtn, _zoomOutBtn;
    public Button _changeMapBtn, _centerMapBtn;
    private Vector3 _sphereLocation;

    WorldMapGlobe map; // the globe


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //map = WorldMapGlobe.instance;
        _vrSafariBtn.onClick.AddListener(VrSafariBtnClick);
        _zoomInBtn.onClick.AddListener(ZoomInBtnClick);
        _zoomOutBtn.onClick.AddListener(ZoomOutBtnClick);

        _centerMapBtn.onClick.AddListener(CenterMapBtnClick);
        _changeMapBtn.onClick.AddListener(ChangeMapBtnClick);

    }

    void VrSafariBtnClick()
    {
        StartCoroutine(StartCardboardVR());
        Debug.Log("Vr Btn Cliclk");
    }

    public void OnMapBox()
    {
        _mapObj.SetActive(true);
        // Get a reference to the World Map API:
        map = WorldMapGlobe.instance;
        map.autoRotationSpeed = 0.03f;
        //SetMarker();
        Invoke(nameof(SetMarker), 1f);

    }

    public void OffMapBox()
    {
        _mapObj.SetActive(false);
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

        var cardboardLoader = xrManager.loaders.FirstOrDefault(l => l.name.Contains("Cardboard"));

        xrManager.TrySetLoaders(new List<XRLoader> { cardboardLoader });

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

    void ChangeMapBtnClick()
    {
        int currentEarthStyle = (int)map.earthStyle;
        currentEarthStyle = (currentEarthStyle + 1) % 20;
        map.earthStyle = (EARTH_STYLE)currentEarthStyle;
    }

    void CenterMapBtnClick()
    {
        //Debug.Log("Btn Click" + cityIndex);
        //int countryIndex = map.GetCountryIndex(_sphereLocation);
        int countryIndex = map.GetCountryNearPoint(_sphereLocation);
        //float zoomLevel = map.GetCountryMainRegionZoomExtents(countryIndex);
        //map.FlyToCountry(countryIndex, 2f, zoomLevel, 0.5f);
        map.FlyToCountry(countryIndex, 2f, 2f, 0f);
    }

    public void SetMarker()
    {
        //SetLatitudeData(22.292622716664944f, 70.78425536746414f, _marker[0]);
        SetLatitudeData(22.547159f, 59.808086f, _marker[0]);
        SetLatitudeData(23.763506f, 58.714994f, _marker[1]);
        SetLatitudeData(22.730328f, 59.648665f, _marker[2]);
        SetLatitudeData(22.865309f, 59.309663f, _marker[3]);
    }

    public void SetLatitudeData(float LatDec, float LonDec, GameObject marker)
    {
        marker.transform.SetParent(default);
        marker.transform.rotation = Quaternion.identity;

        float phi = LatDec * Mathf.Deg2Rad;
        float theta = (LonDec + 90.0f) * Mathf.Deg2Rad;

        float fromX = Mathf.Cos(phi) * Mathf.Cos(theta);
        float fromY = Mathf.Sin(phi);
        float fromZ = Mathf.Cos(phi) * Mathf.Sin(theta);

        Vector3 sphereLocation = new Vector3(fromX, fromY, fromZ) * 0.5f;
        map.AddMarker(marker, sphereLocation, 1);
        if (marker == _marker[0])
        {
            _sphereLocation = sphereLocation;
        }

        //map.FlyToLocation(sphereLocation);
    }


    void ZoomInBtnClick()
    {
        if (map.autoRotationSpeed != 0f)
        {
            map.autoRotationSpeed = 0f;
        }
        float num = map.GetZoomLevel();
        num = num - 0.01f;
        float prevZoomLevel = Mathf.Clamp(num, 0, 2);
        map.SetZoomLevel(prevZoomLevel);
    }


    void ZoomOutBtnClick()
    {
        if (map.autoRotationSpeed != 0f)
        {
            map.autoRotationSpeed = 0f;
        }
        float num = map.GetZoomLevel();
        num = num + 0.01f;
        float prevZoomLevel = Mathf.Clamp(num, 0, 2);
        map.SetZoomLevel(prevZoomLevel);
    }

}
