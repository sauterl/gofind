using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Query {
    public class QbEDisplay :MonoBehaviour {

        public RawImage image;

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
            builder.RemoveQbE();
        }
        
        public void Display(Texture2D texture) {
            if (image != null) {
                image.SizeToParent(0.5f); // Experimental
                image.texture = texture;
            }
        }

    }
}