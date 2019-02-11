using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Script witch combines the cam of a device and its gyroscope.
 */
public class DeviceCam : MonoBehaviour
{

    private WebCamTexture cam;

    public RawImage image;
    public AspectRatioFitter fitter; // Required: Mode Envelop Parent, Ratio: 1.501182

    private bool active = true;

	// Use this for initialization
	void Start () {
	    if (WebCamTexture.devices.Length == 0)
	    {
	        active = false;
            Debug.Log("Could not find a cam");
	        return;
	    }

        cam = new WebCamTexture(
            (int)0.8f*Screen.width, (int)0.8f*Screen.height, 30);//desired width, height and fps

	    if (cam == null)
	    {
	        Debug.Log("Cam could not be initialized");
	        active = false;
	    }

        //cam.Play(); //Dont start until externally called!
	    image.texture = cam;
	}

    public void StartCam()
    {
        if (cam != null && !cam.isPlaying)
        {
            cam.Play();
        }
    }

    public void StopCam()
    {
        if (cam != null && cam.isPlaying)
        {
            cam.Stop();
        }
    }
	
	// Update is called once per frame
	void Update () {
	    if (!active)
	    {
            return;
	    }

	    float ratio = (float) cam.width / (float) cam.height;

	    fitter.aspectRatio = ratio;

	    float scaleY = cam.videoVerticallyMirrored ? -1f : 1f;
        image.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

	    int orientation = -cam.videoRotationAngle;

        image.rectTransform.localEulerAngles = new Vector3(0,0,orientation);
	}

    void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            StopCam();
        }
    }

    void OnApplicationQuit()
    {
        StopCam();
    }

	
	public WebCamTexture Cam
	{
		get { return cam; }
	}
}
