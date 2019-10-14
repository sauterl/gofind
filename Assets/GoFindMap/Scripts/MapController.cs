using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.UI;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using UnityEngine;

namespace Assets.GoFindMap.Scripts {
    public class MapController : MonoBehaviour {
        private const float PIXEL_SIZE = 1280f;

        private Bounds bounds;

        private Action<bool> finishedNotifyAction;

        private double initialLatitude;
        private double initialLongitude;

        private double latitude;
        private double longitude;
        public bool mapPlaneRotated = true;

        private string[] markers;

        public GameObject markerPrefab;

        private bool markersIncluded = false;

        private bool reloading;

        private MapTouchController touchController;

        private Action<GeoLocation> onLocationSelectedHandler = null;

        private Camera mapCam;

        public void SetOnLocationSelectedHandler(Action<GeoLocation> handler)
        {
            onLocationSelectedHandler = handler;
        }

        private void Awake() {
            touchController = GetComponent<MapTouchController>();
            reloading = false;
            mapCam = GameObject.Find("MapCamera").GetComponent<Camera>();
        }

        public Action<GeoLocation> GetOnLocationSelectedHandler()
        {
            return onLocationSelectedHandler;
        }

        public double GetLatitude() {
            return latitude;
        }

        public double GetLongitude() {
            return longitude;
        }


        private void Start() {
            bounds = GetComponent<MeshCollider>().bounds;
            /*
            latitude = 47.56441; // Legacy debug
            longitude = 7.615870; // Legacy debug
            Initialize(latitude, longitude);*/
        }

        public void Initialize(double latitude, double longitude) {
            initialLatitude = latitude;
            initialLongitude = longitude;
            LoadMap(latitude, longitude);
        }

        public void LoadMap(double latitude, double longitude, bool reCenter = false,
            Action<bool> finishedNotifyAction = null, string[] markers = null) {
            this.latitude = latitude;
            this.longitude = longitude;
            this.finishedNotifyAction = finishedNotifyAction;
            StartCoroutine(LoadMap(reCenter, markers));
        }

        public void ReloadAndCenter(double deltaLatitude, double deltaLongitude, Action<bool> finishedReloading = null,
            string[] markers = null) {
            if (reloading) {
                return;
            }
            Debug.Log("ReloadAndCenter " + CreateLocationDump());
            Debug.LogFormat("Reload: {0}/{1} -> {2}/{3}", deltaLatitude, deltaLongitude, latitude + deltaLatitude,
                longitude + deltaLongitude);
            if ((this.markers == null) && (markers != null)) {
                this.markers = markers;
            }
            LoadMap(latitude + deltaLatitude, longitude + deltaLongitude, true, finishedReloading, this.markers);
        }

        private IEnumerator LoadMap(bool reCenter = false, string[] markers = null) {
            Debug.Log("LoadMapIEnumerator " + CreateLocationDump());
            reloading = true;
            string url = BuildUrl(latitude, longitude, markers);
            Debug.LogFormat("URL={0}", url);

            var www = new WWW(url);
            yield return www;

            var renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = www.textureNonReadable;

            if (reCenter) {
                transform.position = Vector3.zero;
            }

            reloading = false;
            ReloadFinished();
        }

        private void ReloadFinished() {
            if (finishedNotifyAction != null) {
                Debug.Log("Notifying, reload finished");
                finishedNotifyAction.Invoke(true);
            }
            touchController.EnableDoubleTap(!markersIncluded);
        }

        public bool AreMarkersPresent() {
            return markersIncluded;
        }

        private bool markersExistent = false;

        public UIManager uiManager;

        public void CreateMarkers(List<MultimediaObject> mmos) {
            markersExistent = true;
            foreach (MultimediaObject mmo in mmos) {
                GameObject markerObj = Instantiate(markerPrefab);
                if (markerObj == null) {
                    Debug.Log("ERROR! MarkerObject was not instantiated");
                }
                markerObj.GetComponent<ResultMarker>().result = mmo;
                markerObj.GetComponent<ResultMarker>().uiManager = uiManager;
                markerObj.AddComponent<GeoPosition>();
                markerObj.GetComponent<GeoPosition>().Location = new GeoLocation(mmo.latitude, mmo.longitude);
                touchController.AddGeoPositionedObject(markerObj);
            }
        }

        public static string CreateTinyMarkerAt(double latitude, double longitude) {
            return "size:tiny%7Ccolor:red%7C" + latitude + "," + longitude;
        }

        public static string BuildBaseUrl(double latitude, double longitude, string[] markers = null) {
            
            var baseURL = "https://maps.googleapis.com/maps/api/staticmap?";
            string location = "center=" + latitude + "," + longitude;
            string parameters = "&zoom=" + ScalingUtils.ZOOM_FACTOR;
            parameters += "&size=" + "640x640";
            parameters += "&scale=" + "2";
            parameters += "&maptype=" + "hybrid";

            if (markers != null) {
                foreach (string marker in markers) {
                    parameters += "&markers=" + marker;
                }
            }
            
            return baseURL + location + parameters;
        }

        public static string AddCredentialsToUrl(string url) {
            return url + "&key=" + GoogleMapsKey.API_KEY; // Use YOUR Google Maps Static API key here
        }

        private string BuildUrl(double latitude, double longitude, string[] markers = null) {
            Debug.Log("BuilUrl " + CreateLocationDump());
            string baseURL = BuildBaseUrl(latitude, longitude, markers);

            if (!markersExistent){
                if (markers != null)
                {
                    markersIncluded = true;
                }
                else
                {
                    markersIncluded = false;
                }
            }

            return AddCredentialsToUrl(baseURL);
        }

        public void ReloadAndCenterInitial() {
            Debug.Log("ReloadInitial: " + CreateLocationDump());
            LoadMap(initialLatitude, initialLongitude, true);
        }

        private string CreateLocationDump() {
            return string.Format("Initial {0},{1} - Current {2},{3}", initialLatitude, initialLongitude, latitude,
                longitude);
        }

        public void EnableCamera(bool enable) {
            mapCam.enabled = enable;
        }
    }
}