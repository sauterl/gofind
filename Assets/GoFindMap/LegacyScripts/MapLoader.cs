using System.Collections;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public const double X_SCALE = 465f / 1280f; //zoom level 15, for movement times 10
    public const double Y_SCALE = 690f / 1280f;
    public const double SCALE_FACTOR = 10d;

    private const string API_KEY = "AIzaSyAXMm1eN70Xo_5KjPzsMnPZGSerD2oXTNw";

    private double latitude;
    private double longitude;


    private MapTouchMover mover;

    private double previousLatitude;
    private double previousLongitude;

    private bool reloading;

    private void Awake()
    {
        mover = GetComponent<MapTouchMover>();
    }

    // Use this for initialization
    private void Start()
    {
        latitude = 47.56441;
        longitude = 7.615870;
        StartCoroutine(LoadMap(latitude, longitude));
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void ReloadAndCenter(double deltaLat, double deltaLon)
    {
        if (reloading)
            return;
        Debug.LogFormat("lat={0},lon={1}", latitude, longitude);
        Debug.LogFormat(":ReloadAndCenter deltaLat={0},deltaLon={1}", deltaLat, deltaLon);
        StartCoroutine(LoadMap(previousLatitude + deltaLat, previousLongitude + deltaLon, true));
        reloading = true;
    }

    private IEnumerator LoadMap(double latitude, double longitude, bool reCenter = false)
    {
        var baseURL = "https://maps.googleapis.com/maps/api/staticmap?";
        var location = "center=" + latitude + "," + longitude;
        var parameters = "&zoom=" + "15";
        parameters += "&size=" + "640x640";
        parameters += "&scale=" + "2";
        parameters += "&maptype=" + "hybrid";

        var url = "";
        url = baseURL + location + parameters + "&key=" + API_KEY;
        Debug.Log(url);

        var www = new WWW(url);
        // Wait for download to complete
        yield return www;

        var renderer = GetComponent<Renderer>();
        // assign texture
        renderer.material.mainTexture = www.textureNonReadable;

        previousLatitude = latitude;
        previousLongitude = longitude;

        if (reCenter)
            transform.position = Vector3.zero;
        reloading = false;
        mover.touchFreeze = false;
    }
}