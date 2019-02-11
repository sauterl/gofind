using UnityEngine;

namespace DefaultNamespace.GoFindScripts {
    public class GeoPositioned : MonoBehaviour {

        public GeoArithmetic.GeoCoordinates GeoCoordinates { get; set; }

        private float _bearing = float.NaN;
        
        public float Bearing
        {
            get { return _bearing;}
            set { _bearing = value; }
        }

        public bool IsBearingSet() {
            return !float.IsNaN(_bearing);
        }
        
    }
}