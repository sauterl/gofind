using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadIndicator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Indicate(true);
	}

    public float rotationSpeed = 1f;

    private bool show = false;

    public void Indicate(bool show) {
        this.show = show;
    }
	
	// Update is called once per frame
	void Update () {
	    if (show) {
	        gameObject.transform.Rotate(Vector3.back, rotationSpeed);
        }
	}
}
