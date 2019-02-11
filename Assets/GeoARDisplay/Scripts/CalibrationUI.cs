using System.Collections;
using System.Collections.Generic;
using System.Xml;
using GoFindScripts;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationUI : MonoBehaviour {
    private Text compassDisplay;
    private Text additionalDisplay;
    private CalibrationUtility controller;
    public string CalibrationUtilityGameObjectName = "ARMapper";
    private bool ShowAdditionalDisplay = true;
    public bool AdditionalDisplayEnalbed = false;


    // Use this for initialization
    void Start() {
        compassDisplay = GameObject.Find("CompassDisplay").GetComponent<Text>();
        additionalDisplay = GameObject.Find("AdditionalDisplay").GetComponent<Text>();
        controller = GameObject.Find(CalibrationUtilityGameObjectName).GetComponent<CalibrationUtility>();
        Button btn = GameObject.Find("Calibrate").GetComponent<Button>();
        btn.onClick.AddListener(OnCalibrate);
        if (!AdditionalDisplayEnalbed) {
            ToggleAdditionalDisplay();
        }
    }

    public void ToggleAdditionalDisplay() {
        ShowAdditionalDisplay = !ShowAdditionalDisplay;
        additionalDisplay.gameObject.active = ShowAdditionalDisplay;
    }

    public void SetOrientation(double orientation) {
        if (compassDisplay != null) {
            compassDisplay.text = string.Format("{0:N3}°", orientation);
        }
    }

    public void SetAdditionalText(string msg) {
        additionalDisplay.text = msg;
    }

    public void OnCalibrate() {
        controller.Calibrate();
    }
}