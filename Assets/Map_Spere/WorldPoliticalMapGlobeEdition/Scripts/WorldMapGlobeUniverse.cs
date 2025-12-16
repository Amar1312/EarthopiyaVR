// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using System;
using UnityEngine;

namespace WPM {

	public enum SKYBOX_STYLE {
		UserDefined = 0,
		Basic = 1,
		MilkyWay = 2,
		DualSkybox = 3
	}

	/* Public WPM Class */
	public partial class WorldMapGlobe : MonoBehaviour {

		[SerializeField]
		Vector3 _earthScenicLightDirection = new Vector4(-0.5f, 0.5f, -1f);

		public Vector3 earthScenicLightDirection {
			get { return _earthScenicLightDirection; }
			set {
				if (value != _earthScenicLightDirection) {
					_earthScenicLightDirection = value;
					isDirty = true;
					DrawAtmosphere();
				}
			}
		}

		[SerializeField]
		Transform _sun;

		public Transform sun {
			get { return _sun; }
			set {
				if (value != _sun) {
					_sun = value;
					isDirty = true;
					RestyleEarth();
				}
			}
		}


		[SerializeField]
		bool _showMoon;

		public bool showMoon {
			get { return _showMoon; }
			set {
				if (_showMoon != value) {
					_showMoon = value;
					isDirty = true;
					UpdateMoon();
				}
			}
		}


		[SerializeField]
		bool _moonAutoScale = true;

		public bool moonAutoScale {
			get { return _moonAutoScale; }
			set {
				if (_moonAutoScale != value) {
					_moonAutoScale = value;
					isDirty = true;
					UpdateMoon();
				}
			}
		}


		[SerializeField]
		SKYBOX_STYLE _skyboxStyle = SKYBOX_STYLE.UserDefined;

		public SKYBOX_STYLE skyboxStyle {
			get { return _skyboxStyle; }
			set {
				if (_skyboxStyle != value) {
					_skyboxStyle = value;
					isDirty = true;
					UpdateSkybox();
				}
			}
		}

		[SerializeField]
		float _skyboxEnvironmentTransitionAltitudeMin = 1000;

		public float skyboxEnvironmentTransitionAltitudeMin {
			get { return _skyboxEnvironmentTransitionAltitudeMin; }
			set {
				if (_skyboxEnvironmentTransitionAltitudeMin != value) {
					_skyboxEnvironmentTransitionAltitudeMin = value;
					isDirty = true;
				}
			}
		}


		[SerializeField]
		float _skyboxEnvironmentTransitionAltitudeMax = 1100;

		public float skyboxEnvironmentTransitionAltitudeMax {
			get { return _skyboxEnvironmentTransitionAltitudeMax; }
			set {
				if (_skyboxEnvironmentTransitionAltitudeMax != value) {
					_skyboxEnvironmentTransitionAltitudeMax = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		Texture2D _skyboxEnvironmentTextureHDR;

		public Texture2D skyboxEnvironmentTextureHDR {
			get { return _skyboxEnvironmentTextureHDR; }
			set {
				if (_skyboxEnvironmentTextureHDR != value) {
					_skyboxEnvironmentTextureHDR = value;
					UpdateSkybox();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool _syncTimeOfDay = false;

		public bool syncTimeOfDay {
			get { return _syncTimeOfDay; }
			set {
				if (_syncTimeOfDay != value) {
					_syncTimeOfDay = value;
					if (_syncTimeOfDay) {
						if (!_earthStyle.isScatter() && !_earthStyle.isScenic()) {
							earthStyle = EARTH_STYLE.NaturalHighResScenicScatterCityLights;
						}
					} else {
						TiltGlobe();
					}
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Sets solar rotation and adjust Earth rotation as well to match a given date
		/// </summary>
		/// <param name="date"></param>
		public void SetTimeOfDay(DateTime date) {
			DateTime Now = date.ToUniversalTime();                // Get unlocalised time
			float SolarDeclination = -23.45f * Mathf.Cos((360f / 365f) * (Now.DayOfYear + 10) * Mathf.Deg2Rad);
			float sunRot = ((Now.Hour * 60f) + Now.Minute + (Now.Second / 60f)) / 4f;     // Convert time into minutes, then scale to a 0-360 range value
			Vector3 sunRotation = new Vector3(SolarDeclination, sunRot, 0);        // Combine the axis and calculated sun angle into a vector
			_sun.transform.localRotation = Quaternion.Euler(sunRotation);
			globeTransform.rotation = Misc.QuaternionZero;
			_navigationMode = NAVIGATION_MODE.CAMERA_ROTATES;
			_autoRotationSpeed = 0;
		}

		/// <summary>
		/// Simpler version of SetTimeOfDay which just rotates Earth according to a 24h value
		/// </summary>
		/// <param name="time24h"></param>
		public void SetTimeOfDaySimple(float time24h) {
			globeTransform.localRotation = Quaternion.Euler(23.45f, 360f * time24h / 24f, 0);
		}


	}

}

