using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPanelController : MonoBehaviour
{

    private const string HEAD_PANEL_NAME = "HeadPanel";
    private const string BOTTOM_PANEL_NAME = "BottomPanel";
    private const string TITLE_NAME = "TitleText";
    private const string FOOTER_NAME = "FooterText";
    private const string IMAGE_NAME = "DisplayImage";

    private Text titleText;
    private Text footerText;
    private RawImage imageDisplay;


    void Awake()
    {
        titleText = GameObject.Find(HEAD_PANEL_NAME+"/"+TITLE_NAME).GetComponent<Text>();
        footerText = GameObject.Find(BOTTOM_PANEL_NAME + "/" + FOOTER_NAME).GetComponent<Text>();
        imageDisplay = GameObject.Find(IMAGE_NAME).GetComponent<RawImage>();
    }
    
	// Use this for initialization
	void Start ()
	{
        
	}

    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public void SetFooter(string footer)
    {
        if (footerText != null)
        {
            footerText.text = footer;
        }
    }

    public RawImage GetImage()
    {
        return imageDisplay;
    }

    private string imageUrl;

    public void SetImageUrl(string url)
    {
        imageUrl = url;
    }

    public void LoadImage(string url)
    {
        Debug.Log(":LoadImage - " + url);
        SetImageUrl(url);
        StartCoroutine(RetrieveImage());
    }

    private Texture2D texture;

    public IEnumerator RetrieveImage()
    {
        WWW www = new WWW(imageUrl);
        yield return www;

        texture = www.texture;
        imageDisplay.texture = texture;
    }
}
