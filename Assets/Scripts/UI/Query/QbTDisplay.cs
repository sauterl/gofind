using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Query {
    public class QbTDisplay : MonoBehaviour {

        /*
         * One day this might be replaced with a sophisticated datetime input ui
         */
        
        public InputField lowerBoundInput;
        public InputField upperBoundInput;

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
            builder.RemoveQbT();
        }
    }
}