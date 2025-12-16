using UnityEngine;
using System.Collections;

namespace WPM {
    public class SurfaceBlinker : MonoBehaviour {

        public float duration;
        public Color color1, color2;
        public float speed;
        public Material blinkMaterial;
        public Region customizableSurface;
        public bool smoothBlink;

        Material oldMaterial;
        float startTime, lapTime;
        bool whichColor;
        Renderer r;

        void Start() {
            r = GetComponent<Renderer>();
            oldMaterial = r.sharedMaterial;
            GenerateMaterial();
            startTime = Time.time;
            lapTime = startTime - speed;
        }

        private void OnDestroy() {
            if (blinkMaterial != null) {
                DestroyImmediate(blinkMaterial);
            }
            RestoreMaterial();
        }

        // Update is called once per frame
        void Update() {
            float elapsed = Time.time - startTime;
            if (elapsed > duration) {
                // Hide surface?
                if (customizableSurface.customMaterial == null) {
                    gameObject.SetActive(false);
                }
                Destroy(this);
                return;
            }
            if (smoothBlink) {
                Material mat = r.sharedMaterial;
                if (mat != blinkMaterial)
                    GenerateMaterial();

                float t = Mathf.PingPong(Time.time * speed, 1f);
                blinkMaterial.color = Color.Lerp(color1, color2, t);

            } else if (Time.time - lapTime > speed) {
                lapTime = Time.time;
                Material mat = r.sharedMaterial;
                if (mat != blinkMaterial)
                    GenerateMaterial();
                whichColor = !whichColor;
                if (whichColor) {
                    blinkMaterial.color = color1;
                } else {
                    blinkMaterial.color = color2;
                }
            }
        }

        void RestoreMaterial() {
            if (customizableSurface == null) return;
            Material goodMat;
            if (customizableSurface.customMaterial != null) {
                goodMat = customizableSurface.customMaterial;
            } else {
                goodMat = oldMaterial;
            }
            r.sharedMaterial = goodMat;
        }

        void GenerateMaterial() {
            blinkMaterial = Instantiate(blinkMaterial);
            r.sharedMaterial = blinkMaterial;
        }
    }

}