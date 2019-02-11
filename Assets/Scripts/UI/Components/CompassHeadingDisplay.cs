using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassHeadingDisplay : MonoBehaviour {
    private const int UPDATE_INTERVAL = 30;

    private Text textObject;

    public int targetHeading = -1;

    void Awake()
    {
        textObject = GetComponent<Text>();
    }

	// Use this for initialization
	void Start ()
	{
	    Input.compass.enabled = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (Time.frameCount % UPDATE_INTERVAL == 0) {
	        textObject.text = "" + RoundOff(Input.compass.magneticHeading);
	        if (targetHeading != -1)
	        {
	            textObject.text = string.Format("{0} ({1})", RoundOff(Input.compass.magneticHeading, targetHeading));
	        }
        }

	    

	}

    public static int RoundOff(double d, int accuracy = 10) {
        return ((int) (d / (double)accuracy) * accuracy);
    }
}
