using System.Collections;
using UnityEngine;

// TODO Remove failure cases with try-to-add-renderer and replace with log warning and stop
namespace GeoARDisplay.Scripts {
    public class WebTextured : MonoBehaviour {
        public string url;

        IEnumerator LoadImage() {
            if (!string.IsNullOrEmpty(url)) {
                            Texture2D tex;
                            tex = new Texture2D(4, 4, TextureFormat.RGBA32, false); // Former TextureFormat: DXT1
                            using (WWW www =
                                new WWW(
                                    url)
                            ) {
                                yield return www;
                                www.LoadImageIntoTexture(tex);
                                if (GetComponent<Renderer>() != null) {
                                    ApplyTexture(tex, GetComponent<Renderer>());
                                } else {
                                    Debug.Log("Second attempt; creating renderer");
                                    gameObject.AddComponent<MeshRenderer>();
                                    ApplyTexture(tex, GetComponent<Renderer>());
                                }
                                
                            }
                        }
        }

        IEnumerator Start() {
            return LoadImage();
        }

        public void LoadImageFromWeb() {
            StartCoroutine(LoadImage());
        }

        private void ApplyTexture(Texture2D tex, Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("Cannot apply texture if renderer is null");
            }
            if (renderer.material == null) {
                Debug.LogWarning("Cannot apply texture if renderer's material is null");
            } else {
                renderer.material.mainTexture = tex;
            }
        }
    }
}