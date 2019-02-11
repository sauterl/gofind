using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaxDistanceLabel : MonoBehaviour {

    private float maxDistance = 3;
    private bool modified = false;

    private Text label;

    private void Awake() {
        label = GetComponent<Text>();
    }

    private void Update() {
        label.text = string.Format("{0} m", UIManager.Round(maxDistance*1000, 0));
    }

    public void SetMaxDistance(float dist) {
        maxDistance = dist;
        modified = true;
    }

    public float GetMaxDistance() {
        return maxDistance;
    }

    public bool IsModified() {
        return modified;
    }
}
