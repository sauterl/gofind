using System;
using GoogleARCore;
using UnityEngine;

namespace GoFindScripts {
    public class CalibrationUtility : MonoBehaviour {
        private CalibrationUI ui;


        private Camera cam;
        
        private bool calibrated = false;

        private float north = float.NaN;
        
        private int frameRate = 10;

        private void Start() {
            Init();
        }

        public void Init() {
            try {
                ui = GameObject.Find("CalibrationUI").GetComponent<CalibrationUI>();
                compassEnabled = EnableCompass();
                cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            } catch (NullReferenceException e) {
                Debug.Log(e);
                Debug.Log("Init failed, will have to be init again, when used");
            }
            
        }

        private bool compassEnabled;
        private Compass compass;

        private bool active = true;
        
        private bool EnableCompass() {
            compass = Input.compass;
            compass.enabled = true;

            Input.location.Start();

            return true;
        }

        public void ShowUI(bool show) {
            ui.gameObject.active = show;
            active = show;
        }

        // Update is called once per frame
        void Update() {
            if (active && Time.frameCount % frameRate == 0) {
                if (compassEnabled) {
                    ui.SetOrientation(compass.trueHeading);
                }
                /*
                var rot2 = Frame.Pose.rotation.eulerAngles;
                var rot = cam.transform.rotation.eulerAngles;
                var str1 = string.Format("{0:N3},{1:N3},{2:N3}", rot.x, rot.y, rot.z);
                var str2 = string.Format("{0:N3},{1:N3},{2:N3}", rot2.x, rot2.y, rot2.z);
                
                ui.SetAdditionalText(string.Format("c: {0}\np: {1}",str1,str2));*/
                
            }
        }

        public float GetNorthOffset() {
            return north;
        }

        public bool IsCalibrated() {
            return !float.IsNaN(north);
        }
        
        public void Calibrate() {
            try {
                north = Frame.Pose.rotation.eulerAngles.z;
                ui.SetAdditionalText("Calibrate!"); //DEBUG
                ShowUI(false);
                CalibrationDone();
            } catch (NullReferenceException ex) {
                Debug.LogWarning("Pose was null, probably not tracking. Remaining in calibration ui");
            }
            
            
        }

        private Action onCalibrationDone = null;
        
        public void OnCalibrationDone(Action calibrationFinished) {
            onCalibrationDone = calibrationFinished;
        }

        private void CalibrationDone() {
            if (onCalibrationDone != null) {
                onCalibrationDone.Invoke();
            }
        }
    }
}