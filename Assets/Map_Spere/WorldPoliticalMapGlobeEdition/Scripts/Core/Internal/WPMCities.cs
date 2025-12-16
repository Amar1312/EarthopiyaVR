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

        const float CITY_HIT_PRECISION = 0.001f;
        const string CITY_ATTRIB_DEFAULT_FILENAME = "citiesAttrib";

        #region Internal variables

        List<City> _cities;

        // resources
        Material citiesNormalMat, citiesRegionCapitalMat, citiesCountryCapitalMat;
        GameObject citiesLayer;
        MaterialPropertyBlock cityProps;

        #endregion

        // internal cache
        City[] visibleCities;

        /// <summary>
        /// City look up dictionary. Used internally for fast searching of city objects.
        /// </summary>
        Dictionary<City, int> _cityLookup;
        int lastCityLookupCount = -1;

        Dictionary<City, int> cityLookup {
            get {
                if (_cityLookup != null && cities.Count == lastCityLookupCount)
                    return _cityLookup;
                if (_cityLookup == null) {
                    _cityLookup = new Dictionary<City, int>();
                } else {
                    _cityLookup.Clear();
                }
                if (cities != null) {
                    int cityCount = cities.Count;
                    for (int k = 0; k < cityCount; k++)
                        _cityLookup[cities[k]] = k;
                }
                lastCityLookupCount = _cityLookup.Count;
                return _cityLookup;
            }
        }


        #region System initialization

        bool ValidCityIndex (int cityIndex) {
            return cityIndex >= 0 && cities != null && cityIndex < _cities.Count;
        }

        void ReadCitiesGeoData () {
            ReadCitiesGeoData("cities10");
        }

        void ReadCitiesGeoData (string filename) {
            string cityCatalogFileName = _geodataResourcesPath + "/" + filename;

            TextAsset ta = null;
            if (_geodataFormat == GEODATA_FORMAT.BinaryFormat) {
                // try to load in binary form first
                ta = Resources.Load<TextAsset>(cityCatalogFileName + "_bin");
                if (ta != null) {
                    SetCitiesGeoDataBinary(ta.bytes);
                } else {
                    Debug.LogWarning("Geodata format is set to binary but cities binary file can't be found or read.");
                }
            }

            if (ta == null) {
                // load legacy packed string format
                ta = Resources.Load<TextAsset>(cityCatalogFileName);
                if (ta != null) {
                    SetCitiesGeoData(ta.text);
                } else {
                    Debug.LogWarning("Cities packed string format file can't be read.");
                }
            }

            if (ta != null) {
                ReloadCitiesAttributes();
                Resources.UnloadAsset(ta);
            }
        }


        void ReloadCitiesAttributes () {
            TextAsset ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/" + _cityAttributeFile);
            if (ta == null)
                return;
            SetCitiesAttributes(ta.text);
            Resources.UnloadAsset(ta);
        }





        #endregion

        #region IO stuff

        /// <summary>
        /// Returns the file name corresponding to the current city data file
        /// </summary>
        public string GetCityGeoDataFileName () {
            return "cities10.txt";
        }


        /// <summary>
        /// Returns the file name corresponding to the current country data file (countries10, countries110)
        /// </summary>
        public string GetCityGeoDataBinaryFileName () {
            return "cities10_bin.txt";
        }

        /// <summary>
        /// Exports the geographic data in packed string format.
        /// </summary>
        public string GetCitiesGeoData () {
            if (cities == null) return null;
            StringBuilder sb = new StringBuilder();
            int citiesCount = cities.Count;
            for (int k = 0; k < citiesCount; k++) {
                City city = cities[k];
                if (k > 0)
                    sb.Append("|");
                sb.Append(city.name);
                sb.Append("$");
                if (city.province != null && city.province.Length > 0) {
                    sb.Append(city.province);
                    sb.Append("$");
                } else {
                    sb.Append("$");
                }
                sb.Append(countries[city.countryIndex].name);
                sb.Append("$");
                sb.Append(city.population);
                sb.Append("$");
                sb.Append((int)(city.localPosition.x * WorldMapGlobe.MAP_PRECISION));
                sb.Append("$");
                sb.Append((int)(city.localPosition.y * WorldMapGlobe.MAP_PRECISION));
                sb.Append("$");
                sb.Append((int)(city.localPosition.z * WorldMapGlobe.MAP_PRECISION));
                sb.Append("$");
                sb.Append((int)city.cityClass);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Exports the geographic data in binary format.
        /// </summary>
        public byte[] GetCitiesGeoDataBinary () {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8);
            int citiesCount = cities.Count;
            bw.Write((uint)citiesCount);
            for (int k = 0; k < citiesCount; k++) {
                City city = cities[k];
                bw.Write(city.name);
                bw.Write(city.province);
                bw.Write(countries[city.countryIndex].name);
                bw.Write(city.population);
                bw.Write(city.localPosition.x);
                bw.Write(city.localPosition.y);
                bw.Write(city.localPosition.z);
                bw.Write((byte)city.cityClass);
            }
            bw.Flush();
            return ms.ToArray();
        }


        /// <summary>
        /// Reads the cities data from a packed string. Use GetCityGeoData method to get the current city geodata information.
        /// </summary>
        public void SetCitiesGeoData (string s) {

            if (_countries == null) {
                Init();
                if (_cities != null)
                    return;
            }
            lastCityLookupCount = -1;

            int cityCount = s.Count('|', 0, s.Length);
            cities = new List<City>(cityCount);
            int citiesCount = 0;
            foreach (StringSpan citySpan in s.Split('|', 0, s.Length)) {
                IEnumerator<StringSpan> cityReader = s.Split('$', citySpan.start, citySpan.length).GetEnumerator();
                string name = s.ReadString(cityReader);
                string province = s.ReadString(cityReader);
                string country = s.ReadString(cityReader);
                int countryIndex = GetCountryIndex(country);
                if (countryIndex >= 0) {
                    int population = s.ReadInt(cityReader); // int.Parse(cityInfo[3]);
                    float x = s.ReadFloat(cityReader) / MAP_PRECISION;
                    float y = s.ReadFloat(cityReader) / MAP_PRECISION;
                    float z = s.ReadFloat(cityReader) / MAP_PRECISION;

                    CITY_CLASS cityClass = (CITY_CLASS)s.ReadInt(cityReader);

                    City city = new City(name, province, countryIndex, population, new Vector3(x, y, z), cityClass);
                    _cities.Add(city);
                    if (cityClass == CITY_CLASS.COUNTRY_CAPITAL) {
                        _countries[countryIndex].cityCapitalIndex = citiesCount;
                    }
                    citiesCount++;
                }
            }
        }

        /// <summary>
        /// Loads cities information from a binary array
        /// </summary>
        public void SetCitiesGeoDataBinary (byte[] data) {
            if (data == null || data.Length < 4) return;

            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8);

            int cityCount = (int)br.ReadUInt32();

            lastCityLookupCount = -1;

            cities = new List<City>(cityCount);
            int cityIndex = 0;
            int unknownCountryIndex = -1;
            for (int k = 0; k < cityCount; k++) {
                string name = br.ReadString();
                string province = br.ReadString();
                string country = br.ReadString();
                int countryIndex = GetCountryIndex(country);
                if (countryIndex < 0) {
                    if (unknownCountryIndex < 0) {
                        Country uc = new Country("Unknown", "None");
                        unknownCountryIndex = CountryAdd(uc);
                    }
                    countryIndex = unknownCountryIndex;
                }
                if (countryIndex >= 0) {
                    int population = br.ReadInt32();
                    float x = br.ReadSingle();
                    float y = br.ReadSingle();
                    float z = br.ReadSingle();
                    CITY_CLASS cityClass = (CITY_CLASS)br.ReadByte();
                    if (cityClass == CITY_CLASS.COUNTRY_CAPITAL) {
                        _countries[countryIndex].cityCapitalIndex = cityIndex;
                    }
                    City city = new City(name, province, countryIndex, population, new Vector3(x, y, z), cityClass);
                    cities.Add(city);
                    cityIndex++;
                }
            }
        }

        #endregion

        #region Drawing stuff

        /// <summary>
        /// Redraws the cities. This is automatically called by Redraw(). Used internally by the Map Editor. You should not need to call this method directly.
        /// </summary>
        public virtual void DrawCities () {

            if (!_showCities || !gameObject.activeInHierarchy || cities == null)
                return;

            if (cityProps == null) cityProps = new MaterialPropertyBlock();

            // Create cities layer
            Transform t = transform.Find("Cities");
            if (t != null)
                DestroyImmediate(t.gameObject);
            citiesLayer = new GameObject("Cities");
            citiesLayer.transform.SetParent(transform, false);
            citiesLayer.layer = gameObject.layer;
            if (_earthInvertedMode) {
                citiesLayer.transform.localScale *= 0.99f;
            }

            // Create cityclass parents
            GameObject countryCapitals = new GameObject("Country Capitals");
            countryCapitals.hideFlags = HideFlags.DontSave;
            countryCapitals.transform.SetParent(citiesLayer.transform, false);
            countryCapitals.layer = gameObject.layer;
            GameObject regionCapitals = new GameObject("Region Capitals");
            regionCapitals.hideFlags = HideFlags.DontSave;
            regionCapitals.transform.SetParent(citiesLayer.transform, false);
            regionCapitals.layer = gameObject.layer;
            GameObject normalCities = new GameObject("Normal Cities");
            normalCities.hideFlags = HideFlags.DontSave;
            normalCities.transform.SetParent(citiesLayer.transform, false);
            normalCities.layer = gameObject.layer;
            float scale = CityScaler.GetScale(this);
            Vector3 cityScale = new Vector3(scale, scale, 1f);

            // Draw city marks
            _numCitiesDrawn = 0;
            int minPopulation = _minPopulation * 1000;
            int visibleCount = 0;

            // flip localscale.x to prevent transform issues
            if (_earthInvertedMode) {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }

            int cityCount = cities.Count;
            int layer = gameObject.layer;

            if (citySpot.GetComponentInChildren<Renderer>() == null) {
                Debug.LogWarning("City Spot prefab has no renderer attached.");
                cityCount = 0;
            }
            if (citySpotCapitalRegion.GetComponentInChildren<Renderer>() == null) {
                Debug.LogWarning("City Region Capital prefab has no renderer attached.");
                cityCount = 0;
            }
            if (citySpotCapitalCountry.GetComponentInChildren<Renderer>() == null) {
                Debug.LogWarning("City Capital has no renderer attached.");
                cityCount = 0;
            }

            for (int k = 0; k < cityCount; k++) {
                City city = cities[k];
                Country country = countries[city.countryIndex];
                city.isShown = !country.hidden && ((((int)city.cityClass & _cityClassAlwaysShow) != 0) || (minPopulation == 0 || city.population >= minPopulation));
                if (city.isShown) {
                    GameObject cityObj, cityParent;
                    Material cityMaterial;
                    switch (city.cityClass) {
                        case CITY_CLASS.COUNTRY_CAPITAL:
                            cityObj = Instantiate(citySpotCapitalCountry);
                            cityMaterial = citiesCountryCapitalMat;
                            cityParent = countryCapitals;
                            break;
                        case CITY_CLASS.REGION_CAPITAL:
                            cityObj = Instantiate(citySpotCapitalRegion);
                            cityMaterial = citiesRegionCapitalMat;
                            cityParent = regionCapitals;
                            break;
                        default:
                            cityObj = Instantiate(citySpot);
                            cityMaterial = citiesNormalMat;
                            cityParent = normalCities;
                            break;
                    }
                    city.renderer = cityObj.GetComponentInChildren<Renderer>();
                    city.renderer.sharedMaterial = cityMaterial;
                    cityObj.layer = layer;
                    cityObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                    cityObj.transform.SetParent(cityParent.transform, false);
                    cityObj.transform.localPosition = city.localPosition;
                    cityObj.transform.localScale = cityScale;
                    if (_earthInvertedMode) {
                        cityObj.transform.LookAt(transform.TransformPoint(city.localPosition * 2f));
                    } else {
                        cityObj.transform.LookAt(transform.position);
                    }
                    _numCitiesDrawn++;
                    visibleCount++;
                } else {
                    city.renderer = null;
                }
            }

            if (_earthInvertedMode) {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }

            // Cache visible cities (this is faster than iterate through the entire collection)
            if (visibleCities == null || visibleCities.Length != visibleCount)
                visibleCities = new City[visibleCount];

            for (int k = 0; k < cityCount; k++) {
                City city = cities[k];
                if (city.isShown)
                    visibleCities[--visibleCount] = city;
            }

            // Toggle cities layer visibility according to settings
            citiesLayer.SetActive(_showCities);

            CityScaler cityScaler = citiesLayer.GetComponent<CityScaler>();
            if (cityScaler == null) {
                cityScaler = citiesLayer.AddComponent<CityScaler>();
            }
            cityScaler.Init(this);
            cityScaler.ScaleCities();
        }

        void HighlightCity (int cityIndex) {
            if (cityIndex == _cityHighlightedIndex)
                return;
            if (!ValidCityIndex(cityIndex)) return;

            if (_enableCityHighlight && _cityHighlightedIndex >= 0) {
                SetCityColor(cityIndex, GetCityColorByClass(_cityHighlightedIndex));
            }
            _cityHighlightedIndex = cityIndex;
            City city = cities[cityIndex];
            _cityHighlighted = city;

            if (_enableCityHighlight) {
                SetCityColor(_cityHighlightedIndex, _citiesHighlightColor);
            }
        }

        void HideCityHighlight () {
            if (_cityHighlightedIndex < 0)
                return;

            cityProps.SetColor(ShaderParams.Color, GetCityColorByClass(_cityHighlightedIndex));
            cities[_cityHighlightedIndex].renderer.SetPropertyBlock(cityProps);

            // Raise event
            if (OnCityExit != null) {
                OnCityExit(_cityHighlightedIndex);
            }
            _cityHighlighted = null;
            _cityHighlightedIndex = -1;
        }

        #endregion

        #region Internal Cities API

        /// <summary>
        /// Returns the color of a city by its class
        Color GetCityColorByClass (int cityIndex) {
            switch (cities[cityIndex].cityClass) {
                case CITY_CLASS.COUNTRY_CAPITAL: return citiesCountryCapitalMat.color;
                case CITY_CLASS.REGION_CAPITAL: return citiesRegionCapitalMat.color;
                default: return citiesNormalMat.color;
            }
        }

        void SetCityColor (int cityIndex, Color color) {
            if (cityProps == null) cityProps = new MaterialPropertyBlock();
            cityProps.SetColor(ShaderParams.Color, color);
            City city = cities[cityIndex];
            if (city.renderer != null) {
                city.renderer.SetPropertyBlock(cityProps);
            }
        }

        /// <summary>
        /// Returns any city near the point specified in sphere coordinates. If accurate is false, the city must be closer than CITY_HIT_PRECISION constant, usually when pointer clicks over a city icon.
        /// Optimized method for mouse hitting.
        /// </summary>
        /// <returns>The city near point.</returns>
        /// <param name="localPoint">Local point in sphere coordinates.</param>
        public int GetCityNearPointFast (Vector3 localPoint) {
            if (visibleCities == null)
                return -1;
            float hitPrecision = CITY_HIT_PRECISION * _cityIconSize * 5.0f;
            float hitPrecisionSqr = hitPrecision * hitPrecision;
            Vector2 latlon = Conversion.GetLatLonFromUnitSpherePoint(localPoint);
            int cityCount = visibleCities.Length;
            for (int c = 0; c < countries.Length; c++) {
                Country country = countries[c];
                if (country.regionsRect2D.Contains(latlon)) {
                    for (int t = 0; t < cityCount; t++) {
                        City city = visibleCities[t];
                        if (city.countryIndex != c)
                            continue;
                        float dist = FastVector.SqrDistance(ref city.localPosition, ref localPoint);
                        if (dist < hitPrecisionSqr) {
                            return GetCityIndex(city, false);
                        }
                    }
                }
            }
            return -1;
        }

        bool GetCityUnderMouse (int countryIndex, Vector3 localPoint, out int cityIndex) {
            cityIndex = -1;
            if (visibleCities == null)
                return false;
            float hitPrecision = CITY_HIT_PRECISION * _cityIconSize * 5.0f;
            float hitPrecisionSqr = hitPrecision * hitPrecision;
            int cityCount = visibleCities.Length;
            for (int c = 0; c < cityCount; c++) {
                City city = visibleCities[c];
                if (city.countryIndex == countryIndex && city.isShown) {
                    if (FastVector.SqrDistance(ref city.localPosition, ref localPoint) < hitPrecisionSqr) {
                        cityIndex = GetCityIndex(city, false);
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Returns the nearest city to the point specified in sphere coordinates.
        /// </summary>
        /// <param name="localPoint">Local point in sphere coordinates.</param>
        public int GetCityNearPoint (Vector3 localPoint, int countryIndex = -1) {
            if (visibleCities == null)
                return -1;
            float minDist = float.MaxValue;
            int cityCount = visibleCities.Length;
            int candidate = -1;
            for (int t = 0; t < cityCount; t++) {
                City city = visibleCities[t];
                if (countryIndex >= 0 && city.countryIndex != countryIndex) continue;
                float dist = FastVector.SqrDistance(ref city.localPosition, ref localPoint);
                if (dist < minDist) {
                    minDist = dist;
                    candidate = GetCityIndex(city, false);
                }
            }
            return candidate;
        }


        /// <summary>
        /// Returns cities belonging to a provided country.
        /// </summary>
        public List<City> GetCities (int countryIndex) {
            List<City> results = new List<City>(20);
            int cityCount = cities.Count;
            for (int c = 0; c < cityCount; c++) {
                if (cities[c].countryIndex == countryIndex)
                    results.Add(cities[c]);
            }
            return results;
        }

        /// <summary>
        /// Returns cities enclosed by a region.
        /// </summary>
        public List<City> GetCities (Region region) {
            List<City> results = new List<City>(20);
            int cityCount = cities.Count;
            for (int c = 0; c < cityCount; c++) {
                if (region.Contains(_cities[c].latlon)) {
                    results.Add(cities[c]);
                }
            }
            return results;
        }


        /// <summary>
        /// Updates the city scale.
        /// </summary>
        public void ScaleCities () {
            if (citiesLayer != null) {
                CityScaler scaler = citiesLayer.GetComponent<CityScaler>();
                if (scaler != null) {
                    scaler.ScaleCities();
                } else {
                    DrawCities();
                }
            }
        }


        int GetCityCountryRegionIndex (City city) {
            if (city.regionIndex < 0) {
                int countryIndex = city.countryIndex;
                if (countryIndex < 0 || countryIndex > countries.Length)
                    return -1;

                Country country = countries[countryIndex];
                if (country.regions == null)
                    return -1;
                int regionCount = country.regions.Count;

                float minDist = float.MaxValue;
                for (int cr = 0; cr < regionCount; cr++) {
                    Region region = country.regions[cr];
                    if (region == null || region.spherePoints == null)
                        continue;
                    for (int p = 0; p < region.spherePoints.Length; p++) {
                        float dist = (region.spherePoints[p] - city.localPosition).sqrMagnitude;
                        if (dist < minDist) {
                            minDist = dist;
                            city.regionIndex = cr;
                        }
                    }
                }
            }
            return city.regionIndex;
        }



        #endregion
    }

}