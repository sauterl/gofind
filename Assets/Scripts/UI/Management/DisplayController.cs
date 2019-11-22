using System;
using System.Collections;
using Assets.Scripts.Core;
using Assets.Scripts.UI;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RawImage bodyImage;
    public string footerStr;
    private Text footerText;
    public string imageURL;

    private ScreenOrientation lastOrientation;

    private Texture2D texture;
    private ImagePresenter imgPresenter;

    public string titleStr;

    private Text titleText;

    public UIManager UiManager { get; set; }
    public MultimediaObject Mmo { get; set; }
    public Controller Controller { get; set; }

    private void Awake()
    {
        titleText = transform.Find("ContentPanel/HeaderPanel/HeaderText").GetComponent<Text>();
        footerText = transform.Find("ContentPanel/FooterPanel/FooterText").GetComponent<Text>();
        bodyImage = transform.Find("ContentPanel/BodyPanel/BodyImage").GetComponent<RawImage>();
        imgPresenter = gameObject.AddComponent<ImagePresenter>();
        imgPresenter.imageDisplay = bodyImage;
        // TODO No loading indicator for imgPresenter
        lastOrientation = Screen.orientation;
    }

    // Use this for initialization
    private void Start()
    {
        titleText.text = titleStr;
        footerText.text = footerStr;

        if (!string.IsNullOrEmpty(imageURL))
        {
            imgPresenter.LoadImage(imageURL);
        }
    }
    
    public void LoadImageFromWeb(string url)
    {
        imageURL = url;
        imgPresenter.LoadImage(imageURL);
    }

    public void SetTitle(string title)
    {
        titleText.text = title;
        titleStr = title;
    }

    public void SetFooter(string footer)
    {
        footerText.text = footer;
        footerStr = footer;
    }

    private float firstDown=float.NaN;
    private float firstUp = float.NaN;
    private float secondDown= float.NaN;
    private float secondUp = float.NaN;

    public void OnPointerDown(PointerEventData eventData) {
        DumpPointerInfo();
        if (float.IsNaN(firstDown)) {
            firstDown = Time.frameCount;
            Debug.Log("Stored fd="+firstDown);
        }
        if (!float.IsNaN(firstUp) && float.IsNaN(secondDown)) {
            secondDown = Time.frameCount;
            Debug.Log("Stored SD="+secondDown);
        }

        

    }

    public void OnPointerUp(PointerEventData eventData) {
        DumpPointerInfo();
        if (!float.IsNaN(firstDown) && float.IsNaN(firstUp)) {
            firstUp = Time.frameCount;
            Debug.Log("Stored fu="+firstUp);
        }

        if (!float.IsNaN(secondDown) && float.IsNaN(secondUp)) {
            secondUp = Time.frameCount;
            Debug.Log("Stored su="+secondUp);
            DumpPointerInfo();
            if (ClickTimes(firstDown, firstUp) && ClickTimes(secondDown, secondUp) && ClickTimes(firstUp, secondDown,25)) {
                Debug.Log("Click times are short. double tapped");
                //UiManager.Present(Mmo);
                Controller.Display(Mmo);
            }
            ResetPointerInfo();
        }

    }

    private bool ClickTimes(float down, float up, float maxDelay = 15) {
        return Math.Abs(down - up) <= maxDelay;
    }

    private void ResetPointerInfo() {
        Debug.Log("resetting pointer info");
        firstDown = float.NaN;
        firstUp = float.NaN;
        secondDown = float.NaN;
        secondUp = float.NaN;
    }

    private void DumpPointerInfo() {
        Debug.LogFormat("Disp{4} FD={0}/FU={1}   SD={2}/SU={3}",firstDown,firstUp,secondDown,secondUp,Mmo.resultIndex);
    }
}