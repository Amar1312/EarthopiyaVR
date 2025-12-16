using UnityEngine;

namespace WPM {
    public class TwoCamerasCode : MonoBehaviour {

        public WorldMapGlobe map;

        public void FlyToCountry(string countryName) {
            int countryIndex = map.GetCountryIndex(countryName);
            if (countryIndex < 0) {
                Debug.LogError("Country not found! " + countryName);
                return;
            }
            map.FlyToCountry(countryIndex);
            map.BlinkCountry(countryIndex, Color.red, Color.yellow, 4f, 0.5f);
            map.autoRotationSpeed = 0;
        }

        public void FlyToCity() {
            map.FlyToCity("United States of America", "New York");
            map.autoRotationSpeed = 0;
        }
    }


}
