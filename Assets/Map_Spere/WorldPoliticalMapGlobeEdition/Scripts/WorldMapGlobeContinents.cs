// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace WPM {


    /* Event definitions */
    public delegate void ContinentEvent(string continent);
    public delegate void ContinentClickEvent(string continent, int buttonIndex);

    /* Public WPM Class */
    public partial class WorldMapGlobe : MonoBehaviour {

        public event ContinentEvent OnContinentEnter;
        public event ContinentEvent OnContinentExit;
        public event ContinentClickEvent OnContinentPointerDown;
        public event ContinentClickEvent OnContinentPointerUp;
        public event ContinentClickEvent OnContinentClick;


        string _continentHighlighted;

        /// <summary>
        /// Returns Continent under mouse position or null if none.
        /// </summary>
        public string continentHighlighted { get { return _continentHighlighted; } }


        string _continentLastClicked;

        /// <summary>
        /// Returns the last clicked continent.
        /// </summary>
        public string continentLastClicked { get { return _continentLastClicked; } }


        /// <summary>
        /// Iterates for the countries list and colorizes those belonging to specified continent name.
        /// </summary>
        public void ToggleContinentSurface(string continentName, bool visible, Color color) {
            for (int colorizeIndex = 0; colorizeIndex < countries.Length; colorizeIndex++) {
                if (string.Equals(countries[colorizeIndex].continent, continentName)) {
                    ToggleCountrySurface(countries[colorizeIndex].name, visible, color);
                }

            }
        }

        /// <summary>
        /// Uncolorize/hide specified countries beloning to a continent.
        /// </summary>
        public void HideContinentSurface(string continentName) {
            for (int colorizeIndex = 0; colorizeIndex < countries.Length; colorizeIndex++) {
                if (string.Equals(countries[colorizeIndex].continent, continentName)) {
                    HideCountrySurface(colorizeIndex);
                }
            }
        }


    }

}