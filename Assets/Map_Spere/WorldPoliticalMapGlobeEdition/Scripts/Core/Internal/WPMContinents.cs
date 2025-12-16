// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WPM {
    public partial class WorldMapGlobe : MonoBehaviour {

        void HideContinentHighlight() {
            // Raise exit event
            if (OnContinentExit != null && !string.IsNullOrEmpty(_continentHighlighted)) {
                OnContinentExit(_continentHighlighted);

            }
            _continentHighlighted = null;
        }
    }

}