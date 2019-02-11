using UnityEngine;
using UnityEngine.UI;

namespace GeoARDisplay.Scripts {
    public class ARMapperDebugUI : MonoBehaviour {


        public ARMapper ArMapper;
        public PlacementInfo PlacementInfo;
        public PlacementModifier PlacementModifier;

        public Slider rotationSlider;

        private void Start() {
            rotationSlider.minValue = 0;
            rotationSlider.maxValue = 10;
            rotationSlider.onValueChanged.AddListener(value => {
                if (ArTransform != null) {
                    ArTransform.Rotate(Vector3.up, value, Space.Self);
                }
            });
        }

        public Transform ArTransform;        

        public void SetPlacement() {
            foreach (var posed in ArMapper.GetPositioned()) {
                var pos = posed.transform.position;
                if (!float.IsNaN(PlacementModifier.GetXValue())) {
                    pos.x = PlacementModifier.GetXValue();
                }

                if (!float.IsNaN(PlacementModifier.GetZValue())) {
                    pos.z = PlacementModifier.GetZValue();
                }
                
                posed.transform.position = pos;

                if (!float.IsNaN(PlacementModifier.GetOValue())) {
                    posed.transform.Rotate(Vector3.up, 0, Space.Self);
                                    
                                    posed.transform.Rotate(Vector3.up, PlacementModifier.GetOValue(), Space.Self);
                }
                
                
                PlacementInfo.SetPlacementInfo(posed.transform.position.x, posed.transform.position.z, posed.transform.rotation.eulerAngles.y);
            } 
        }
    }
}