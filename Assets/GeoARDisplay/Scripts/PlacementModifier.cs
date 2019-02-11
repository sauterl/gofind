using System;
using UnityEngine;
using UnityEngine.UI;

namespace GeoARDisplay.Scripts {
    public class PlacementModifier : MonoBehaviour {

        public InputField XInput;
        public InputField ZInput;
        public InputField OInput;

        public float GetXValue() {
            if (XInput != null) {
                try {
                    return float.Parse(XInput.text);
                } catch (FormatException e) {
                    // will return NaN
                }
                
            }

            return float.NaN;
        }
        
        public float GetZValue() {
            if (ZInput != null) {
                try {
                    return float.Parse(ZInput.text);
                } catch (FormatException e) {
                    
                }
                
            }

            return float.NaN;
        }
        
        public float GetOValue() {
            if (OInput != null) {

                try {
                    return float.Parse(OInput.text);
                } catch (FormatException e) {
                    
                }
                
            }

            return float.NaN;
        }

    }
}