using Assets.GoFindMap.Scripts;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Query {
    public class QbLDisplay : MonoBehaviour {

        public ImagePresenter image;

        public Text text;

        public Text title;
        
        public QueryBuilder builder;


        private void Start() {
            Transform t = transform.Find("RemoveButton");
            if (t != null) {
                if (t.GetComponent<Button>() != null) {
                    t.GetComponent<Button>().onClick.AddListener(call: HandleRemovement);
                }
            }
        }

        private void HandleRemovement() {
            builder.RemoveQbL();
        }

        public void DisplayLocation(double lat, double lon) {
            string url = MapController.AddCredentialsToUrl(MapController.BuildBaseUrl(lat, lon,
                new[] {MapController.CreateTinyMarkerAt(lat, lon)}));
            image.LoadImage(url);
            text.text = string.Format("Latitude: {0}, Longitude: {1}", lat, lon);
        }


    }
}