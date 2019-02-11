using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransparentRawImage : MonoBehaviour {

    private RawImage image;

    void Awake()
    {
        image = GetComponent<RawImage>();
    }

    // Use this for initialization
    void Start()
    {
        

        Color col = image.color;
        image.color = new Color(col.r, col.g, col.b, 0f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetAlpha(float alpha)
    {
        if (alpha < 0)
        {
            alpha = 0f;
        }
        if (alpha > 1)
        {
            alpha = 1f;
        }
        Color col = image.color;
        image.color = new Color(col.r, col.g, col.b, alpha);
    }
}
