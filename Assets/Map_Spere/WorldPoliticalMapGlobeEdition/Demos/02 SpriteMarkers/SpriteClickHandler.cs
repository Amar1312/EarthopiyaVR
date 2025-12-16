using UnityEngine;

namespace WPM {

	public class SpriteClickHandler : MonoBehaviour {

		public WorldMapGlobe map;

		void Start () {
			if (GetComponent<Collider>() == null) {
				BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
				boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, 0.01f);
			}
			if (map == null) {
				map = WorldMapGlobe.instance;
			}
		}

		void OnMouseDown () {
			Debug.Log ("Mouse down on sprite!");
		}

		void OnMouseUp () {
			Debug.Log ("Mouse up on sprite!");

			int countryIndex = map.countryLastClicked;
			if (countryIndex >= 0) {
				Debug.Log ("Clicked on " + map.countries [countryIndex].name);
			}
		}

		void OnMouseEnter () {
			Debug.Log ("Mouse over the sprite!");
		}

		void OnMouseExit () {
			Debug.Log ("Mouse exited the sprite!");
		}

	}
}
