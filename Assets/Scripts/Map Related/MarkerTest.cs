using UnityEditor;
using UnityEngine;
using WPM;

public class MarkerTest : MonoBehaviour
{
    WorldMapGlobe map; // the globe

    public GameObject _marker;
    public GameObject _marker1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get a reference to the World Map API:
        map = WorldMapGlobe.instance;
    }

    public void SetMarker()
    {
        SetLatitudeData(22.730328f, 59.648665f, _marker);
    }
    public void SetMarker1()
    {
        SetLatitudeData(22.865309f, 59.309663f, _marker1);
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
        map.FlyToLocation(sphereLocation);
    }

    public void ZoomIn()
    {
        float num = map.GetZoomLevel();
        num = num - 0.01f;
        float prevZoomLevel = Mathf.Clamp(num, 0, 2);
        map.SetZoomLevel(prevZoomLevel);
    }

    public void ZoomOut()
    {
        float num = map.GetZoomLevel();
        num = num + 0.01f;
        float prevZoomLevel = Mathf.Clamp(num, 0, 2);
        map.SetZoomLevel(prevZoomLevel);

    }
}
