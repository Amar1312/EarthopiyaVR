#define LIGHTSPEED

using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WPM
{
    public class Demo_UI : MonoBehaviour
    {

        //For Only First Camera Effect only
        public Camera _camera;
        public AnimalSafariPanel _animal;
        public MainMarkerScript _marker;

        // UI references
        public Slider zoomSlider;

        // Rocket prefab
        public GameObject rocketPrefab;

        // Information labels and update rate 
        public Text langLongText;
        public Text informationText;
        public float langLongUpdateRate = 0.1f;
        float nextLangLongUpdate = 0;

        // Cached for our use
        WorldMapGlobe map; // the globe
        ColorPicker colorPicker; // color picker for borders and country selection highlight

        // State bools
        bool changingFrontiersColor = false;
        bool minimizeState = false;
        bool animatingField;

        private void OnEnable()
        {
#if LIGHTSPEED
            if (_camera != null)
                _camera.fieldOfView = 180;
            else
                Camera.main.fieldOfView = 180;
            animatingField = true;
#endif
        }


        private void OnDisable()
        {
            map.ResetCamera(2f);
        }


        // Start is called before the first frame update
        void Start()
        {
            colorPicker = GetComponent<ColorPicker>();
            GUIResizer.Init(Screen.width, Screen.height);

            // Get a reference to the World Map API:
            map = WorldMapGlobe.instance;
            //#if LIGHTSPEED
            //            if (_camera != null)
            //                _camera.fieldOfView = 180;
            //            else
            //                Camera.main.fieldOfView = 180;
            //            animatingField = true;
            //#endif
            map.earthInvertedMode = false;

            /* Register events: this is optionally but allows your scripts to be informed instantly as the mouse enters or exits a country, province or city */
            map.OnCityEnter += (int cityIndex) => Debug.Log("Entered city " + map.cities[cityIndex].name);
            map.OnCityExit += (int cityIndex) => Debug.Log("Exited city " + map.cities[cityIndex].name);
            map.OnCityPointerDown += (int cityIndex, int buttonIndex) => Debug.Log("Pointer down on city " + map.cities[cityIndex].name);
            map.OnCityClick += (int cityIndex, int buttonIndex) => Debug.Log("Clicked city " + map.cities[cityIndex].name);
            map.OnCityPointerUp += (int cityIndex, int buttonIndex) => Debug.Log("Pointer up on city " + map.cities[cityIndex].name);

            map.OnCountryEnter += (int countryIndex, int regionIndex) => Debug.Log("Entered country (" + countryIndex + ") " + map.countries[countryIndex].name);
            map.OnCountryExit += (int countryIndex, int regionIndex) => Debug.Log("Exited country " + map.countries[countryIndex].name);
            map.OnCountryPointerDown += (int countryIndex, int regionIndex, int buttonIndex) => Debug.Log("Pointer down on country " + map.countries[countryIndex].name);
            map.OnCountryClick += (int countryIndex, int regionIndex, int buttonIndex) => Debug.Log("Clicked country " + map.countries[countryIndex].name);
            map.OnCountryPointerUp += (int countryIndex, int regionIndex, int buttonIndex) => Debug.Log("Pointer up on country " + map.countries[countryIndex].name);

            map.OnProvinceEnter += (int provinceIndex, int regionIndex) => Debug.Log("Entered province (" + provinceIndex + ") " + map.provinces[provinceIndex].name);
            map.OnProvinceExit += (int provinceIndex, int regionIndex) => Debug.Log("Exited province " + map.provinces[provinceIndex].name);
            map.OnProvincePointerDown += (int provinceIndex, int regionIndex, int buttonIndex) => Debug.Log("Pointer down on province " + map.provinces[provinceIndex].name);
            map.OnProvinceClick += (int provinceIndex, int regionIndex, int buttonIndex) => Debug.Log("Clicked province " + map.provinces[provinceIndex].name);
            map.OnProvincePointerUp += (int provinceIndex, int regionIndex, int buttonIndex) => Debug.Log("Pointer up on province " + map.provinces[provinceIndex].name);

            map.OnContinentEnter += (string continent) => Debug.Log("Entered continent " + continent);
            map.OnContinentExit += (string continent) => Debug.Log("Exited continent " + continent);
            map.OnContinentPointerDown += (string continent, int buttonIndex) => Debug.Log("Pointer down on continent " + continent);
            map.OnContinentClick += (string continent, int buttonIndex) => Debug.Log("Clicked continent " + continent);
            map.OnContinentPointerUp += (string continent, int buttonIndex) => Debug.Log("Pointer up on continent " + continent);

            map.OnClick += (sphereLocation, mouseButtonIndex) =>
            {
                Vector2 latLon = Conversion.GetLatLonFromSpherePoint(sphereLocation);
                Debug.Log("Clicked on Latitude: " + latLon.x + ", Longitude: " + latLon.y);
            };
        }


        void Update()
        {

            // Check whether a country or city is selected, then show a label
            if (map.mouseIsOver)
            {
                string text;

                if (map.countryHighlighted != null || map.cityHighlighted != null || map.provinceHighlighted != null)
                {
                    City city = map.cityHighlighted;
                    if (city != null)
                    {
                        if (city.province != null && city.province.Length > 0)
                        {
                            text = "City: " + map.cityHighlighted.name + " (" + city.province + ", " + map.countries[map.cityHighlighted.countryIndex].name + ")";
                        }
                        else
                        {
                            text = "City: " + map.cityHighlighted.name + " (" + map.countries[map.cityHighlighted.countryIndex].name + ")";
                        }
                    }
                    else if (map.provinceHighlighted != null)
                    {
                        text = map.provinceHighlighted.name + ", " + map.countryHighlighted.name;
                        List<Province> neighbours = map.ProvinceNeighboursOfCurrentRegion();
                        if (neighbours.Count > 0)
                            text += "\n" + EntityListToString(neighbours);
                    }
                    else if (map.countryHighlighted != null)
                    {
                        text = map.countryHighlighted.name + " (" + map.countryHighlighted.continent + ")";
                        List<Country> neighbours = map.CountryNeighboursOfCurrentRegion();
                        if (neighbours.Count > 0)
                            text += "\n" + EntityListToString(neighbours);
                    }
                    else
                    {
                        text = "";
                    }

                    informationText.text = text;
                }

                if (Time.time > nextLangLongUpdate)
                {
                    langLongText.text = map.calc.prettyCurrentLatLon;
                    nextLangLongUpdate = Time.time + langLongUpdateRate;
                }

            }

            if (colorPicker.showPicker)
            {
                if (changingFrontiersColor)
                {
                    map.frontiersColor = colorPicker.setColor;
                }
                else
                {
                    map.fillColor = colorPicker.setColor;
                }
            }


            if (!animatingField)
            {
                zoomSlider.value = map.GetZoomLevel() * 100f;
                if (_animal != null)
                {
                    _animal._marker[0].GetComponent<MainMarkerScript>().SetSprite(map.GetZoomLevel());
                }

                if (_marker != null)
                {
                    _marker.SetSprite(map.GetZoomLevel());
                }
            }

            if (_camera != null)
            {

                if (animatingField)
                {
                    if (_camera.fieldOfView > 60)
                    {
                        _camera.fieldOfView -= (181.0f - _camera.fieldOfView) / (220.0f - _camera.fieldOfView);
                    }
                    else
                    {
                        _camera.fieldOfView = 60;
                        animatingField = false;
                        map.zoomMaxDistance = map.GetZoomLevelDistance(1);
                    }
                }
            }
            else
            {
                // Animates the camera field of view (just a cool effect at the begining)
                if (animatingField)
                {
                    if (Camera.main.fieldOfView > 60)
                    {
                        Camera.main.fieldOfView -= (181.0f - Camera.main.fieldOfView) / (220.0f - Camera.main.fieldOfView);
                    }
                    else
                    {
                        Camera.main.fieldOfView = 60;
                        animatingField = false;
                        map.zoomMaxDistance = map.GetZoomLevelDistance(1);
                        map.SetZoomLevel(2f);
                    }
                }

            }

        }

        // Public methods for Buttons

        public void ChangeFrontiersColor()
        {
            colorPicker.showPicker = true;
            changingFrontiersColor = true;
        }

        public void ChangeFillColor()
        {
            colorPicker.showPicker = true;
            changingFrontiersColor = false;
        }

        public void ShowFrontiers(bool state)
        {
            map.showFrontiers = state;
        }

        public void ShowEarth(bool state)
        {
            map.showEarth = state;
        }
        public void ShowCities(bool state)
        {
            map.showCities = state;
        }

        public void ShowCountryNames(bool state)
        {
            map.showCountryNames = state;
        }

        public void ShowProvinces(bool state)
        {
            map.showProvinces = state;
        }

        public void ChangeEarthStyle()
        {
            int currentEarthStyle = (int)map.earthStyle;
            currentEarthStyle = (currentEarthStyle + 1) % 20;
            map.earthStyle = (EARTH_STYLE)currentEarthStyle;
        }

        public void ChangeZoomLevel(float zoomLevel)
        {
            map.SetZoomLevel(zoomLevel / 100);
            if(zoomLevel != 200)
            {
                if (map.autoRotationSpeed != 0f)
                {
                    map.autoRotationSpeed = 0f;
                }
            }
        }

        public void ColorizeEurope()
        {
            map.FlyToCity("Brussels");

            for (int colorizeIndex = 0; colorizeIndex < map.countries.Length; colorizeIndex++)
            {
                if (map.countries[colorizeIndex].continent.Equals("Europe"))
                {
                    Color color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                    map.ToggleCountrySurface(map.countries[colorizeIndex].name, true, color, true);
                }
            }
        }

        public void ColorizeRandomCountry()
        {
            map.FlyToCity("Brussels");
            int countryIndex = UnityEngine.Random.Range(0, map.countries.Length);
            Color color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
            map.ToggleCountrySurface(countryIndex, true, color);
            map.FlyToCountry(countryIndex);
        }

        public void ResetCountries()
        {
            map.HideCountrySurfaces();
        }

        string EntityListToString<T>(List<T> entities)
        {
            StringBuilder sb = new StringBuilder("Neighbours: ");
            for (int k = 0; k < entities.Count; k++)
            {
                if (k > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(((IAdminEntity)entities[k]).name);
            }
            return sb.ToString();
        }

        // Sample code to show how to:
        // 1.- Navigate and center a country in the map
        // 2.- Add a blink effect to one country (can be used on any number of countries)
        public void FlyToCountry(string countryName)
        {
            int countryIndex = map.GetCountryIndex(countryName);
            float zoomLevel = map.GetCountryMainRegionZoomExtents(countryIndex);
            map.FlyToCountry(countryIndex, 2f, zoomLevel, 0.5f);
            map.BlinkCountry(countryIndex, Color.black, Color.green, 4, 2.5f, true);
        }

        // Sample code to show how to navigate to a city:
        public void FlyToCity(string cityName)
        {
            int cityIndex = map.GetCityIndex(cityName);
            map.FlyToCity(cityIndex, 2f, 0.2f, 0.5f);
        }

        // Sample code to show how tickers work
        public void TickerSample()
        {
            map.ticker.ResetTickerBands();

            // Configure 1st ticker band: a red band in the northern hemisphere
            TickerBand tickerBand = map.ticker.tickerBands[0];
            tickerBand.verticalOffset = 0.2f;
            tickerBand.backgroundColor = new Color(1, 0, 0, 0.9f);
            tickerBand.scrollSpeed = 0; // static band
            tickerBand.visible = true;
            tickerBand.autoHide = true;

            // Prepare a static, blinking, text for the red band
            TickerText tickerText = new TickerText(0, "WARNING!!");
            tickerText.textColor = Color.yellow;
            tickerText.blinkInterval = 0.2f;
            tickerText.horizontalOffset = 0.1f;
            tickerText.duration = 10.0f;

            // Draw it!
            map.ticker.AddTickerText(tickerText);

            // Configure second ticker band (below the red band)
            tickerBand = map.ticker.tickerBands[1];
            tickerBand.verticalOffset = 0.1f;
            tickerBand.verticalSize = 0.05f;
            tickerBand.backgroundColor = new Color(0, 0, 1, 0.9f);
            tickerBand.visible = true;
            tickerBand.autoHide = true;

            // Prepare a ticker text
            tickerText = new TickerText(1, "INCOMING MISSILE!!");
            tickerText.textColor = Color.white;

            // Draw it!
            map.ticker.AddTickerText(tickerText);
        }

        // Sample code to show how to use decorators to assign a texsture
        public void TextureSample()
        {
            // 1st way (best): assign a flag texture to USA using direct API - this texture will get cleared when you call HideCountrySurfaces()
            Texture2D texture = Resources.Load<Texture2D>("flagUSA");
            int countryIndex = map.GetCountryIndex("United States of America");
            map.ToggleCountrySurface(countryIndex, true, Color.white, texture);

            // 2nd way: assign a flag texture to Brazil using decorator - the texture will stay when you call HideCountrySurfaces()
            string countryName = "Brazil";
            CountryDecorator decorator = new CountryDecorator();
            decorator.isColorized = true;
            decorator.texture = Resources.Load<Texture2D>("flagBrazil");
            decorator.textureOffset = Misc.Vector2down * 2.4f;
            map.decorator.SetCountryDecorator(0, countryName, decorator);

            Debug.Log("USA flag added with direct API.");
            Debug.Log("Brazil flag added with decorator (persistent texture).");

            map.FlyToCountry("Panama", 2f);
        }

        // The globe can be moved and scaled at wish
        public void ToggleMinimize()
        {
            minimizeState = !minimizeState;

            Camera.main.transform.position = Vector3.back * 14.95f;
            Camera.main.transform.rotation = Misc.QuaternionZero; // Quaternion.Euler (Misc.Vector3zero);
            if (minimizeState)
            {
                map.transform.localScale = Vector3.one;
                map.transform.localPosition = new Vector3(0.0f, -7f, 0);
                map.allowUserZoom = false;
                map.earthStyle = EARTH_STYLE.Alternate2;
                map.earthColor = Color.black;
                map.longitudeStepping = 4;
                map.latitudeStepping = 40;
                map.showFrontiers = false;
                map.showCities = false;
                map.showCountryNames = false;
                map.gridLinesColor = new Color(0.06f, 0.23f, 0.398f);
            }
            else
            {
                map.transform.localScale = new Vector3(10f, 10f, 10f);
                map.transform.localPosition = Vector3.zero;
                map.allowUserZoom = true;
                map.earthStyle = EARTH_STYLE.NaturalHighResScenicScatter;
                map.longitudeStepping = 15;
                map.latitudeStepping = 15;
                map.showFrontiers = true;
                map.showCities = true;
                map.showCountryNames = true;
                map.gridLinesColor = new Color(0.16f, 0.33f, 0.498f);
            }
        }


        /// <summary>
        /// Illustrates how to add custom markers over the globe using the AddMarker API.
        /// In this example a building prefab is added to a random city (see comments for other options).
        /// </summary>
        public void AddMarkerGameObjectOnRandomCity()
        {
            // Every marker is put on a spherical-coordinate (assuming a radius = 0.5 and relative center at zero position)
            Vector3 sphereLocation;

            // Add a marker on a random city
            City city = map.cities[Random.Range(0, map.cities.Count)];
            sphereLocation = city.localPosition;

            // or... choose a city by its name:
            //		int cityIndex = map.GetCityIndex("Moscow");
            //		sphereLocation = map.cities[cityIndex].unitySphereLocation;

            // or... use the centroid of a country
            //		int countryIndex = map.GetCountryIndex("Greece");
            //		sphereLocation = map.countries[countryIndex].center;

            // or... use a custom location lat/lon. Example put the building over New York:
            //      sphereLocation = Conversion.GetSpherePointFromLatLon(40.71f, -74.00f);

            // or... use the calc converter component
            //		map.calc.fromLatDec = 40.71f;	// 40.71 decimal degrees north
            //		map.calc.fromLonDec = -74.00f;	// 74.00 decimal degrees to the west
            //		map.calc.fromUnit = UNIT_TYPE.DecimalDegrees;
            //		map.calc.Convert();
            //		sphereLocation = map.calc.toSphereLocation;

            // Send the prefab to the AddMarker API setting a scale of 0.02f (this depends on your marker scales)
            GameObject building = Instantiate(Resources.Load<GameObject>("Building/Building"));

            map.AddMarker(building, sphereLocation, 0.02f);

            // Fly to the destination and see the building created
            map.FlyToLocation(sphereLocation);

            // Optionally add a blinking effect to the marker
            MarkerBlinker.AddTo(building, 4, 0.2f);
        }

        public void AddMarkerCircleOnRandomPosition()
        {
            // Draw a beveled circle
            //Vector3 sphereLocation = Random.onUnitSphere * 0.5f;
            Vector3 sphereLocation = new Vector3(-0.873705f, 0.3793454f, 0.304527f) * 0.5f;
            float km = Random.value * 500 + 500; // Circle with a radius of (500...1000) km

            //			sphereLocation = map.cities[map.GetCityIndex("Paris")].unitySphereLocation;
            //			km = 1053;
            //			sphereLocation = map.cities[map.GetCityIndex("New York")].unitySphereLocation;
            //			km = 500;
            //map.AddMarker(MARKER_TYPE.CIRCLE_PROJECTED, sphereLocation, km, 0.975f, 1.0f, new Color(0.85f, 0.45f, 0.85f, 0.9f));
            //map.AddMarker(MARKER_TYPE.CIRCLE_PROJECTED, sphereLocation, km, 0, 0.975f, new Color(0.5f, 0, 0.5f, 0.9f));
            map.AddMarker(MARKER_TYPE.CIRCLE_PROJECTED, sphereLocation, km, 0, 0.975f, new Color(255f, 255f, 255f, 0.9f));
            map.FlyToLocation(sphereLocation);
        }


        /// <summary>
        /// Example of how to add custom lines to the map
        /// Similar to the AddMarker functionality, you need two spherical coordinates and then call AddLine
        /// </summary>
        public void AddTrajectories(int numberOfLines)
        {
            // In this example we will add random lines from a group of cities to another cities (see AddMaker example above for other options to get locations)
            for (int line = 0; line < numberOfLines; line++)
            {
                // Get two random cities
                int city1 = Random.Range(0, map.cities.Count);
                int city2 = Random.Range(0, map.cities.Count);

                // Get their sphere-coordinates
                Vector3 start = map.cities[city1].localPosition;
                Vector3 end = map.cities[city2].localPosition;

                // Add lines with random color, speeds and elevation
                Color color = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1));
                float elevation = Random.Range(0, 0.5f);    // elevation is % relative to the Earth radius
                float drawingDuration = 4.0f;
                float lineWidth = 0.005f;
                float fadeAfter = 2.0f; // line stays for 2 seconds, then fades out - set this to zero to avoid line removal
                LineMarkerAnimator lma = map.AddLine(start, end, color, elevation, drawingDuration, lineWidth, fadeAfter);
                lma.useTube = true; // use a 3D tube instead of flat line
            }
        }

        /// <summary>
        /// Mount points are special locations on the map defined by user in the Map Editor.
        /// </summary>
        public void LocateMountPoint()
        {
            int mountPointsCount = map.mountPoints.Count;
            Debug.Log("There're " + map.mountPoints.Count + " mount point(s). You can define more mount points using the Map Editor. Mount points are stored in mountPoints.txt file inside Resources/Geodata folder.");
            if (mountPointsCount > 0)
            {
                Debug.Log("Locating random mount point...");
                int mp = UnityEngine.Random.Range(0, mountPointsCount - 1);
                Vector3 location = map.mountPoints[mp].localPosition;
                map.FlyToLocation(location);
            }
        }

        public void ShowStatesNames()
        {
            // First we ensure only states for USA are shown
            int countryUSAIndex = map.GetCountryIndex("United States of America");
            for (int k = 0; k < map.countries.Length; k++)
            {
                if (k != countryUSAIndex)
                {
                    map.countries[k].allowShowProvinces = false;
                }
            }
            map.showProvinces = true;
            map.drawAllProvinces = true;

            // Now, hide all country names and show states for USA
            map.showCountryNames = false;
            Country usaCountry = map.countries[countryUSAIndex];
            for (int p = 0; p < usaCountry.provinces.Length; p++)
            {
                Province state = usaCountry.provinces[p];
                Color color = new Color(Random.value, Random.value, Random.value);
                map.AddText(state.name, state.localPosition, color);  // Uses legacy Text Mesh
                //map.AddTextPro(state.name, state.localPosition, color); // Uses Text Mesh Pro
            }

            map.FlyToCountry(usaCountry);
        }

        #region Bullet shooting!

        /// <summary>
        /// Creates a rocket on current map position and launch it over a random position on the globe following an arc
        /// </summary>
        public void FireRocket()
        {
            GameObject rocket = Instantiate(rocketPrefab);
            rocket.GetComponentInChildren<Renderer>().material.color = Color.yellow;

            // Choose starting pos
            Vector3 startPos = map.GetCurrentMapLocation();

            // Get a random target city
            int randomCity = Random.Range(0, map.cities.Count);
            Vector3 endPos = map.cities[randomCity].localPosition;

            // Fire the bullet!
            StartCoroutine(AnimateMissile(rocket, 0.005f, startPos, endPos));
        }

        IEnumerator AnimateMissile(GameObject missile, float scale, Vector3 startPos, Vector3 endPos, float duration = 3f, float arc = 0.25f)
        {
            // Optional: Draw the trajectory
            map.AddLine(startPos, endPos, Color.white, arc, duration, lineWidth: 0.002f, fadeOutAfter: 0.1f);

            // Optional: Follow the bullet
            map.FlyToLocation(endPos, duration);

            // Animate loop for moving bullet over time
            float bulletFireTime = Time.time;
            float elapsed = Time.time - bulletFireTime;
            Vector3 prevPos = map.transform.position;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 pos = Vector3.Lerp(startPos, endPos, t).normalized * 0.5f;
                float altitude = Mathf.Sin(t * Mathf.PI) * arc / scale;
                map.AddMarker(missile, pos, scale, true, altitude);

                Vector3 wpos = map.transform.TransformPoint(pos);
                if (elapsed > 0)
                {
                    Vector3 axis = (wpos - prevPos).normalized;
                    missile.transform.forward = axis;
                }
                prevPos = wpos;

                yield return new WaitForFixedUpdate();
                elapsed = Time.time - bulletFireTime;
            }

            Destroy(missile);
        }

        #endregion
    }
}

