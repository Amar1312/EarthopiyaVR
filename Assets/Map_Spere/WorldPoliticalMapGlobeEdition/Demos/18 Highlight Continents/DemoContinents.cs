using System.Collections.Generic;
using UnityEngine;

namespace WPM {

    public class DemoContinents : MonoBehaviour {

        public WorldMapGlobe map;
        public Color continentFillColor = Color.green;
        public Color continentOutlineColor = Color.black;

        readonly Dictionary<string, List<int>> continentCountryIndices = new Dictionary<string, List<int>>();
        readonly Dictionary<string, GameObject> activeOutlines = new Dictionary<string, GameObject>();

        void Start () {

            Country[] countries = map.countries;
            if (countries == null) return;

            // Get the countries for each continent
            int countryCount = countries.Length;
            for (int k = 0; k < countryCount; k++) {
                Country country = countries[k];
                if (continentCountryIndices.TryGetValue(country.continent, out List<int> indices)) {
                    indices.Add(k);
                } else {
                    continentCountryIndices[country.continent] = new List<int> { k };
                }
            }

            // Add event listeners
            map.OnCountryEnter += OnCountryEnter;
            map.OnCountryExit += OnCountryExit;
        }

        void OnCountryEnter (int countryIndex, int regionIndex) {
            string continent = map.countries[countryIndex].continent;
            if (!continentCountryIndices.TryGetValue(continent, out List<int> indices)) {
                return;
            }

            if (activeOutlines.TryGetValue(continent, out GameObject previousOutline) && previousOutline != null) {
                Destroy(previousOutline);
                activeOutlines.Remove(continent);
            }

            // Color countries
            int indexCount = indices.Count;
            for (int k = 0; k < indexCount; k++) {
                map.ToggleCountrySurface(indices[k], true, continentFillColor);
            }

            // Add an outline to the countries
            GameObject outline = map.DrawCountriesOutline(indices, continentOutlineColor);
            if (outline != null) {
                activeOutlines[continent] = outline;
            }
        }

        void OnCountryExit (int countryIndex, int regionIndex) {
            string continent = map.countries[countryIndex].continent;
            if (!continentCountryIndices.TryGetValue(continent, out List<int> indices)) {
                return;
            }

            //Destroy the outline
            if (activeOutlines.TryGetValue(continent, out GameObject outline) && outline != null) {
                Destroy(outline);
                activeOutlines.Remove(continent);
            }

            // Hide the countries
            int indexCount = indices.Count;
            for (int k = 0; k < indexCount; k++) {
                map.ToggleCountrySurface(indices[k], false);
            }
        }

    }

}