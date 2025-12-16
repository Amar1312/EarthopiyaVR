using UnityEngine;

namespace WPM {

    [DefaultExecutionOrder(100)]
    [ExecuteAlways]
    public class SyncGlobalPosition : MonoBehaviour {

        WorldMapGlobe map;

        private void OnEnable() {
            map = GetComponent<WorldMapGlobe>();
        }
        private void LateUpdate() {
            map.SetGlobalShaderData();
        }

    }

}