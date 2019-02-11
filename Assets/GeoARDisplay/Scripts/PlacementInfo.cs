using UnityEngine;
using UnityEngine.UI;

namespace GeoARDisplay.Scripts {
    public class PlacementInfo : MonoBehaviour {

        public Text XValue;
        public Text ZValue;
        public Text OValue;

        void Start() {
            XValue.text = "x=";
            ZValue.text = "z=";
            OValue.text = "o=";
        }
        
        public void SetPlacementInfo(float x, float z, float o) {
            if (XValue != null) {
                XValue.text = "x="+x.ToString();
            }
            if (ZValue != null) {
                ZValue.text = "z="+z.ToString();
            }
            if (OValue != null) {
                OValue.text = "o="+o.ToString();
            }
        }

    }
}