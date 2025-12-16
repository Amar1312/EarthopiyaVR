// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using WPM.ClipperLib;

namespace WPM {

    public enum FRONTIERS_DETAIL {
        Low = 0,
        High = 1
    }

    public enum LABELS_QUALITY {
        Low = 0,
        Medium = 1,
        High = 2,
        NotUsed = 3
    }

    public enum LABELS_ANTIALIAS {
        None = 1,
        x2 = 2,
        x4 = 4,
        x8 = 8
    }


    public enum TEXT_ENGINE {
        TextMeshStandard = 0,
        TextMeshPro = 1
    }

    public enum LABELS_RENDER_METHOD {
        Blended = 0,
        WorldSpace = 1
    }

    public enum FRONTIERS_THICKNESS {
        Thin = 0,
        Custom = 1
    }

    public enum COUNTRY_LABELS_ORIENTATION {
        Automatic = 0,
        Horizontal = 1
    }

    /* Event definitions */
    public delegate void CountryBeforeEnterEvent (int countryIndex, int regionIndex, ref bool ignoreCountry);
    public delegate void CountryEvent (int countryIndex, int regionIndex);
    public delegate void CountryClickEvent (int countryIndex, int regionIndex, int buttonIndex);

    /* Public WPM Class */
    public partial class WorldMapGlobe : MonoBehaviour {

        public event CountryBeforeEnterEvent OnCountryBeforeEnter;
        public event CountryEvent OnCountryEnter;
        public event CountryEvent OnCountryExit;
        public event CountryClickEvent OnCountryPointerDown;
        public event CountryClickEvent OnCountryPointerUp;
        public event CountryClickEvent OnCountryClick;


        /// <summary>
        /// Complete list of countries and the continent name they belong to.
        /// </summary>
        public Country[] countries {
            get { return _countries; }
            set {
                _countries = value;
                lastCountryLookupCount = -1;
            }
        }

        Country _countryHighlighted;

        /// <summary>
        /// Returns Country under mouse position or null if none.
        /// </summary>
        public Country countryHighlighted { get { return _countryHighlighted; } }

        int _countryHighlightedIndex = -1;

        /// <summary>
        /// Returns currently highlighted country index in the countries list.
        /// </summary>
        public int countryHighlightedIndex { get { return _countryHighlightedIndex; } }

        Region _countryRegionHighlighted;

        /// <summary>
        /// Returns currently highlightd country's region.
        /// </summary>
        /// <value>The country region highlighted.</value>
        public Region countryRegionHighlighted { get { return _countryRegionHighlighted; } }

        int _countryRegionHighlightedIndex = -1;

        /// <summary>
        /// Returns currently highlighted region of the country.
        /// </summary>
        public int countryRegionHighlightedIndex { get { return _countryRegionHighlightedIndex; } }

        int _countryLastClicked = -1;

        /// <summary>
        /// Returns the last clicked country.
        /// </summary>
        public int countryLastClicked { get { return _countryLastClicked; } }

        int _countryRegionLastClicked = -1;

        /// <summary>
        /// Returns the last clicked country region index.
        /// </summary>
        public int countryRegionLastClicked { get { return _countryRegionLastClicked; } }

        [SerializeField]
        bool
            _enableCountryHighlight = true;

        /// <summary>
        /// Enable/disable country highlight when mouse is over.
        /// </summary>
        public bool enableCountryHighlight {
            get {
                return _enableCountryHighlight;
            }
            set {
                if (_enableCountryHighlight != value) {
                    _enableCountryHighlight = value;
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Set whether all regions of active country should be highlighted.
        /// </summary>
        [SerializeField]
        bool
            _highlightAllCountryRegions = false;

        public bool highlightAllCountryRegions {
            get {
                return _highlightAllCountryRegions;
            }
            set {
                if (_highlightAllCountryRegions != value) {
                    _highlightAllCountryRegions = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float
            _countryHighlightMaxScreenAreaSize = 1f;

        /// <summary>
        /// Defines the maximum area of a highlighted country. To prevent filling the whole screen with the highlight color, you can reduce this value and if the highlighted screen area size is greater than this factor (1=whole screen) the country won't be filled (it will behave as selected though)
        /// </summary>
        public float countryHighlightMaxScreenAreaSize {
            get {
                return _countryHighlightMaxScreenAreaSize;
            }
            set {
                if (_countryHighlightMaxScreenAreaSize != value) {
                    _countryHighlightMaxScreenAreaSize = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _enableContinentHighlight;

        /// <summary>
        /// Enable/disable continent highlight when mouse is over.
        /// </summary>
        public bool enableContinentHighlight {
            get {
                return _enableContinentHighlight;
            }
            set {
                if (_enableContinentHighlight != value) {
                    _enableContinentHighlight = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _showFrontiers = true;

        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showFrontiers {
            get {
                return _showFrontiers;
            }
            set {
                if (value != _showFrontiers) {
                    _showFrontiers = value;
                    isDirty = true;

                    if (frontiersLayer != null) {
                        frontiersLayer.SetActive(_showFrontiers);
                    } else if (_showFrontiers) {
                        DrawFrontiers();
                    }
                }
            }
        }

        [SerializeField]
        bool
            _showCoastalFrontiers = false;

        /// <summary>
        /// Toggle coastal frontiers visibility.
        /// </summary>
        public bool showCoastalFrontiers {
            get {
                return _showCoastalFrontiers;
            }
            set {
                if (value != _showCoastalFrontiers) {
                    _showCoastalFrontiers = value;
                    isDirty = true;
                    OptimizeFrontiers();
                    DrawFrontiers();
                    DrawInlandFrontiers();
                }
            }
        }

        [SerializeField]
        Color
            _fillColor = new Color(1, 0, 0, 0.7f);

        /// <summary>
        /// Fill color to use when the mouse hovers a country's region.
        /// </summary>
        public Color fillColor {
            get {
                if (hudMatCountry != null) {
                    return hudMatCountry.color;
                } else {
                    return _fillColor;
                }
            }
            set {
                if (_fillColor != value) {
                    _fillColor = value;
                    isDirty = true;
                    if (hudMatCountry != null && _fillColor != hudMatCountry.color) {
                        hudMatCountry.color = _fillColor;
                    }
                }
            }
        }


        [SerializeField]
        float
            _countryHighlightFadeDuration;

        /// <summary>
        /// Duration of the fade
        /// </summary>
        public float countryHighlightFadeDuration {
            get {
                return _countryHighlightFadeDuration;
            }
            set {
                if (_countryHighlightFadeDuration != value) {
                    _countryHighlightFadeDuration = Mathf.Max(0, value);
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        Color
            _frontiersColor = Color.green;

        /// <summary>
        /// Global color for frontiers.
        /// </summary>
        public Color frontiersColor {
            get {
                if (frontiersMatCurrent != null) {
                    return frontiersMatCurrent.color;
                } else {
                    return _frontiersColor;
                }
            }
            set {
                if (value != _frontiersColor) {
                    _frontiersColor = value;
                    isDirty = true;
                    UpdateFrontiersMat();
                }
            }
        }

        [SerializeField]
        bool
            _showOutline = true;

        /// <summary>
        /// Toggle outline visibility.
        /// </summary>
        public bool showOutline {
            get {
                return _showOutline;
            }
            set {
                if (value != _showOutline) {
                    _showOutline = value;
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        bool
            _showProvinceCountryOutline;

        /// <summary>
        /// Toggle outline visibility of the country when a province is highlighted
        /// </summary>
        public bool showProvinceCountryOutline {
            get {
                return _showProvinceCountryOutline;
            }
            set {
                if (value != _showProvinceCountryOutline) {
                    _showProvinceCountryOutline = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        Color
            _outlineColor = Color.black;

        /// <summary>
        /// Outline color.
        /// </summary>
        public Color outlineColor {
            get {
                if (outlineMatCurrent != null) {
                    return outlineMatCurrent.color;
                } else {
                    return _outlineColor;
                }
            }
            set {
                if (value != _outlineColor) {
                    _outlineColor = value;
                    isDirty = true;

                    if (outlineMatCurrent != null && _outlineColor != outlineMatCurrent.color) {
                        outlineMatCurrent.color = _outlineColor;
                    }
                }
            }
        }

        [SerializeField]
        FRONTIERS_DETAIL
            _frontiersDetail = FRONTIERS_DETAIL.Low;

        public FRONTIERS_DETAIL frontiersDetail {
            get { return _frontiersDetail; }
            set {
                if (_frontiersDetail != value) {
                    _frontiersDetail = value;
                    isDirty = true;
                    ReloadData();
                }
            }
        }



        [SerializeField]
        FRONTIERS_THICKNESS
            _frontiersThicknessMode = FRONTIERS_THICKNESS.Thin;

        /// <summary>
        /// Thin or custom thickness.
        /// </summary>
        public FRONTIERS_THICKNESS frontiersThicknessMode {
            get {
                return _frontiersThicknessMode;
            }
            set {
                if (value != _frontiersThicknessMode) {
                    _frontiersThicknessMode = value;
                    isDirty = true;
                    DrawFrontiers();
                }
            }
        }



        [SerializeField]
        float
            _frontiersThickness = 0.01f;

        /// <summary>
        /// Thickness for the country frontiers. Only supported on systems capable of Shader Model 4+
        /// </summary>
        public float frontiersThickness {
            get {
                return _frontiersThickness;
            }
            set {
                if (value != _frontiersThickness) {
                    _frontiersThickness = value;
                    isDirty = true;
                    UpdateFrontiersMat();
                }
            }
        }

        [SerializeField]
        bool
            _showCountryNames = false;

        public bool showCountryNames {
            get {
                return _showCountryNames;
            }
            set {
                if (value != _showCountryNames) {
#if TRACE_CTL
					Debug.Log ("CTL " + DateTime.Now + ": showcountrynames!");
#endif
                    _showCountryNames = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        if (!showCountryNames) {
                            DestroyMapLabels();
                        } else {
                            DrawMapLabels();
                            // Cool scrolling animation for map labels following...
                            if (Application.isPlaying && _labelsRenderMethod == LABELS_RENDER_METHOD.Blended) {
                                for (int k = 0; k < _countries.Length; k++) {
                                    GameObject o = _countries[k].labelTextGameObject;
                                    if (o != null) {
                                        LabelAnimator anim = o.AddComponent<LabelAnimator>();
                                        anim.destPos = o.transform.localPosition;
                                        anim.startPos = o.transform.localPosition + Misc.Vector3right * 100.0f * Mathf.Sign(o.transform.localPosition.x);
                                        anim.duration = 1.0f;
                                        anim.map = this;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        [SerializeField]
        COUNTRY_LABELS_ORIENTATION _countryLabelsOrientation = COUNTRY_LABELS_ORIENTATION.Automatic;

        /// <summary>
        /// Line orientation for country labels
        /// </summary>
        public COUNTRY_LABELS_ORIENTATION countryLabelsOrientation {
            get { return _countryLabelsOrientation; }
            set {
                if (_countryLabelsOrientation != value) {
                    _countryLabelsOrientation = value;
                    RedrawMapLabels();
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _countryLabelsEnableAutomaticFade = true;

        /// <summary>
        /// Automatic fading of country labels depending on camera distance and label screen size
        /// </summary>
        public bool countryLabelsEnableAutomaticFade {
            get { return _countryLabelsEnableAutomaticFade; }
            set {
                if (_countryLabelsEnableAutomaticFade != value) {
                    _countryLabelsEnableAutomaticFade = value;
                    RedrawMapLabels();
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsAutoFadeMaxHeight = 0.3f;

        /// <summary>
        /// Max height of a label relative to screen height (0..1) at which fade out starts
        /// </summary>
        public float countryLabelsAutoFadeMaxHeight {
            get {
                return _countryLabelsAutoFadeMaxHeight;
            }
            set {
                if (value != _countryLabelsAutoFadeMaxHeight) {
                    _countryLabelsAutoFadeMaxHeight = value;
                    _countryLabelsAutoFadeMinHeight = Mathf.Min(_countryLabelsAutoFadeMaxHeight, _countryLabelsAutoFadeMinHeight);
                    isDirty = true;
                    FadeCountryLabels();
                }
            }
        }

        [SerializeField]
        int
            _countryLabelsFadePerFrame = 25;

        /// <summary>
        /// Max number of country labels faded per frame
        /// </summary>
        public int countryLabelsFadePerFrame {
            get {
                return _countryLabelsFadePerFrame;
            }
            set {
                if (value != _countryLabelsFadePerFrame) {
                    _countryLabelsFadePerFrame = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsAutoFadeMaxHeightFallOff = 0.2f;

        /// <summary>
        /// Fall off for fade labels when height is greater than min height
        /// </summary>
        public float countryLabelsAutoFadeMaxHeightFallOff {
            get {
                return _countryLabelsAutoFadeMaxHeightFallOff;
            }
            set {
                if (value != _countryLabelsAutoFadeMaxHeightFallOff) {
                    _countryLabelsAutoFadeMaxHeightFallOff = value;
                    isDirty = true;
                    FadeCountryLabels();
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsAutoFadeMinHeight = 0.02f;

        /// <summary>
        /// Min height of a label relative to screen height (0..1) at which fade out starts
        /// </summary>
        public float countryLabelsAutoFadeMinHeight {
            get {
                return _countryLabelsAutoFadeMinHeight;
            }
            set {
                if (value != _countryLabelsAutoFadeMinHeight) {
                    _countryLabelsAutoFadeMinHeight = value;
                    _countryLabelsAutoFadeMaxHeight = Mathf.Max(_countryLabelsAutoFadeMaxHeight, _countryLabelsAutoFadeMinHeight);
                    isDirty = true;
                    FadeCountryLabels();
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsAutoFadeMinHeightFallOff = 0.02f;

        /// <summary>
        /// Fall off for fade labels when height is less than min height
        /// </summary>
        public float countryLabelsAutoFadeMinHeightFallOff {
            get {
                return _countryLabelsAutoFadeMinHeightFallOff;
            }
            set {
                if (value != _countryLabelsAutoFadeMinHeightFallOff) {
                    _countryLabelsAutoFadeMinHeightFallOff = value;
                    isDirty = true;
                    FadeCountryLabels();
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsAbsoluteMinimumSize = 0.5f;

        public float countryLabelsAbsoluteMinimumSize {
            get {
                return _countryLabelsAbsoluteMinimumSize;
            }
            set {
                if (value != _countryLabelsAbsoluteMinimumSize) {
                    _countryLabelsAbsoluteMinimumSize = value;
                    isDirty = true;
                    if (_showCountryNames)
                        RedrawMapLabels();
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsSize = 0.25f;

        public float countryLabelsSize {
            get {
                return _countryLabelsSize;
            }
            set {
                if (value != _countryLabelsSize) {
                    _countryLabelsSize = value;
                    isDirty = true;
                    if (_showCountryNames)
                        RedrawMapLabels();
                }
            }
        }

        [SerializeField]
        LABELS_QUALITY
            _labelsQuality = LABELS_QUALITY.Medium;

        public LABELS_QUALITY labelsQuality {
            get {
                return _labelsQuality;
            }
            set {
                if (value != _labelsQuality) {
                    _labelsQuality = value;
                    isDirty = true;
                    if (_showCountryNames) {
                        DestroyOverlay(); // needs to recreate the render texture
                        DrawMapLabels();
                    }
                }
            }
        }


        [SerializeField]
        LABELS_ANTIALIAS
            _labelsAntialias = LABELS_ANTIALIAS.None;

        public LABELS_ANTIALIAS labelsAntialias {
            get {
                return _labelsAntialias;
            }
            set {
                if (value != _labelsAntialias) {
                    _labelsAntialias = value;
                    isDirty = true;
                    DestroyOverlay(); // needs to recreate the render texture
                    Redraw();
                }
            }
        }

        [SerializeField]
        LABELS_RENDER_METHOD
            _labelsRenderMethod = LABELS_RENDER_METHOD.Blended;

        /// <summary>
        /// Hoe labels should be rendered. Blended means they are rendered into the globe texture. 
        /// </summary>
        public LABELS_RENDER_METHOD labelsRenderMethod {
            get {
                return _labelsRenderMethod;
            }
            set {
                if (value != _labelsRenderMethod) {
                    _labelsRenderMethod = value;
                    isDirty = true;
                    if (_showCountryNames) {
                        DestroyOverlay(); // needs to recreate the render texture
                        RedrawMapLabels();
                    }
                }
            }
        }

        [SerializeField]
        float
            _labelsElevation = 0;

        public float labelsElevation {
            get {
                return _labelsElevation;
            }
            set {
                if (value != _labelsElevation) {
                    _labelsElevation = value;
                    isDirty = true;
                    if (_labelsRenderMethod == LABELS_RENDER_METHOD.WorldSpace) {
                        DestroyOverlay();
                        DrawMapLabels();
                    } else {
                        if (sphereOverlayLayer != null) {
                            AdjustSphereOverlayLayerScale();
                        }
                    }
                }
            }
        }

        [SerializeField]
        bool
            _showLabelsShadow = true;

        /// <summary>
        /// Draws a shadow under map labels. Specify the color using labelsShadowColor.
        /// </summary>
        /// <value><c>true</c> if show labels shadow; otherwise, <c>false</c>.</value>
        public bool showLabelsShadow {
            get {
                return _showLabelsShadow;
            }
            set {
                if (value != _showLabelsShadow) {
                    _showLabelsShadow = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        RedrawMapLabels();
                    }
                }
            }
        }

        [SerializeField]
        Color
            _countryLabelsColor = Color.white;

        /// <summary>
        /// Color for map labels.
        /// </summary>
        public Color countryLabelsColor {
            get {
                return _countryLabelsColor;
            }
            set {
                if (value != _countryLabelsColor) {
                    _countryLabelsColor = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        RedrawMapLabels();
                    }
                }
            }
        }

        [SerializeField]
        Color
            _countryLabelsShadowColor = Color.black;

        /// <summary>
        /// Color for map labels.
        /// </summary>
        public Color countryLabelsShadowColor {
            get {
                return _countryLabelsShadowColor;
            }
            set {
                if (value != _countryLabelsShadowColor) {
                    _countryLabelsShadowColor = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        if (countryLabelsTextEngine == TEXT_ENGINE.TextMeshPro) {
                            DrawMapLabels();
                        } else {
                            labelsShadowMaterial.color = _countryLabelsShadowColor;
                        }
                    }
                }
            }
        }

        [SerializeField]
        float
        _countryLabelsShadowOffset = 1f;

        public float countryLabelsShadowOffset {
            get {
                return _countryLabelsShadowOffset;
            }
            set {
                if (value != _countryLabelsShadowOffset) {
                    _countryLabelsShadowOffset = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        RedrawMapLabels();
                    }
                }
            }
        }


        [SerializeField]
        Font
            _countryLabelsFont;

        /// <summary>
        /// Gets or sets the default font for country labels
        /// </summary>
        public Font countryLabelsFont {
            get {
                return _countryLabelsFont;
            }
            set {
                if (value != _countryLabelsFont) {
                    _countryLabelsFont = value;
                    isDirty = true;
                    ReloadFont();
                    RedrawMapLabels();
                }
            }
        }



        [SerializeField]
        TEXT_ENGINE _countryLabelsTextEngine = TEXT_ENGINE.TextMeshStandard;

        public TEXT_ENGINE countryLabelsTextEngine {
            get {
                return _countryLabelsTextEngine;
            }
            set {
                if (_countryLabelsTextEngine != value) {
                    _countryLabelsTextEngine = value;
                    isDirty = true;
                    ReloadFont();
                    RedrawMapLabels();
                }
            }
        }



        [SerializeField]
        TMP_FontAsset _countryLabelsFontTMPro;

        public TMP_FontAsset countryLabelsFontTMPro {
            get {
                return _countryLabelsFontTMPro;
            }
            set {
                if (_countryLabelsFontTMPro != value) {
                    _countryLabelsFontTMPro = value;
                    ReloadFont();
                    RedrawMapLabels();
                }
            }
        }



        [SerializeField]
        Material _countryLabelsFontTMProMaterial;

        public Material countryLabelsFontTMProMaterial {
            get {
                return _countryLabelsFontTMProMaterial;
            }
            set {
                if (_countryLabelsFontTMProMaterial != value) {
                    _countryLabelsFontTMProMaterial = value;
                    ReloadFont();
                    RedrawMapLabels();
                }
            }
        }


        [SerializeField]
        bool
            _labelsFaceToCamera = false;

        /// <summary>
        /// Ensure labels are always readable
        /// </summary>
        public bool labelsFaceToCamera {
            get {
                return _labelsFaceToCamera;
            }
            set {
                if (value != _labelsFaceToCamera) {
                    _labelsFaceToCamera = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _enableCountryEnclaves = false;

        /// <summary>
        /// Allows a country to be surrounded by another country
        /// </summary>
        public bool enableCountryEnclaves {
            get {
                return _enableCountryEnclaves;
            }
            set {
                if (value != _enableCountryEnclaves) {
                    _enableCountryEnclaves = value;
                    isDirty = true;
                }
            }
        }



        string _countryAttributeFile = COUNTRY_ATTRIB_DEFAULT_FILENAME;

        public string countryAttributeFile {
            get { return _countryAttributeFile; }
            set {
                if (value != _countryAttributeFile) {
                    _countryAttributeFile = value;
                    if (_countryAttributeFile == null)
                        _countryAttributeFile = COUNTRY_ATTRIB_DEFAULT_FILENAME;
                    isDirty = true;
                    ReloadCountryAttributes();
                }
            }
        }



        #region Public API area

        /// <summary>
        /// Creates a new country with no regions.
        /// </summary>
        /// <returns>Returns the index of the new country in the map.countries array.</returns>
        public int CountryCreate (string name, string continent) {
            int countryIndex = GetCountryIndex(name);
            if (countryIndex >= 0)
                return -1;
            Country country = new Country(name, continent);
            return CountryAdd(country);
        }

        /// <summary>
        /// Adds a new country which has been properly initialized. Used by the Map Editor. Name must be unique.
        /// </summary>
        /// <returns><c>-1</c> if country was not added, <c>new country index</c> otherwise.</returns>
        public int CountryAdd (Country country) {
            int countryIndex = GetCountryIndex(country.name);
            if (countryIndex >= 0)
                return -1;
            Country[] newCountries = new Country[countries.Length + 1];
            for (int k = 0; k < countries.Length; k++) {
                newCountries[k] = countries[k];
            }
            int newCountryIndex = newCountries.Length - 1;
            newCountries[newCountryIndex] = country;
            countries = newCountries;
            RefreshCountryGeometry(country);
            return newCountryIndex;
        }

        /// <summary>
        /// Renames the country. Name must be unique, different from current and one letter minimum.
        /// </summary>
        /// <returns><c>true</c> if country was renamed, <c>false</c> otherwise.</returns>
        public bool CountryRename (string oldName, string newName) {
            if (newName == null || newName.Length == 0)
                return false;
            int countryIndex = GetCountryIndex(oldName);
            int newCountryIndex = GetCountryIndex(newName);
            if (countryIndex < 0 || newCountryIndex >= 0)
                return false;

            // Ensure dependencies are loaded
            if (_provinces == null) ReadProvincesGeoData();
            if (_cities == null) ReadCitiesGeoData();
            if (mountPoints == null) ReloadMountPointsData();

            countries[countryIndex].name = newName;
            lastCountryLookupCount = -1;
            return true;

        }


        /// <summary>
        /// Deletes the country. Optionally also delete its dependencies (provinces, cities, mountpoints).
        /// </summary>
        /// <returns><c>true</c> if country was deleted, <c>false</c> otherwise.</returns>
        public bool CountryDelete (int countryIndex, bool deleteDependencies, bool redraw = true) {
            if (internal_CountryDelete(countryIndex, deleteDependencies)) {
                // Update lookup dictionaries
                lastCountryLookupCount = -1;
                return true;
            }
            if (redraw) {
                Redraw();
            }
            return false;
        }


        /// <summary>
        /// Deletes all provinces from a country.
        /// </summary>
        /// <returns><c>true</c>, if provinces where deleted, <c>false</c> otherwise.</returns>
        public bool CountryDeleteProvinces (int countryIndex) {
            int numProvinces = provinces.Length;
            List<Province> newProvinces = new List<Province>(numProvinces);
            for (int k = 0; k < numProvinces; k++) {
                if (provinces[k] != null && provinces[k].countryIndex != countryIndex) {
                    newProvinces.Add(provinces[k]);
                }
            }
            provinces = newProvinces.ToArray();
            lastProvinceLookupCount = -1;
            return true;
        }

        public void CountriesDeleteFromContinent (string continentName) {

            HideCountryRegionHighlights(true);

            ProvincesDeleteOfSameContinent(continentName);
            CitiesDeleteFromContinent(continentName);
            MountPointsDeleteFromSameContinent(continentName);

            List<Country> newAdmins = new List<Country>(countries.Length - 1);
            for (int k = 0; k < countries.Length; k++) {
                if (!countries[k].continent.Equals(continentName)) {
                    newAdmins.Add(countries[k]);
                } else {
                    int lastIndex = newAdmins.Count - 1;
                    // Updates country index in provinces
                    if (provinces != null) {
                        for (int p = 0; p < _provinces.Length; p++) {
                            if (_provinces[p].countryIndex > lastIndex) {
                                _provinces[p].countryIndex--;
                            }
                        }
                    }
                    // Updates country index in cities
                    if (cities != null) {
                        for (int c = 0; c < cities.Count; c++) {
                            if (cities[c].countryIndex > lastIndex) {
                                cities[c].countryIndex--;
                            }
                        }
                    }
                    // Updates country index in mount points
                    if (mountPoints != null) {
                        for (int c = 0; c < mountPoints.Count; c++) {
                            if (mountPoints[c].countryIndex > lastIndex) {
                                mountPoints[c].countryIndex--;
                            }
                        }
                    }
                }
            }

            countries = newAdmins.ToArray();
            lastCountryLookupCount = -1;

        }


        /// <summary>
        /// Given a country name returns the Country object.
        /// </summary>
        /// <returns>The country.</returns>
        /// <param name="countryName">Country name.</param>
        public Country GetCountry (string countryName) {
            int countryIndex = GetCountryIndex(countryName);
            return GetCountry(countryIndex);
        }

        /// <summary>
        /// Given a country index returns the Country object
        /// </summary>
        /// <returns>The country.</returns>
        /// <param name="countryIndex">Country index.</param>
        public Country GetCountry (int countryIndex) {
            if (countryIndex >= 0 && countryIndex < countries.Length) {
                return countries[countryIndex];
            }
            return null;
        }

        /// <summary>
        /// Returns the index of a country in the countries collection by its name.
        /// </summary>
        public int GetCountryIndex (string countryName) {
            int countryIndex;
            if (countryLookup != null && _countryLookup.TryGetValue(countryName, out countryIndex))
                return countryIndex;
            else
                return -1;
        }

        /// <summary>
        /// Returns the country capital city object.
        /// </summary>
        /// <returns>The country capital.</returns>
        /// <param name="countryName">Country name.</param>
        public City GetCountryCapital (string countryName) {
            int cityIndex = GetCountryCapitaIndex(countryName);
            if (cityIndex < 0)
                return null;
            return _cities[cityIndex];
        }

        /// <summary>
        /// Returns the index of the capital city of a country
        /// </summary>
        /// <returns>The country capita index.</returns>
        /// <param name="countryName">Country name.</param>
        public int GetCountryCapitaIndex (string countryName) {
            int countryIndex = GetCountryIndex(countryName);
            if (countryIndex < 0)
                return -1;
            return _countries[countryIndex].cityCapitalIndex;
        }


        /// <summary>
        /// Returns the index of a country in the countries by its FIPS 10 4 code.
        /// </summary>
        public int GetCountryIndexByFIPS10_4 (string fips) {
            if (string.IsNullOrEmpty(fips)) return -1;
            for (int k = 0; k < _countries.Length; k++) {
                if (_countries[k].fips10_4.Equals(fips)) {
                    return k;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of a country in the countries by its ISO A-2 code.
        /// </summary>
        public int GetCountryIndexByISO_A2 (string iso_a2) {
            if (string.IsNullOrEmpty(iso_a2)) return -1;
            for (int k = 0; k < _countries.Length; k++) {
                if (_countries[k].iso_a2.Equals(iso_a2)) {
                    return k;
                }
            }
            return -1;
        }


        /// <summary>
        /// Returns the index of a country in the countries by its ISO A-3 code.
        /// </summary>
        public int GetCountryIndexByISO_A3 (string iso_a3) {
            if (string.IsNullOrEmpty(iso_a3)) return -1;
            for (int k = 0; k < _countries.Length; k++) {
                if (_countries[k].iso_a3.Equals(iso_a3)) {
                    return k;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of a country in the countries by its ISO N-3 code.
        /// </summary>
        public int GetCountryIndexByISO_N3 (string iso_n3) {
            if (string.IsNullOrEmpty(iso_n3)) return -1;
            for (int k = 0; k < _countries.Length; k++) {
                if (_countries[k].iso_n3.Equals(iso_n3)) {
                    return k;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of a country in the countries collection by its reference.
        /// </summary>
        public int GetCountryIndex (Country country) {
            int countryIndex;
            if (countryLookup.TryGetValue(country.name, out countryIndex))
                return countryIndex;
            else
                return -1;
        }


        /// <summary>
        /// Gets the index of the country that contains the provided map coordinates. This will ignore hidden countries.
        /// </summary>
        public int GetCountryIndex (Vector3 spherePosition) {
            int countryIndex, countryRegionIndex;
            if (GetCountryUnderSpherePosition(spherePosition, out countryIndex, out countryRegionIndex)) {
                return countryIndex;
            }
            return -1;
        }

        /// <summary>
        /// Gets the country that contains a given map coordinate or the country whose center is nearest to that coordinate.
        /// </summary>
        public int GetCountryNearPoint (Vector3 spherePosition) {
            int countryIndex = GetCountryIndex(spherePosition);
            if (countryIndex >= 0)
                return countryIndex;
            float minDist = float.MaxValue;
            int countryCount = countries.Length;
            for (int k = 0; k < countryCount; k++) {
                Country country = countries[k];
                float dist = FastVector.SqrDistanceByValue(country.localPosition, spherePosition); // Vector3.SqrMagnitude (country.sphereCenter - spherePosition);
                if (dist < minDist) {
                    minDist = dist;
                    countryIndex = k;
                }
            }
            return countryIndex;
        }


        /// <summary>
		/// Returns a list of countries whose attributes matches predicate
		/// </summary>
		public void GetCountries (AttribPredicate predicate, List<Country> results) {
            if (results == null) return;
            for (int k = 0; k < _countries.Length; k++) {
                Country country = _countries[k];
                if (country.hasAttributes && predicate(country.attrib))
                    results.Add(country);
            }
        }


        /// <summary>
        /// Gets XML attributes of all countries in jSON format.
        /// </summary>
        public string GetCountriesAttributes (bool prettyPrint = true) {
            if (countries == null) return null;
            return GetCountriesAttributes(new List<Country>(_countries), prettyPrint);
        }

        /// <summary>
        /// Gets XML attributes of provided countries in jSON format.
        /// </summary>
        public string GetCountriesAttributes (List<Country> countries, bool prettyPrint = true) {
            JSONObject composed = new JSONObject();
            for (int k = 0; k < countries.Count; k++) {
                Country country = countries[k];
                if (country.hasAttributes && country.attrib.keys != null)
                    composed.AddField(country.name, country.attrib);
            }
            return composed.Print(prettyPrint);
        }

        /// <summary>
        /// Sets countries attributes from a jSON formatted string.
        /// </summary>
        public void SetCountriesAttributes (string jSON) {
            JSONObject composed = new JSONObject(jSON);
            if (composed.keys == null)
                return;
            int keyCount = composed.keys.Count;
            for (int k = 0; k < keyCount; k++) {
                string countryName = composed.keys[k];
                int countryIndex = GetCountryIndex(countryName);
                if (countryIndex >= 0 && countryIndex < _countries.Length) {
                    _countries[countryIndex].attrib = composed[k];
                }
            }
        }



        /// <summary>
        /// Used by Editor. Returns the country index by screen position defined by a ray in the Scene View.
        /// </summary>
        public bool GetCountryIndex (Ray ray, out int countryIndex, out int regionIndex) {
            Vector3 hitPos;
            if (GetGlobeIntersection(ray, out hitPos)) {
                Vector3 localHit = transform.InverseTransformPoint(hitPos);
                if (GetCountryUnderMouse(localHit, out countryIndex, out regionIndex)) {
                    return true;
                }
            }
            countryIndex = -1;
            regionIndex = -1;
            return false;
        }

        /// <summary>
        /// Returns all neighbour countries
        /// </summary>
        public List<Country> CountryNeighbours (int countryIndex) {

            List<Country> countryNeighbours = new List<Country>();

            // Get country object
            Country country = countries[countryIndex];

            // Iterate for all regions (a country can have several separated regions)
            for (int countryRegionIndex = 0; countryRegionIndex < country.regions.Count; countryRegionIndex++) {
                Region countryRegion = country.regions[countryRegionIndex];

                // Get the neighbours for this region
                for (int neighbourIndex = 0; neighbourIndex < countryRegion.neighbours.Count; neighbourIndex++) {
                    Region neighbour = countryRegion.neighbours[neighbourIndex];
                    Country neighbourCountry = (Country)neighbour.entity;
                    if (!countryNeighbours.Contains(neighbourCountry)) {
                        countryNeighbours.Add(neighbourCountry);
                    }
                }
            }

            return countryNeighbours;
        }


        /// <summary>
        /// Get neighbours of the main region of a country
        /// </summary>
        public List<Country> CountryNeighboursOfMainRegion (int countryIndex) {
            Country country = countries[countryIndex];
            return CountryNeighboursOfRegion(countryIndex, country.mainRegionIndex);
        }


        /// <summary>
        /// Get neighbours of a given region of a country
        /// </summary>
        public List<Country> CountryNeighboursOfRegion (int countryIndex, int regionIndex) {

            List<Country> countryNeighbours = new List<Country>();

            // Get main region
            Country country = countries[countryIndex];
            Region countryRegion = country.regions[regionIndex];

            // Get the neighbours for this region
            for (int neighbourIndex = 0; neighbourIndex < countryRegion.neighbours.Count; neighbourIndex++) {
                Region neighbour = countryRegion.neighbours[neighbourIndex];
                Country neighbourCountry = (Country)neighbour.entity;
                if (!countryNeighbours.Contains(neighbourCountry)) {
                    countryNeighbours.Add(neighbourCountry);
                }
            }
            return countryNeighbours;
        }

        /// <summary>
        /// Get neighbours of the currently selected region
        /// </summary>
        public List<Country> CountryNeighboursOfCurrentRegion () {

            List<Country> countryNeighbours = new List<Country>();

            // Get main region
            Region selectedRegion = countryRegionHighlighted;
            if (selectedRegion == null)
                return countryNeighbours;

            // Get the neighbours for this region
            for (int neighbourIndex = 0; neighbourIndex < selectedRegion.neighbours.Count; neighbourIndex++) {
                Region neighbour = selectedRegion.neighbours[neighbourIndex];
                Country neighbourCountry = (Country)neighbour.entity;
                if (!countryNeighbours.Contains(neighbourCountry)) {
                    countryNeighbours.Add(neighbourCountry);
                }
            }
            return countryNeighbours;
        }

        public bool GetCountryUnderSpherePosition (Vector3 spherePoint, out int countryIndex, out int countryRegionIndex) {
            return GetCountryUnderMouse(spherePoint, out countryIndex, out countryRegionIndex);
        }


        /// <summary>
        /// Starts navigation to target country. Returns false if country is not found.
        /// </summary>
        public CallbackHandler FlyToCountry (Country country) {
            int countryIndex = GetCountryIndex(country);
            if (countryIndex >= 0) {
                return FlyToCountry(countryIndex);
            }
            return CallbackHandler.Null;
        }

        /// <summary>
        /// Starts navigation to target country. Returns false if country is not found.
        /// </summary>
        /// <param name="name">Country name.</param>
        /// <param name="duration">Duration in seconds. If not specified, uses NavigationTime property.</param>
        /// <param name="zoomLevel">Zoom level. If not specified, uses current zoom level.</param>
        public CallbackHandler FlyToCountry (string name, float duration = -1, float zoomLevel = -1) {
            int countryIndex = GetCountryIndex(name);
            if (countryIndex >= 0) {
                if (duration < 0) duration = _navigationTime;
                return FlyToCountry(countryIndex, duration, zoomLevel);
            }
            return CallbackHandler.Null;
        }

        /// <summary>
        /// Starts navigation to target country by index in the countries collection. Returns false if country is not found.
        /// </summary>
        public CallbackHandler FlyToCountry (int countryIndex) {
            return FlyToCountry(countryIndex, _navigationTime);
        }

        /// <summary>
        /// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// </summary>
        public CallbackHandler FlyToCountry (int countryIndex, float duration) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return CallbackHandler.Null;
            return FlyToLocation(countries[countryIndex].localPosition, duration, -1f, _navigationBounceIntensity);
        }

        /// <summary>
        /// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Set zoomLevel to a value from 0 to 1 for the destination zoom level. A value of -1 will keep current zoom level.
        /// </summary>
        public CallbackHandler FlyToCountry (int countryIndex, float duration, float zoomLevel) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return CallbackHandler.Null;
            return FlyToLocation(countries[countryIndex].localPosition, duration, zoomLevel, _navigationBounceIntensity);
        }


        /// <summary>
        /// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Set zoomLevel to a value from 0 to 1 for the destination zoom level. A value of -1 will keep current zoom level.
        /// Set bounceIntensity to a value from 0 to 1 for a bouncing effect between current position and destination
        /// </summary>
        public CallbackHandler FlyToCountry (int countryIndex, float duration, float zoomLevel, float bounceIntensity) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return CallbackHandler.Null;
            return FlyToLocation(countries[countryIndex].localPosition, duration, zoomLevel, bounceIntensity);
        }

        /// <summary>
        /// Colorize all regions of specified country by name. Returns false if not found.
        /// </summary>
        public bool ToggleCountrySurface (string name, bool visible, Color color = new Color(), bool drawOutline = false, Color outlineColor = new Color()) {
            int countryIndex = GetCountryIndex(name);
            if (countryIndex >= 0) {
                ToggleCountrySurface(countryIndex, visible, color, null, drawOutline, outlineColor);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Colorize all regions of specified country by index in the countries collection.
        /// </summary>
        public void ToggleCountrySurface (int countryIndex, bool visible, Color color = new Color(), bool drawOutline = false, Color outlineColor = new Color()) {
            ToggleCountrySurface(countryIndex, visible, color, null, drawOutline, outlineColor);
        }


        /// <summary>
        /// Colorize all regions of specified country and assings a texture.
        /// </summary>
        public void ToggleCountrySurface (string name, bool visible, Color color, Texture2D texture, bool drawOutline = false, Color outlineColor = new Color()) {
            int countryIndex = GetCountryIndex(name);
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return;
            for (int r = 0; r < countries[countryIndex].regions.Count; r++) {
                ToggleCountryRegionSurface(countryIndex, r, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, drawOutline, outlineColor);
            }
        }

        /// <summary>
        /// Colorize all regions of specified country and assings a texture.
        /// </summary>
        public void ToggleCountrySurface (int countryIndex, bool visible, Color color, Texture2D texture, bool drawOutline = false, Color outlineColor = new Color()) {
            if (!visible) {
                HideCountrySurface(countryIndex);
                return;
            }
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return;
            int count = countries[countryIndex].regions.Count;
            for (int r = 0; r < count; r++) {
                ToggleCountryRegionSurface(countryIndex, r, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, drawOutline, outlineColor);
            }
        }

        /// <summary>
        /// Adds a country outline
        /// </summary>
        public GameObject DrawCountryOutline (string countryName, Color color) {
            int countryIndex = GetCountryIndex(countryName);
            return DrawCountryOutline(countryIndex, color);
        }

        /// <summary>
        /// Adds a country outline
        /// </summary>
        public GameObject DrawCountryOutline (int countryIndex, Color color) {
            if (!ValidCountryIndex(countryIndex)) return null;
            return ToggleCountryRegionOutline(countryIndex, countries[countryIndex].mainRegionIndex, true, color);
        }


        /// <summary>
        /// Draws an outline for a list of countries
        /// </summary>
        /// <param name="countryIndices"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        const double MIN_COUNTRY_GROUP_OUTLINE_AREA = 1e-3;
        const double CLIPPER_MULTIPLIER = 5000000d;
        const double CLIPPER_INV_MULTIPLIER = 1d / CLIPPER_MULTIPLIER;
        const double COUNTRY_OUTLINE_BRIDGE_OFFSET = 2500d;

        public GameObject DrawCountriesOutline (IList<int> countryIndices, Color color) {
            if (countryIndices == null || countryIndices.Count == 0) return null;
            List<List<IntPoint>> subjectPaths = new List<List<IntPoint>>();
            int count = countryIndices.Count;
            for (int k = 0; k < count; k++) {
                int countryIndex = countryIndices[k];
                if (!ValidCountryIndex(countryIndex)) {
                    continue;
                }
                Country country = countries[countryIndex];
                if (country == null || country.regions == null || country.regions.Count == 0) {
                    continue;
                }
                int countryRegionCount = country.regions.Count;
                for (int r = 0; r < countryRegionCount; r++) {
                    Region region = country.regions[r];
                    Vector2[] latlon = region.latlon;
                    if (latlon == null || latlon.Length < 3) {
                        continue;
                    }
                    List<IntPoint> path = new List<IntPoint>(latlon.Length);
                    for (int p = 0; p < latlon.Length; p++) {
                        Vector2 point = latlon[p];
                        path.Add(new IntPoint(point.x * CLIPPER_MULTIPLIER, point.y * CLIPPER_MULTIPLIER));
                    }
                    subjectPaths.Add(path);
                }
            }
            if (subjectPaths.Count == 0) return null;

            List<List<IntPoint>> unionPaths = new List<List<IntPoint>>();
            Clipper clipper = new Clipper();
            clipper.AddPaths(subjectPaths, PolyType.ptSubject, true);
            clipper.Execute(ClipType.ctUnion, unionPaths, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            if (unionPaths.Count == 0) return null;

            unionPaths = ApplyCountryOutlineOffset(unionPaths, COUNTRY_OUTLINE_BRIDGE_OFFSET);

            Country outlineContainer = new Country("Countries Outline", string.Empty);
            List<Region> candidateRegions = new List<Region>(unionPaths.Count);
            Dictionary<Region, double> regionAreas = new Dictionary<Region, double>(unionPaths.Count);
            double areaFactor = CLIPPER_INV_MULTIPLIER * CLIPPER_INV_MULTIPLIER;
            int unionCount = unionPaths.Count;
            for (int u = 0; u < unionCount; u++) {
                List<IntPoint> path = unionPaths[u];
                int pointCount = path.Count;
                if (pointCount < 3) {
                    continue;
                }
                double scaledArea = Clipper.Area(path);
                if (scaledArea <= 0) {
                    continue;
                }
                double area = scaledArea * areaFactor;
                if (area <= MIN_COUNTRY_GROUP_OUTLINE_AREA) {
                    continue;
                }
                Vector2[] newPoints = new Vector2[pointCount];
                for (int p = 0; p < pointCount; p++) {
                    IntPoint ip = path[p];
                    newPoints[p] = new Vector2((float)(ip.X * CLIPPER_INV_MULTIPLIER), (float)(ip.Y * CLIPPER_INV_MULTIPLIER));
                }
                Region newRegion = new Region(outlineContainer, outlineContainer.regions.Count);
                newRegion.UpdatePointsAndRect(newPoints);
                outlineContainer.regions.Add(newRegion);
                candidateRegions.Add(newRegion);
                regionAreas[newRegion] = area;
            }

            if (candidateRegions.Count == 0) {
                return null;
            }

            candidateRegions.Sort((regionA, regionB) => regionAreas[regionB].CompareTo(regionAreas[regionA]));

            List<Region> exteriorRegions = new List<Region>(candidateRegions.Count);
            int candidateCount = candidateRegions.Count;
            for (int c = 0; c < candidateCount; c++) {
                Region candidateRegion = candidateRegions[c];
                Vector2 centroid = candidateRegion.centroid;
                bool contained = false;
                int exteriorCount = exteriorRegions.Count;
                for (int e = 0; e < exteriorCount; e++) {
                    if (exteriorRegions[e].Contains(centroid)) {
                        contained = true;
                        break;
                    }
                }
                if (!contained) {
                    exteriorRegions.Add(candidateRegion);
                }
            }

            if (exteriorRegions.Count == 0) {
                return null;
            }

            GameObject root = new GameObject("CountriesOutline");
            root.layer = surfacesLayer.layer;
            root.transform.SetParent(surfacesLayer.transform, false);

            int regionCount = exteriorRegions.Count;
            for (int r = 0; r < regionCount; r++) {
                Region region = exteriorRegions[r];
                GameObject regionGO = new GameObject("RegionOutline");
                regionGO.layer = root.layer;
                regionGO.transform.SetParent(root.transform, false);
                region.surfaceGameObject = regionGO;
                DrawRegionOutline(region, regionGO, false, color);
            }
            return root;
        }

        static List<List<IntPoint>> ApplyCountryOutlineOffset (List<List<IntPoint>> paths, double offset) {
            if (paths == null || paths.Count == 0 || offset <= 0) {
                return paths;
            }
            ClipperOffset expand = new ClipperOffset();
            expand.AddPaths(paths, JoinType.jtMiter, EndType.etClosedPolygon);
            List<List<IntPoint>> expanded = new List<List<IntPoint>>();
            expand.Execute(ref expanded, offset);
            if (expanded.Count == 0) {
                return paths;
            }
            ClipperOffset shrink = new ClipperOffset();
            shrink.AddPaths(expanded, JoinType.jtMiter, EndType.etClosedPolygon);
            List<List<IntPoint>> contracted = new List<List<IntPoint>>();
            shrink.Execute(ref contracted, -offset);
            return contracted.Count > 0 ? contracted : expanded;
        }

        /// <summary>
        /// Toggles on/off a country outline
        /// </summary>
        public void ToggleCountryOutline (string countryName, bool visible, Color color = default(Color)) {
            int countryIndex = GetCountryIndex(countryName);
            ToggleCountryOutline(countryIndex, visible, color);
        }


        /// <summary>
        /// Toggles on/off a country outline
        /// </summary>
        public void ToggleCountryOutline (int countryIndex, bool visible, Color color = default(Color)) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return;
            Country country = countries[countryIndex];
            int regionsCount = country.regions.Count;
            for (int k = 0; k < regionsCount; k++) {
                ToggleRegionOutline(country.regions[k], visible, color);
            }
        }


        /// <summary>
        /// Toggles on/off a country region outline
        /// </summary>
        public GameObject ToggleCountryRegionOutline (int countryIndex, int regionIndex, bool visible, Color color = default(Color)) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return null;
            Country country = countries[countryIndex];
            int regionsCount = country.regions.Count;
            if (regionIndex < 0 || regionIndex >= regionsCount) return null;
            return ToggleRegionOutline(country.regions[regionIndex], visible, color);
        }


        /// <summary>
        /// Uncolorize/hide specified country by index in the countries collection.
        /// </summary>
        public void HideCountrySurface (int countryIndex) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return;
            for (int r = 0; r < countries[countryIndex].regions.Count; r++) {
                HideCountryRegionSurface(countryIndex, r);
            }
        }

        /// <summary>
        /// Colorize main region of a country by index in the countries collection.
        /// </summary>
        public GameObject ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color) {
            return ToggleCountryMainRegionSurface(countryIndex, visible, color, _showOutline, _outlineColor);
        }


        /// <summary>
        /// Add texture to main region of a country by index in the countries collection.
        /// </summary>
        public GameObject ToggleCountryMainRegionSurface (int countryIndex, bool visible, Texture2D texture) {
            return ToggleCountryMainRegionSurface(countryIndex, visible, Color.white, texture, Misc.Vector2one, Misc.Vector2zero, 0);
        }


        /// <summary>
        /// Colorize main region of a country by index in the countries collection.
        /// </summary>
        public GameObject ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color, bool drawOutline, Color outlineColor) {
            return ToggleCountryRegionSurface(countryIndex, countries[countryIndex].mainRegionIndex, visible, color, null, Misc.Vector3one, Misc.Vector3zero, 0, drawOutline, outlineColor);
        }

        /// <summary>
        /// Colorize main region of a country by index in the countries collection.
        /// </summary>
        /// <param name="texture">Optional texture or null to colorize with single color</param>
        public GameObject ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
            return ToggleCountryMainRegionSurface(countryIndex, visible, color, texture, textureScale, textureOffset, textureRotation, _showOutline, _outlineColor);
        }

        /// <summary>
        /// Colorize main region of a country by index in the countries collection.
        /// </summary>
        /// <param name="texture">Optional texture or null to colorize with single color</param>
        public GameObject ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool drawOutline, Color outlineColor) {
            return ToggleCountryRegionSurface(countryIndex, countries[countryIndex].mainRegionIndex, visible, color, texture, textureScale, textureOffset, textureRotation, drawOutline, outlineColor);
        }

        /// <summary>
        /// Colorize specified region of a country by index in the countries collection.
        /// </summary>
        /// <param name="texture">Optional texture or null to colorize with single color</param>
        public GameObject ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color) {
            return ToggleCountryRegionSurface(countryIndex, regionIndex, visible, color, null, Misc.Vector3one, Misc.Vector3zero, 0);
        }

        /// <summary>
        /// Colorize/texture specified region of a country by indexes.
        /// </summary>
        public GameObject ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
            return ToggleCountryRegionSurface(countryIndex, regionIndex, visible, color, texture, textureScale, textureOffset, textureRotation, _showOutline, _outlineColor);
        }

        /// <summary>
        /// Colorize/texture specified region of a country by indexes.
        /// </summary>
        public GameObject ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool drawOutline, Color outlineColor) {
            if (!visible) {
                HideCountryRegionSurface(countryIndex, regionIndex);
                return null;
            }

            Region region = countries[countryIndex].regions[regionIndex];
            int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, regionIndex);
            GameObject surf;
            // Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
            surfaces.TryGetValue(cacheIndex, out surf);

            // Should the surface be recreated?
            Material surfMaterial;
            if (surf != null) {
                if (region.surfaceGameObjectIsDirty || (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset ||
                                textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals(countryTexturizedMat.name)))) {
                    surfaces.Remove(cacheIndex);
                    DestroyImmediate(surf);
                    surf = null;
                }
            }
            // If it exists, activate and check proper material, if not create surface
            bool isHighlighted = countryHighlightedIndex == countryIndex && (countryRegionHighlightedIndex == regionIndex || _highlightAllCountryRegions) && _enableCountryHighlight;
            if (surf != null) {
                bool needMaterial = _highlightAllCountryRegions;
                if (!surf.activeSelf) {
                    surf.SetActive(true);
                    needMaterial = true;
                    UpdateSurfaceCount();
                } else {
                    // Check if material is ok
                    surfMaterial = surf.GetComponent<Renderer>().sharedMaterial;
                    if ((texture == null && !surfMaterial.name.Equals(countryColoredMat.name)) || (texture != null && !surfMaterial.name.Equals(countryTexturizedMat.name))
                                       || (surfMaterial.color != color && !isHighlighted) || (texture != null && region.customMaterial.mainTexture != texture))
                        needMaterial = true;
                }
                if (needMaterial) {
                    Material goodMaterial = GetCountryColoredTexturedMaterial(color, texture);
                    region.customMaterial = goodMaterial;
                    region.customColor = color;
                    region.customTexture = texture;
                    region.customTextureOffset = textureOffset;
                    region.customTextureRotation = textureRotation;
                    region.customTextureScale = textureScale;
                    ApplyMaterialToSurface(surf, goodMaterial);
                }
                if (drawOutline) {
                    ToggleRegionOutline(region, drawOutline, outlineColor);
                }
            } else {
                surfMaterial = GetCountryColoredTexturedMaterial(color, texture);
                surf = GenerateCountryRegionSurface(countryIndex, regionIndex, surfMaterial, textureScale, textureOffset, textureRotation, drawOutline, outlineColor, false);
            }
            // If it was highlighted, highlight it again
            if (region.customMaterial != null && isHighlighted && region.customMaterial.color != hudMatCountry.color) {
                Material clonedMat = Instantiate(region.customMaterial);
                clonedMat.name = region.customMaterial.name;
                clonedMat.color = hudMatCountry.color;
                surf.GetComponent<Renderer>().sharedMaterial = clonedMat;
                countryRegionHighlightedObj = surf;
            }
            return surf;
        }


        /// <summary>
        /// Uncolorize/hide specified country by index in the countries collection.
        /// </summary>
        public void HideCountryRegionSurface (int countryIndex, int regionIndex) {
            int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, regionIndex);
            if (surfaces.TryGetValue(cacheIndex, out GameObject surf)) {
                if (surf != null)
                    surf.SetActive(false);
                else
                    surfaces.Remove(cacheIndex);
            }
            UpdateSurfaceCount();
            countries[countryIndex].regions[regionIndex].customMaterial = null;
        }

        /// <summary>
        /// Highlights the country region specified.
        /// Internally used by the Editor component, but you can use it as well to temporarily mark a country region.
        /// </summary>
        /// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
        public GameObject ToggleCountryRegionSurfaceHighlight (int countryIndex, int regionIndex, Color color, bool drawOutline) {
            GameObject surf;
            Material mat = Instantiate(hudMatCountry);
            mat.color = color;
            mat.renderQueue--;
            int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, regionIndex);
            bool existsInCache = surfaces.ContainsKey(cacheIndex);
            if (existsInCache) {
                surf = surfaces[cacheIndex];
                if (surf == null) {
                    surfaces.Remove(cacheIndex);
                } else {
                    surf.SetActive(true);
                    surf.GetComponent<Renderer>().sharedMaterial = mat;
                }
            } else {
                surf = GenerateCountryRegionSurface(countryIndex, regionIndex, mat, Misc.Vector2one, Misc.Vector2zero, 0, drawOutline, _outlineColor, true);
            }
            return surf;
        }


        /// <summary>
        /// Hides all colorized regions of all countries.
        /// </summary>
        public void HideCountrySurfaces () {
            for (int c = 0; c < countries.Length; c++) {
                HideCountrySurface(c);
            }
        }



        /// <summary>
        /// Flashes specified country by index in the countries collection.
        /// </summary>
        public void BlinkCountry (int countryIndex, Color color1, Color color2, float duration, float blinkingSpeed, bool smoothBlink = false, bool includeAllRegions = false, bool drawOutline = false, Color outlineColor = default) {
            if (!ValidCountryIndex(countryIndex)) return;
            Country country = countries[countryIndex];
            if (country.regions == null) return;
            if (includeAllRegions) {
                int regionsCount = country.regions.Count;
                for (int k = 0; k < regionsCount; k++) {
                    BlinkCountry(countryIndex, k, color1, color2, duration, blinkingSpeed, smoothBlink);
                }
            } else {
                BlinkCountry(countryIndex, country.mainRegionIndex, color1, color2, duration, blinkingSpeed, drawOutline, outlineColor, smoothBlink);
            }
        }

        /// <summary>
        /// Flashes specified country's region.
        /// </summary>
        public void BlinkCountry (int countryIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed, bool smoothBlink = false) {
            BlinkCountry(countryIndex, regionIndex, color1, color2, duration, blinkingSpeed, _showOutline, _outlineColor, smoothBlink);
        }

        /// <summary>
        /// Flashes specified country's region.
        /// </summary>
        public void BlinkCountry (int countryIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed, bool drawOutline, Color outlineColor, bool smoothBlink = false) {
            if (!ValidCountryRegionIndex(countryIndex, regionIndex)) return;
            int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, regionIndex);
            if (!surfaces.TryGetValue(cacheIndex, out GameObject surf) || surf == null) {
                surf = GenerateCountryRegionSurface(countryIndex, regionIndex, hudMatCountry, drawOutline, outlineColor, true);
            }
            surf.SetActive(true);
            SurfaceBlinker sb = surf.GetComponent<SurfaceBlinker>();
            if (sb != null) DestroyImmediate(sb);
            sb = surf.AddComponent<SurfaceBlinker>();
            sb.blinkMaterial = hudMatBlinker;
            sb.color1 = color1;
            sb.color2 = color2;
            sb.duration = duration;
            sb.speed = blinkingSpeed;
            sb.customizableSurface = countries[countryIndex].regions[regionIndex];
            sb.smoothBlink = smoothBlink;
        }

        /// <summary>
        /// Returns proposedName if it's unique in the country collection. Otherwise it adds a suffix to the name to make it unique.
        /// </summary>
        /// <returns>The country unique name.</returns>
        public string GetCountryUniqueName (string proposedName) {
            string n = proposedName;
            int iteration = 2;
            while (countryLookup.ContainsKey(proposedName)) {
                proposedName = n + " " + iteration++;
            }
            return proposedName;
        }

        /// <summary>
        /// Returns an array of country names. The returning list can be grouped by continent.
        /// </summary>
        public string[] GetCountryNames (bool groupByContinent) {
            return GetCountryNames(groupByContinent, true);
        }

        /// <summary>
        /// Returns an array of country names. The returning list can be grouped by continent.
        /// </summary>
        public string[] GetCountryNames (bool groupByContinent, bool includeCountryIndex) {
            List<string> c = new List<string>();
            if (countries == null)
                return c.ToArray();
            string previousContinent = "";
            for (int k = 0; k < countries.Length; k++) {
                Country country = countries[k];
                if (groupByContinent) {
                    if (!country.continent.Equals(previousContinent)) {
                        c.Add(country.continent);
                        previousContinent = country.continent;
                    }
                    if (includeCountryIndex) {
                        c.Add(country.continent + "|" + country.name + " (" + k + ")");
                    } else {
                        c.Add(country.continent + "|" + country.name);
                    }
                } else {
                    if (includeCountryIndex) {
                        c.Add(country.name + " (" + k + ")");
                    } else {
                        c.Add(country.name);
                    }
                }
            }
            c.Sort();

            if (groupByContinent) {
                int k = -1;
                while (++k < c.Count) {
                    int i = c[k].IndexOf('|');
                    if (i > 0) {
                        c[k] = "  " + c[k].Substring(i + 1);
                    }
                }
            }
            return c.ToArray();
        }

        public string[] GetCountryNeighboursNames (int countryIndex, bool includeCountryIndex) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return null;
            List<string> c = new List<string>(50);
            Region region = countries[countryIndex].mainRegion;
            int nc = region.neighbours.Count;
            if (nc == 0)
                return c.ToArray();
            for (int k = 0; k < nc; k++) {
                Region nr = region.neighbours[k];
                Country oc = (Country)nr.entity;
                if (includeCountryIndex) {
                    int cIndex = GetCountryIndex(oc);
                    c.Add("  " + oc.name + " (" + cIndex + ")");
                } else {
                    c.Add("  " + oc.name);
                }
            }
            c.Sort();
            c.Insert(0, "Neighbours of " + countries[countryIndex].name);
            return c.ToArray();
        }

        /// <summary>
        /// Returns the colored surface (game object) of a country. If it has not been colored yet, it will return null.
        /// </summary>
        public GameObject GetCountryRegionSurfaceGameObject (int countryIndex, int regionIndex) {
            int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, regionIndex);
            GameObject surf = null;
            surfaces.TryGetValue(cacheIndex, out surf);
            return surf;
        }


        /// <summary>
        /// Returns the zoom level which shows the country main region in full screen
        /// </summary>
        /// <returns>The country region zoom level.</returns>
        /// <param name="countryIndex">Country index.</param>
        public float GetCountryMainRegionZoomExtents (int countryIndex) {
            if (countryIndex < 0 || countryIndex >= _countries.Length)
                return 0;

            Country country = _countries[countryIndex];
            return GetCountryRegionZoomExtents(countryIndex, country.mainRegionIndex);
        }


        /// <summary>
        /// Returns the zoom level which shows the country region in full screen
        /// </summary>
        /// <returns>The country region zoom level.</returns>
        /// <param name="countryIndex">Country index.</param>
        /// <param name="regionIndex">Region index.</param>
        public float GetCountryRegionZoomExtents (int countryIndex, int regionIndex) {
            if (countryIndex < 0 || countryIndex >= _countries.Length)
                return 0;

            Country country = _countries[countryIndex];
            if (regionIndex < 0 || regionIndex >= country.regions.Count)
                return 0;

            return GetRegionZoomExtents(country.regions[regionIndex]);
        }


        /// <summary>
        /// Returns a list of countries that are visible (front facing camera)
        /// </summary>
        public List<Country> GetVisibleCountries () {
            List<Country> vc = new List<Country>(30);
            Camera cam = mainCamera;
            for (int k = 0; k < countries.Length; k++) {
                Country country = countries[k];
                if (country.hidden)
                    continue;

                // Check if country is facing camera
                Vector3 center = transform.TransformPoint(country.localPosition);
                Vector3 dir = center - transform.position;
                float d = Vector3.Dot(cam.transform.forward, dir);
                if (d < -0.2f) {
                    // Check if center of country is inside viewport
                    Vector3 vpos = cam.WorldToViewportPoint(center);
                    float viewportMinX = cam.rect.xMin;
                    float viewportMaxX = cam.rect.xMax;
                    float viewportMinY = cam.rect.yMin;
                    float viewportMaxY = cam.rect.yMax;

                    if (vpos.x >= viewportMinX && vpos.x <= viewportMaxX && vpos.y >= viewportMinY && vpos.y <= viewportMaxY) {
                        vc.Add(country);
                    } else {
                        // Check if some frontier point is inside viewport
                        Vector3[] frontier = country.regions[country.mainRegionIndex].spherePoints;
                        int step = 1 + frontier.Length / 25;
                        for (int p = 0; p < frontier.Length; p += step) {
                            Vector3 pos = transform.TransformPoint(frontier[p]);
                            vpos = cam.WorldToViewportPoint(pos);
                            if (vpos.x >= viewportMinX && vpos.x <= viewportMaxX && vpos.y >= viewportMinY && vpos.y <= viewportMaxY) {
                                vc.Add(country);
                                break;
                            }
                        }
                    }
                }
            }
            return vc;
        }

        /// <summary>
        /// Returns a list of countries that are visible and overlaps the rectangle defined by two given sphere points
        /// </summary>
        public List<Country> GetVisibleCountries (Vector3 rectTopLeft, Vector3 rectBottomRight) {
            Vector2 latlon0, latlon1;
            latlon0 = Conversion.GetBillboardPosFromSpherePoint(rectTopLeft);
            latlon1 = Conversion.GetBillboardPosFromSpherePoint(rectBottomRight);
            Rect rect = new Rect(latlon0.x, latlon1.y, latlon1.x - latlon0.x, latlon0.y - latlon1.y);

            List<Country> selectedCountries = new List<Country>();
            for (int k = 0; k < countries.Length; k++) {
                Country country = countries[k];
                if (country.hidden)
                    continue;
                if (selectedCountries.Contains(country))
                    continue;

                // Check if any of country's regions is inside rect
                int crc = country.regions.Count;
                for (int cr = 0; cr < crc; cr++) {
                    Region region = country.regions[cr];
                    if (rect.Overlaps(region.rect2Dbillboard)) {
                        selectedCountries.Add(country);
                        break;
                    }
                }
            }
            return selectedCountries;
        }


        /// <summary>
        /// Makes countryIndex absorb another country providing any of its regions. All regions are transfered to target country.
        /// This function is quite slow with high definition frontiers.
        /// </summary>
        /// <param name="countryIndex">Country index of the conquering country.</param>
        /// <param name="sourceRegion">Source region of the loosing country.</param>
        public bool CountryTransferCountryRegion (int countryIndex, Region sourceCountryRegion, bool redraw = true) {
            int sourceCountryIndex = GetCountryIndex((Country)sourceCountryRegion.entity);
            if (countryIndex < 0 || sourceCountryIndex < 0 || countryIndex == sourceCountryIndex)
                return false;

            if (_provinces == null && !_showProvinces) {
                ReadProvincesGeoData(); // Forces loading of provinces
            }

            // Transfer all provinces records to target country
            Country sourceCountry = countries[sourceCountryIndex];
            Country targetCountry = countries[countryIndex];
            if (sourceCountry.provinces != null) {
                List<Province> destProvinces;
                if (targetCountry.provinces != null) {
                    destProvinces = new List<Province>(targetCountry.provinces);
                } else {
                    destProvinces = new List<Province>(sourceCountry.provinces.Length);
                }
                for (int k = 0; k < sourceCountry.provinces.Length; k++) {
                    Province province = sourceCountry.provinces[k];
                    province.countryIndex = countryIndex;
                    destProvinces.Add(province);
                }
                destProvinces.Sort(ProvinceSizeComparer);
                targetCountry.provinces = destProvinces.ToArray();
            }

            // Transfer cities
            int cityCount = cities.Count;
            for (int k = 0; k < cityCount; k++) {
                if (cities[k].countryIndex == sourceCountryIndex)
                    cities[k].countryIndex = countryIndex;
            }

            // Transfer mount points
            int mountPointCount = mountPoints.Count;
            for (int k = 0; k < mountPointCount; k++) {
                if (mountPoints[k].countryIndex == sourceCountryIndex)
                    mountPoints[k].countryIndex = countryIndex;
            }

            // Add main region of the source country to target if they are joint
            Region targetRegion = null;
            if (targetCountry.regions == null) {
                targetCountry.regions = new List<Region>();
            } else if (targetCountry.mainRegionIndex >= 0 && targetCountry.mainRegionIndex < targetCountry.regions.Count) {
                targetRegion = targetCountry.regions[targetCountry.mainRegionIndex];
            }

            // Add region to target country's polygon - only if the country is touching or crossing target country frontier
            if (targetRegion != null && sourceCountryRegion.Intersects(targetRegion)) {
                RegionMagnet(sourceCountryRegion, targetRegion);
                Clipper clipper = new Clipper();
                clipper.AddPaths(targetCountry.regions, PolyType.ptSubject);
                clipper.AddPath(sourceCountryRegion, PolyType.ptClip);
                clipper.Execute(ClipType.ctUnion, targetCountry);
            } else {
                // Add new region to country
                sourceCountryRegion.entity = targetCountry;
                sourceCountryRegion.regionIndex = targetCountry.regions.Count;
                targetCountry.regions.Add(sourceCountryRegion);
            }

            // Transfer additional regions
            if (sourceCountry.regions.Count > 1) {
                List<Region> targetRegions = new List<Region>(targetCountry.regions);
                for (int k = 0; k < sourceCountry.regions.Count; k++) {
                    Region otherRegion = sourceCountry.regions[k];
                    if (otherRegion != sourceCountryRegion) {
                        targetRegions.Add(sourceCountry.regions[k]);
                    }
                }
                targetCountry.regions = targetRegions;
            }

            // Fusion any adjacent regions that results from merge operation
            MergeAdjacentRegions(targetCountry);
            RegionSanitize(targetCountry.regions, false);

            // Finish operation
            internal_CountryDelete(sourceCountryIndex, false);
            if (countryIndex > sourceCountryIndex)
                countryIndex--;

            if (redraw) {
                RefreshCountryDefinition(countryIndex, null);
                Redraw();
            } else {
                RefreshCountryGeometry(targetCountry);
            }
            return true;
        }


        /// <summary>
        /// Treats source country as a province that joins target country
        /// </summary>
        /// <param name="targetCountryIndex"></param>
        /// <param name="sourceCountryIndex"></param>
        /// <returns></returns>
        public bool CountryTransferAsProvince (int targetCountryIndex, int sourceCountryIndex, bool redraw = true) {

            Country sourceCountry = countries[sourceCountryIndex];
            Country targetCountry = countries[targetCountryIndex];

            // Add source country regions to target country
            targetCountry.regions.AddRange(sourceCountry.regions);

            // Create new province
            string provinceName = sourceCountry.name;
            Province newProvince = new Province(sourceCountry.name, targetCountryIndex);
            newProvince.regions = sourceCountry.regions;
            newProvince.mainRegionIndex = sourceCountry.mainRegionIndex;
            newProvince.latlonCenter = sourceCountry.latlonCenter;

            // Add province
            ProvinceAdd(newProvince);

            // Move all cities to target country
            int cityCount = cities.Count;
            for (int k = 0; k < cityCount; k++) {
                if (cities[k].countryIndex == sourceCountryIndex) {
                    cities[k].countryIndex = targetCountryIndex;
                    cities[k].province = provinceName;
                }
            }

            // Move all mount points to target country
            int provinceIndex = GetProvinceIndex(newProvince);
            int mpCount = mountPoints.Count;
            for (int k = 0; k < mpCount; k++) {
                if (mountPoints[k].countryIndex == sourceCountryIndex) {
                    mountPoints[k].countryIndex = targetCountryIndex;
                    mountPoints[k].provinceIndex = provinceIndex;
                }
            }

            // Delete source country
            CountryDelete(sourceCountryIndex, true, false);

            // Update geometry bounds for target country
            if (redraw) {
                targetCountryIndex = GetCountryIndex(targetCountry);
                RefreshCountryDefinition(targetCountryIndex, null);
                Redraw();
            } else {
                RefreshCountryGeometry(targetCountry);
            }

            return true;

        }

        /// <summary>
        /// Changes province's owner to specified country and modifies frontiers/borders.
        /// Note: provinceRegion parameter usually is the province main region - although it does not matter since all regions will transfer as well. 
        /// </summary>
        public bool CountryTransferProvinceRegion (int targetCountryIndex, Region provinceRegion, bool redraw = true) {
            if (provinceRegion == null) return false;
            int provinceIndex = GetProvinceIndex((Province)provinceRegion.entity);
            if (provinceIndex < 0 || targetCountryIndex < 0 || targetCountryIndex >= countries.Length)
                return false;

            // Province must belong to another country
            Province province = provinces[provinceIndex];
            int sourceCountryIndex = province.countryIndex;
            if (sourceCountryIndex == targetCountryIndex)
                return false;

            // Remove province form source country
            Country sourceCountry = countries[sourceCountryIndex];
            if (sourceCountry.provinces != null) {
                List<Province> sourceProvinces = new List<Province>(sourceCountry.provinces);
                if (sourceProvinces.Contains(province)) {
                    sourceProvinces.Remove(province);
                    sourceCountry.provinces = sourceProvinces.ToArray();
                }
            }

            // Adds province to target country
            Country targetCountry = countries[targetCountryIndex];
            if (targetCountry.provinces != null) {
                List<Province> destProvinces = new List<Province>(targetCountry.provinces);
                destProvinces.Add(province);
                destProvinces.Sort(ProvinceSizeComparer);
                targetCountry.provinces = destProvinces.ToArray();
            } else {
                List<Province> destProvinces = new List<Province>();
                destProvinces.Add(province);
                targetCountry.provinces = destProvinces.ToArray();
            }

            // Extract province region from source country
            Clipper clipper = new Clipper();
            clipper.AddPaths(sourceCountry.regions, PolyType.ptSubject);
            clipper.AddPath(provinceRegion, PolyType.ptClip);
            clipper.Execute(ClipType.ctDifference, sourceCountry);

            // Remove invalid regions from source country
            if (sourceCountry.regions != null) {
                for (int k = 0; k < sourceCountry.regions.Count; k++) {
                    Region otherSourceRegion = sourceCountry.regions[k];
                    if (!otherSourceRegion.sanitized && otherSourceRegion.latlon.Length < 5) {
                        sourceCountry.regions.RemoveAt(k);
                        k--;
                    }
                }
            }

            // Adds province region to target country regions
            clipper = new Clipper();
            clipper.AddPaths(targetCountry.regions, PolyType.ptSubject);
            clipper.AddPath(provinceRegion, PolyType.ptClip);
            clipper.Execute(ClipType.ctUnion, targetCountry);

            // Fusion any adjacent regions that results from merge operation
            MergeAdjacentRegions(targetCountry);

            // Remove invalid regions from source country
            if (sourceCountry.regions != null) {
                for (int k = 0; k < sourceCountry.regions.Count; k++) {
                    Region otherSourceRegion = sourceCountry.regions[k];
                    if (!otherSourceRegion.sanitized && otherSourceRegion.latlon.Length < 5) {
                        sourceCountry.regions.RemoveAt(k);
                        k--;
                    }
                }
            }

            // Ensure all provinces have a land region at country level
            if (sourceCountry.provinces != null) {
                int crc = sourceCountry.regions.Count;
                for (int p = 0; p < sourceCountry.provinces.Length; p++) {
                    Province sourceProv = sourceCountry.provinces[p];
                    int sprc = sourceProv.regions.Count;
                    for (int pr = 0; pr < sprc; pr++) {
                        Region sourceProvRegion = sourceProv.regions[pr];
                        bool covered = false;
                        for (int cr = 0; cr < crc; cr++) {
                            Region sourceCountryRegion = sourceCountry.regions[cr];
                            if (sourceProvRegion.Intersects(sourceCountryRegion)) {
                                covered = true;
                                break;
                            }
                        }
                        if (!covered) {
                            Region newCountryRegion = new Region(sourceCountry, sourceCountry.regions.Count);
                            newCountryRegion.UpdatePointsAndRect(sourceProvRegion.latlon);
                            sourceCountry.regions.Add(newCountryRegion);
                        }
                    }
                }
            }

            // Update cities
            int cityCount = cities.Count;
            for (int k = 0; k < cityCount; k++) {
                City city = cities[k];
                if (city.countryIndex == sourceCountryIndex && city.province.Equals(province.name)) {
                    city.countryIndex = targetCountryIndex;
                }
            }

            // Update mount points
            int mountPointsCount = mountPoints.Count;
            for (int k = 0; k < mountPointsCount; k++) {
                MountPoint mp = mountPoints[k];
                if (mp.countryIndex == sourceCountryIndex && mp.provinceIndex == provinceIndex) {
                    mp.countryIndex = targetCountryIndex;
                }
            }

            // Update source country definition
            if (sourceCountry.regions.Count == 0) {
                internal_CountryDelete(sourceCountryIndex, false);
                if (targetCountryIndex > sourceCountryIndex)
                    targetCountryIndex--;
            } else {
                RegionSanitize(sourceCountry.regions, false);
                RefreshCountryGeometry(sourceCountry);
            }

            // Update target country definition
            RegionSanitize(targetCountry.regions, false);
            RefreshCountryDefinition(targetCountryIndex, null);
            province.countryIndex = targetCountryIndex;

            if (redraw) {
                Redraw();
            }

            return true;
        }


        /// <summary>
        /// Makes countryIndex absorb an hexagonal portion of the map. If that portion belong to another country, it will be substracted from that country as well.
        /// This function is quite slow with high definition frontiers.
        /// </summary>
        /// <param name="countryIndex">Country index of the conquering country.</param>
        /// <param name="cellIndex">Index of the cell to add to the country.</param>
        public bool CountryTransferCell (int countryIndex, int cellIndex, bool redraw = true) {
            if (countryIndex < 0 || cellIndex < 0 || cells == null || cellIndex >= cells.Length)
                return false;

            // Start process
            Country country = countries[countryIndex];
            Cell cell = cells[cellIndex];

            // Create a region for the cell
            Region sourceRegion = new Region(country, country.regions.Count);
            // Convert cell points to latlon coordinates
            sourceRegion.UpdatePointsAndRect(cell.latlon);

            // Transfer cities
            List<City> citiesInCell = GetCities(sourceRegion);
            int cityCount = citiesInCell.Count;
            for (int k = 0; k < cityCount; k++) {
                City city = citiesInCell[k];
                if (city.countryIndex != countryIndex) {
                    city.countryIndex = countryIndex;
                    int provinceIndex = GetProvinceIndex(city.localPosition);
                    if (provinceIndex >= 0) {
                        city.province = _provinces[provinceIndex].name;
                    } else {
                        city.province = ""; // clear province since it does not apply anymore
                    }
                }
            }

            // Transfer mount points
            List<MountPoint> mountPointsInCell = new List<MountPoint>();
            int mountPointCount = GetMountPoints(sourceRegion, mountPointsInCell);
            for (int k = 0; k < mountPointCount; k++) {
                MountPoint mp = mountPointsInCell[k];
                if (mp.countryIndex != countryIndex) {
                    mp.countryIndex = countryIndex;
                    mp.provinceIndex = -1;  // same as cities - province cleared in case it's informed since it does not apply anymore
                }
            }

            // Add region to target country's polygon - only if the country is touching or crossing target country frontier
            if (country.mainRegion != null) {
                Region targetRegion = country.mainRegion;
                RegionMagnet(sourceRegion, targetRegion);
                Clipper clipper = new Clipper();
                clipper.AddPath(sourceRegion, PolyType.ptClip);
                clipper.AddPaths(country.regions, PolyType.ptSubject);
                clipper.Execute(ClipType.ctUnion, country);

                // Finish operation with the country
                RegionSanitize(country.regions, true);
            } else {
                country.regions.Add(sourceRegion);
            }

            RefreshCountryGeometry(country);

            // Substract cell region from any other country
            {
                for (int k = 0; k < _countries.Length; k++) {
                    Country otherCountry = _countries[k];
                    if (otherCountry == country || !otherCountry.Overlaps(sourceRegion))
                        continue;
                    Clipper clipper = new Clipper();
                    clipper.AddPath(sourceRegion, PolyType.ptClip);
                    clipper.AddPaths(otherCountry.regions, PolyType.ptSubject);
                    clipper.Execute(ClipType.ctDifference, otherCountry);
                    RegionSanitize(otherCountry.regions, true);
                    if (otherCountry.regions.Count == 0) {
                        int otherCountryIndex = GetCountryIndex(otherCountry);
                        CountryDelete(otherCountryIndex, true, false);
                        if (k >= otherCountryIndex)
                            k--;
                    } else {
                        RefreshCountryGeometry(otherCountry);
                    }
                }
            }

            OptimizeFrontiers();

            if (redraw)
                Redraw();
            return true;
        }

        /// <summary>
        /// Sets the provinces for an existing country. The country regions will be updated to reflect the regions of the new provinces.
        /// </summary>
        /// <param name="mergeRegions">If true, adjacent regions will be merged (default).</param>
        /// <returns></returns>
        public bool CountrySetProvinces (int countryIndex, List<Province> provinces, bool mergeRegions = true, bool updateCities = true, bool updateMountPoints = true) {
            if (countryIndex < 0 || countryIndex >= countries.Length || provinces == null) return false;

            Country country = countries[countryIndex];
            country.regions.Clear();

            List<Province> newProvinces = new List<Province>();

            foreach (Province province in provinces) {
                if (province == null) continue;
                if (province.regions == null) {
                    ReadProvincePackedString(province);
                    if (province.regions == null) continue;
                }
                int r = 0;
                foreach (Region region in province.regions) {
                    if (region == null) continue;
                    Region newRegion = region.Clone();
                    newRegion.entity = country;
                    newRegion.regionIndex = r++;
                    country.regions.Add(newRegion);
                }
                newProvinces.Add(province);
            }
            country.provinces = newProvinces.ToArray();
            if (mergeRegions) {
                MergeAdjacentRegions(country);
            }

            int provincesCount = country.provinces.Length;

            if (updateCities) {
                int citiesCount = cities.Count;
                for (int p = 0; p < provincesCount; p++) {
                    Province province = country.provinces[p];
                    for (int k = 0; k < citiesCount; k++) {
                        City city = _cities[k];
                        if (city.countryIndex == province.countryIndex && city.province.Equals(province.name)) {
                            city.countryIndex = countryIndex;
                        }
                    }
                }
            }

            if (updateMountPoints) {
                int mpCount = mountPoints.Count;
                for (int p = 0; p < provincesCount; p++) {
                    Province province = country.provinces[p];
                    int provinceIndex = GetProvinceIndex(province);
                    for (int k = 0; k < mpCount; k++) {
                        MountPoint mp = mountPoints[k];
                        if (mp.countryIndex == province.countryIndex && mp.provinceIndex == provinceIndex) {
                            mp.countryIndex = countryIndex;
                        }
                    }
                }
            }

            // Update provinces country index
            for (int p = 0; p < provincesCount; p++) {
                Province province = country.provinces[p];
                province.countryIndex = countryIndex;
            }

            lastCountryLookupCount = -1;
            RefreshCountryGeometry(country);
            return true;
        }


        /// <summary>
        /// Add provinces to an existing country. The country regions will be updated to reflect the regions of the new provinces.
        /// Please note that if the provinces currently belong to another country, that other country won't be updated. Call CountrySetProvinces or CountryRemoveProvinces on the other country as well.
        /// </summary>
        /// <param name="mergeRegions">If true, adjacent regions will be merged (default).</param>
        /// <returns></returns>
        public bool CountryAddProvinces (int countryIndex, List<Province> provinces, bool mergeRegions = true, bool updateCities = true, bool updateMountPoints = true) {
            if (countryIndex < 0 || countryIndex >= countries.Length || provinces == null) return false;

            Country country = countries[countryIndex];

            List<Province> newProvinces = new List<Province>();

            if (country.provinces != null) {
                newProvinces.AddRange(country.provinces);
            }

            // ensure there's at least one new province
            foreach (Province province in provinces) {
                if (province == null) continue;
                bool alreadyExists = false;
                if (country.provinces != null) {
                    foreach (Province existingProvince in country.provinces) {
                        if (existingProvince == province) {
                            alreadyExists = true;
                            break;
                        }
                    }
                }
                if (alreadyExists) continue;
                if (province.regions == null) {
                    ReadProvincePackedString(province);
                    if (province.regions == null) continue;
                }
                int r = 0;
                foreach (Region region in province.regions) {
                    if (region == null) continue;
                    Region newRegion = region.Clone();
                    newRegion.entity = country;
                    newRegion.regionIndex = r++;
                    country.regions.Add(newRegion);
                }
                newProvinces.Add(province);

                if (updateCities) {
                    int citiesCount = cities.Count;
                    for (int k = 0; k < citiesCount; k++) {
                        City city = _cities[k];
                        if (city.countryIndex == province.countryIndex && city.province.Equals(province.name)) {
                            city.countryIndex = countryIndex;
                        }
                    }
                }

                if (updateMountPoints) {
                    int mpCount = mountPoints.Count;
                    int provinceIndex = GetProvinceIndex(province);
                    for (int k = 0; k < mpCount; k++) {
                        MountPoint mp = mountPoints[k];
                        if (mp.countryIndex == province.countryIndex && mp.provinceIndex == provinceIndex) {
                            mp.countryIndex = countryIndex;
                        }
                    }
                }
            }
            country.provinces = newProvinces.ToArray();
            if (mergeRegions) {
                MergeAdjacentRegions(country);
            }

            // Update provinces country index
            for (int p = 0; p < country.provinces.Length; p++) {
                Province province = country.provinces[p];
                province.countryIndex = countryIndex;
            }

            lastCountryLookupCount = -1;
            RefreshCountryGeometry(country);
            return true;
        }





        /// <summary>
        /// Remove provinces from an existing country. The country regions will be updated to reflect the regions of the new provinces.
        /// </summary>
        /// <param name="mergeRegions">If true, adjacent regions will be merged (default).</param>
        /// <returns></returns>
        public bool CountryRemoveProvinces (int countryIndex, List<Province> provinces, bool mergeRegions = true) {
            if (countryIndex < 0 || countryIndex >= countries.Length || provinces == null) return false;

            Country country = countries[countryIndex];
            List<Province> newProvinces = new List<Province>();
            foreach (Province province in country.provinces) {
                if (province == null || provinces.Contains(province)) continue;
                newProvinces.Add(province);
            }

            return CountrySetProvinces(countryIndex, newProvinces, mergeRegions, false);
        }



        #endregion

    }

}