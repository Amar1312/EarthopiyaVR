using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using System.Globalization;
using WPM;

public class AnimalSafariPanel : MonoBehaviour
{
    public Button _vrSafariBtn, _backBtn;
    public GameObject _mapObj;

    public List<GameObject> _marker;
    public Button _zoomInBtn, _zoomOutBtn;
    public Button _changeMapBtn, _centerMapBtn;
    private Vector3 _sphereLocation;

    WorldMapGlobe map; // the globe
    private SceneController _sceneData;

    [Space]
    [Header("Location Data")]
    public List<LocationData> locations;

    [Space]
    [Header("Scroll component")]
    public TextMeshProUGUI _nameText;
    public TextMeshProUGUI _meetNameText;
    public TextMeshProUGUI _firstText;
    public List<GameObject> _uniqueChaObject;
    public TextMeshProUGUI _secondText;
    public List<GameObject> _bioRescueImage;
    public List<GameObject> _biggestThreatImage;
    public TextMeshProUGUI _biggestAcitityText;

    [Space]
    public int _locationIndex;

    private void OnEnable()
    {
        _sceneData = SceneController.Instance;
        if (_sceneData._animalData.notificationPanel)
        {
            _nameText.text = _sceneData._animalData.userAnimalName;
        }
        //_meetNameText.text = "Meet " + _sceneData._animalData.userAnimalName;
        //_firstText.text = _sceneData._animalData.firstText;
        //UniqueCheON(_sceneData._animalData.animalNumber);
        //_secondText.text = _sceneData._animalData.secondText;


        //BioRescueON(_sceneData._animalData.animalNumber);
        //BiggestThreatON(_sceneData._animalData.biggestImageIndex);
        //_biggestAcitityText.text = _sceneData._animalData.biggestAcitityText;
    }

    void BiggestThreatON(int Index)
    {
        for (int i = 0; i < _biggestThreatImage.Count; i++)
        {
            _biggestThreatImage[i].SetActive(false);
        }
        _biggestThreatImage[Index].SetActive(true);
    }

    void BioRescueON(int Index)
    {
        for (int i = 0; i < _bioRescueImage.Count; i++)
        {
            _bioRescueImage[i].SetActive(false);
        }
        _bioRescueImage[Index].SetActive(true);
    }

    void UniqueCheON(int Index)
    {
        for (int i = 0; i < _uniqueChaObject.Count; i++)
        {
            _uniqueChaObject[i].SetActive(false);
        }
        _uniqueChaObject[Index].SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _vrSafariBtn.onClick.AddListener(VrSafariBtnClick);
        _zoomInBtn.onClick.AddListener(ZoomInBtnClick);
        _zoomOutBtn.onClick.AddListener(ZoomOutBtnClick);

        _centerMapBtn.onClick.AddListener(CenterMapBtnClick);
        _changeMapBtn.onClick.AddListener(ChangeMapBtnClick);
        _backBtn.onClick.AddListener(BackBtnClick);
        _locationIndex = GetLocationIndexBySystemDate();
    }

    void VrSafariBtnClick()
    {
        StartCoroutine(StartCardboardVR());
        Debug.Log("Vr Btn Cliclk");
    }
    void BackBtnClick()
    {
        UIManager.instance.SwitchScreen(8);
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

        for (int i = 0; i < _marker.Count; i++)
        {
            _marker[i].transform.SetParent(default);
            _marker[i].transform.position = Vector3.zero;
        }
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
        if (map.tileServer == TILE_SERVER.ESRISatellite)
        {
            map.tileServer = TILE_SERVER.OpenStreeMap;
            Debug.Log("Switched to Open Street Map");
        }
        else
        {
            map.tileServer = TILE_SERVER.ESRISatellite;
            Debug.Log("Switched to ESRI Satellite");
        }
    }

    void CenterMapBtnClick()
    {
        map.FlyToLocation(_sphereLocation, 2f, 2f, 0f);
    }

    public void SetMarker()
    {
        //SetLatitudeData(22.547159f, 59.808086f, _marker[0]);
        //SetLatitudeData(23.763506f, 58.714994f, _marker[1]);
        //SetLatitudeData(22.730328f, 59.648665f, _marker[2]);
        //SetLatitudeData(22.865309f, 59.309663f, _marker[3]);

        if (_locationIndex != -1)
        {
            float MainLati = locations[_locationIndex].latitude;
            float MainLon = locations[_locationIndex].longitude;
            SetLatitudeData(MainLati, MainLon, _marker[0]);

            if (_locationIndex != 0)
            {
                float cross1Lati = locations[_locationIndex - 1].latitude;
                float cross1Lon = locations[_locationIndex - 1].longitude;
                SetLatitudeData(cross1Lati, cross1Lon, _marker[1]);
            }
        }
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

    [ContextMenu("Return Date Index")]
    public void IndexReturn()
    {
        int num = GetLocationIndexBySystemDate();
        Debug.Log(num + " Index Data");
    }

    public int GetLocationIndexBySystemDate()
    {
        DateTime today = DateTime.Today;
        DateTime nearestPreviousDate = DateTime.MinValue;
        int nearestIndex = -1;

        for (int i = 0; i < locations.Count; i++)
        {
            if (DateTime.TryParseExact(
                locations[i].date,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime listDate))
            {
                // Exact match
                if (listDate.Date == today)
                {
                    return i;
                }

                // Find nearest previous date
                if (listDate < today && listDate > nearestPreviousDate)
                {
                    nearestPreviousDate = listDate;
                    nearestIndex = i;
                }
            }
        }

        return nearestIndex; // -1 if no previous date found
    }

}
