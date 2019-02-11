using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using Assets.Plugins.LocationAwarenessPlugin;
using DefaultNamespace.GoFindScripts;
using GeoARDisplay.Scripts;
using GoFindScripts;
using GoogleARCore;
using UnityEngine;
using UnityEngine.UI;
using Logger = Assets.Modules.SimpleLogging.Logger;

public class ARMapper : MonoBehaviour {
    public int Scale = 100000; // Empirically found value
    public bool Debug;
    public bool DoUpdates = false;
    public bool Experimental = false;

    public ARMapperDebugUI DebugUI;

    public Text TextualInfoDisplay; // DEBUG

    private LocationService LocationService;
    private CalibrationUtility CalibrationUtility;
    private bool gpsReady = false;

    private GeoArithmetic.GeoCoordinates current;

    private List<GeoPositioned> positionedObjects;
    private bool originOriented = false;
    private Anchor origin;
    private Transform originTransform;
    private bool anchorSet = false;

    private Camera arCoreCam;

    public bool IsGPSReady() {
        return gpsReady;
    }

    public bool IsAnchorSet() {
        return anchorSet;
    }

    public void ToggleDebug() {
        Debug = !Debug;
    }

    private State state = State.PRE_INIT;

    public enum State {
        PRE_INIT,
        INITIALISING,
        INITIALISED,
        RUNNING,
        STOPPED,
        AWAITING_LOCATION,
        AWAITING_AR
    }

    public void Run() {
        state = State.RUNNING;
    }

    public void Stop() {
        state = State.STOPPED;
    }

    public bool IsAvailable() {
        if (Application.isEditor) {
            return false;
        }

        return state == State.INITIALISED;
    }

    private Logger logger;
    
    private void Awake() {
        positionedObjects = new List<GeoPositioned>();
        logger = LogManager.GetInstance().GetLogger(GetType());
    }

    public void SetOnCalibrationDone(Action calibrationFinished) {
        CalibrationUtility.OnCalibrationDone(calibrationFinished);
    }

    private IEnumerator InitLocationService() {
        state = State.AWAITING_LOCATION;
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        if (Input.location.status != LocationServiceStatus.Initializing &&
            Input.location.status != LocationServiceStatus.Running) {
            // Start service before querying location
            Input.location.Start();
        }

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1) {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed) {
            print("Unable to determine device location");
            yield break;
        } else {
            LocationService = Input.location;
            gpsReady = true;
        }
    }

    // Use this for initialization
    void Start() {
        #if UNITY_ANDROID
        if (Session.Status == SessionStatus.ErrorApkNotAvailable) {
            AsyncTask<ApkInstallationStatus> task = Session.RequestApkInstallation(true);
            task.ThenAction(status => { logger.Debug("Status: {0}", status); });
        }
        #endif
        logger.Debug("AR CORE STATUS: {0}", Session.Status);

        if (GameObject.FindWithTag("MainCamera") != null &&
            GameObject.FindWithTag("MainCamera").GetComponent<ARCoreBackgroundRenderer>() != null) {
            logger.Debug("TAG APPROACH WORKING");
        }
        
        /*if (GameObject.Find("First person Camera") == null) {
            logger.Error("No first Person Camera Found!");
            return;
        }*/
        CalibrationUtility = GetComponent<CalibrationUtility>();
        arCoreCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        ShowDebugUI(false);
    }

    public void ShowDebugUI(bool show) {
        DebugUI.gameObject.SetActive(show);
        foreach (var posed in positionedObjects) {
            DebugUI.PlacementInfo.SetPlacementInfo(posed.transform.position.x, posed.transform.position.z,
                posed.transform.rotation.eulerAngles.y);
        }
    }


    private void PreInit() {
        StartLocationService();
    }

    public void Initialise() {
        PreInit();
        CalibrationUtility.Init();
        state = State.INITIALISING;
        UnityEngine.Debug.Log("Init ongoing");
        CalibrationUtility.ShowUI(true);
    }

    private void StopLocationService() {
        LocationService.Stop();
    }

    private void StartLocationService() {
        StartCoroutine(InitLocationService());
    }

    /// <summary>
    /// Should be called after init process has finished
    /// </summary>
    /// <param name="posed"></param>
    public void AddGeoPositioned(GeoPositioned posed) {
        positionedObjects.Add(posed);
        posed.transform.parent = originTransform;
        DebugUI.ArTransform = posed.transform;
        UpdatePositioned();
    }

    public void RemoveGeoPositioned(GeoPositioned posed) {
        positionedObjects.Remove(posed);
    }

    private List<TrackedPlane> listOfTrackedPlanes = new List<TrackedPlane>();

    public void SetOnInitialised(Action handler) {
        OnInitialised = handler;
    }

    private Action OnInitialised;
    private bool OnInitInvoked;

    public bool IsRunning() {
        return state == State.RUNNING;
    }

    // Update is called once per frame
    void Update() {
        switch (state) {
            case State.PRE_INIT:
                // DO Nothing, wait for LOCATION
                return;
            case State.INITIALISING:
                if (Session.Status != SessionStatus.Tracking) {
                    Screen.sleepTimeout = 15;
                    if (Time.frameCount % 100 == 0) {
                        UnityEngine.Debug.Log("Not tracking"); // Debug print doesn't have to be printed that often
                    }
                }

                Session.GetTrackables<TrackedPlane>(listOfTrackedPlanes, TrackableQueryFilter.New);

                if (gpsReady) {
                    current = GetCurrentGeoCoordinates();
                }

                if (!anchorSet) {
                    if (Experimental) {
                        var originGo = new GameObject("Origin");
                        originTransform = originGo.transform;
                        originTransform.transform.position = Vector3.zero;
                    } else {
                        origin = Session.CreateAnchor(Pose.identity);
                        originTransform = origin.transform;
                    }

                    anchorSet = true;
                }

                if (anchorSet && CalibrationUtility.IsCalibrated() && !originOriented) {
                    if (originTransform != null) {
                        originTransform.Rotate(Vector3.up, -CalibrationUtility.GetNorthOffset(), Space.World);
                        originOriented = true;
                        UnityEngine.Debug.Log("Anchored!");
                    } else {
                        UnityEngine.Debug.LogWarning("Tried to orient origin-anchor, but was null");
                        anchorSet = false; // Not really sure how to get there
                    }
                }

                if (gpsReady && anchorSet && originOriented) {
                    state = State.INITIALISED;
                    UnityEngine.Debug.Log("Initialised");
                }

                break;
            case State.INITIALISED:
                if (!OnInitInvoked) {
                    OnInitialised.Invoke();
                    OnInitInvoked = true;
                }

                break;
            case State.RUNNING:


                if (Time.frameCount % 10 == 0) {
                    // Every 10th frame
                    if (DoUpdates && gpsReady && anchorSet) {
                        UpdatePositioned(); // maybe only upon creation geoposition the object, from there on, ARCore does the magic?
                    }

                    UnityEngine.Debug.Log(string.Format("GpsReady={0}, anchorSet={1}", gpsReady, anchorSet));
                }

                break;
            case State.STOPPED:
                return; // Nothing to do
            case State.AWAITING_LOCATION:
                // DO Nothing, wait for LOCATION
                break;
            case State.AWAITING_AR:

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdatePositioned() {
        GeoArithmetic.GeoCoordinates currentOrigin = GetCurrentGeoCoordinates();
        foreach (GeoPositioned positionedObject in positionedObjects) {
            UpdatePositioned(positionedObject, currentOrigin);
        }
    }

    private void UpdatePositioned(GeoPositioned posed, GeoArithmetic.GeoCoordinates currentOrigin) {
        double latOff = posed.GeoCoordinates.latitude - currentOrigin.latitude;
        double lonOff = posed.GeoCoordinates.longitude - currentOrigin.longitude;

        Vector3 transformPosition = posed.transform.position;
        if (Debug) {
            transformPosition.z = arCoreCam.transform.position.z;
            transformPosition.x = arCoreCam.transform.position.x;
        } else {
            transformPosition.z = (float) (latOff * Scale);
            transformPosition.x = (float) (lonOff * Scale);
        }

        posed.transform.position = transformPosition;

        var infoMsg = string.Format("Updated GeoPos {0},{1}) @ {2},{3}, with O={4},{5} @ {6},{7}",
            posed.GeoCoordinates.latitude,
            posed.GeoCoordinates.longitude, transformPosition.z, transformPosition.x, currentOrigin.latitude,
            currentOrigin.longitude, arCoreCam.transform.position.z, arCoreCam.transform.position.x);
        logger.Debug(infoMsg);
        UIManager.ShowAndroidToastMessage(infoMsg);


        if (posed.IsBearingSet()) {
            if (Debug) {
                posed.transform.Rotate(Vector3.up, 180, Space.Self);
            } else {
                posed.transform.Rotate(Vector3.up, posed.Bearing, Space.Self);
            }
            logger.Debug("Bearing was: {0}, actual euler angles: {1},{2},{3}", posed.Bearing, posed.transform.eulerAngles.x, posed.transform.eulerAngles.y, posed.transform.eulerAngles.z);
        }

        DebugUI.PlacementInfo.SetPlacementInfo(posed.transform.position.x, posed.transform.position.z,
            posed.transform.rotation.eulerAngles.y);
        //DisplayText(string.Format("x={0},z={1}", posed.transform.position.x, posed.transform.position.z));
    }

    public State GetState() {
        return state;
    }

    public GeoArithmetic.GeoCoordinates GetCurrentGeoCoordinates() {
        if (gpsReady) {
            return new GeoArithmetic.GeoCoordinates(LocationService.lastData.latitude,
                LocationService.lastData.longitude);
        } else {
            throw new NotSupportedException();
        }
    }

    public void EnableARCam(bool enable) {
        arCoreCam.enabled = enable;
    }

    public bool IsInitialised() {
        return state == State.INITIALISED;
    }

    private void DisplayText(string str) {
        if (TextualInfoDisplay != null) {
            TextualInfoDisplay.text = str;
        }
    }

    public List<GeoPositioned> GetPositioned() {
        return positionedObjects;
    }

    public void Reset() {
        originOriented = false;
        anchorSet = false;
        state = State.PRE_INIT;
    }
}