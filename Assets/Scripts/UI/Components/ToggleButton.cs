using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour {


    public bool defaultValue = true;

    private bool value;

    public Image toggledImage;
    public Image untoggledImage;

    private Action<bool> toggleHandler = null;

    public void SetToggleHandler(Action<bool> handler) {
        toggleHandler = handler;
    }

    public void ResetToggleHandler() {
        toggleHandler = null;
    }

    public void Toggle() {
        Toggle(!value);
    }

    public void ResetToDefault() {
        Toggle(defaultValue, true);
    }

    

    private void Toggle(bool state, bool silent = false) {
        value = state;
        toggledImage.gameObject.SetActive(value);
        untoggledImage.gameObject.SetActive(!value);
        if (toggleHandler != null && !silent ) {
            toggleHandler.Invoke(value);
        }
    }
        
	// Use this for initialization
	void Start () {
		ResetToDefault();
	}
}
