using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ImagePresenter : MonoBehaviour
    {
        public RawImage imageDisplay;

        public LoadIndicator indicator;

        private string url;

        public void SetImageUrl(string url)
        {
            this.url = url;
        }

        void Awake()
        {
            texture = new Texture2D(100, 100, TextureFormat.DXT1, false);
        }

        private Texture2D texture;

        private IEnumerator LoadImageFromWeb()
        {
            WWW www = new WWW(url);
            yield return www;

            www.LoadImageIntoTexture(texture);
            imageDisplay.texture = texture;

            SwitchIndicator(false);

            imageDisplay.SizeToParent();
        }

        public void LoadImage(string url)
        {
            SetImageUrl(url);
            
            SwitchIndicator(true);

            StartCoroutine(LoadImageFromWeb());
        }

        private void SwitchIndicator(bool on) {
            if (indicator != null) {
                indicator.gameObject.SetActive(on);
            indicator.Indicate(on);
            }
            
        }
    }
}