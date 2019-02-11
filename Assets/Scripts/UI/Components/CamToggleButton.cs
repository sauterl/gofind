using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamToggleButton : MonoBehaviour
{

    public DeviceCam deviceCam;

    private Button btn; // Unused

    private bool toggled = false;

    public Image camOnImage;
    public Image camOffImage;

    void Awake()
    {
        try
        {
            deviceCam = transform.parent.Find("CamDisplay").GetComponent<DeviceCam>();
        }
        catch (NullReferenceException e)
        {
            // Silently ignoring exception -> camnot found
            Debug.Log("Cam not found");
        }
        btn = GetComponent<Button>(); 

    }

    public void PressBtn()
    {
        Debug.Log("CamToggle: "+toggled);
        if (toggled)
        {
            deviceCam.StopCam();
        }
        else
        {
            deviceCam.StartCam();
        }
        toggled = !toggled;
        SetCamImage(toggled);
    }

    private void SetCamImage(bool on)
    {
        camOffImage.gameObject.SetActive(on);
        camOnImage.gameObject.SetActive(!on);
    }
    

}
