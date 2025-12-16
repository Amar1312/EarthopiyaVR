using UnityEngine;
using WPM;

public class DemoHighlight : MonoBehaviour
{
    public WorldMapGlobe map;

    void Update()
    {
        if (map.input.GetKeyDown("space")) {
            // highlights the country at the center of screen
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            if (map.GetCountryIndex(ray, out int countryIndex, out int regionIndex)) {
                map.HighlightCountry(countryIndex, refreshGeometry: false, drawOutline: true, outlineColor: Color.white);
                Debug.Log($"Country highlighted: {map.countryHighlighted.name}");
            }
        }

        if (map.input.GetKeyDown("c")) {
            // hides the selection
            map.HideCountryRegionHighlights(false);
        }
        
    }
}
