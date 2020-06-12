using System;
using System.Collections.Generic;
using Assets.GoFindMap.Scripts;
using Assets.Modules.SimpleLogging;
using Assets.Plugins.LocationAwarenessPlugin;
using Assets.Scripts.IO;
using DefaultNamespace.GoFindScripts;
using GeoARDisplay.Scripts;
using UnityEngine;
using Logger = Assets.Modules.SimpleLogging.Logger;

using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models.Messages.Query;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Processing;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;

namespace Assets.Scripts.Core {
    public class Controller : MonoBehaviour {
        public enum State {
            IDLE,
            CINEAST_REQUEST,
            CINEAST_RESPONSE,
            LOCATION_STARTED,
            LOCATION_FOUND,
            DISPLAYING,
            UNKOWN
        }


        private const float CLOSE_DISTANCE = 10f;
        private static readonly Configuration CONFIG = new Configuration(5f, CLOSE_DISTANCE, 30 * 1000);

        public GameObject ScreenPrefab;

        private MultimediaObject active;

        private CineastApi cineast;

        private State currentState;
        private GeoLocation initialGeoLocation;

        private LocationInfo initialLocation;

        private Logger logger;
        public MapController mapController;

        private List<MultimediaObject> mmoList;

        private LocationAwarenessPlugin nativeLocation;

        private LocationTargetList targetList;

        public UIManager uiManager;

        public bool DisplayAR = false;

        private UnityLocationService unityLocation;

        private ARMapper arMapper;

        [Obsolete("Will be replaced by bearing of object, rather than the heading of the device")]
        private HeadingDictionary headingDictionary;

        private void Awake() {
            // Important to not crash in editor
            if (Application.isEditor) {
                GameObject.Find("ARCore Device").active = false;
            }


            ActiveScreens = new List<GameObject>();

            logger = LogManager.GetInstance().GetLogger(GetType());
            logger.Debug("Awake!");
            currentState = State.IDLE;

            // Use Awake instead of ctor
            nativeLocation = LocationAwarenessPlugin.GetInstance();
            logger.Debug("Created LocationAwarenessPlugin");

            gameObject.AddComponent<UnityLocationService>();
            unityLocation = gameObject.GetComponent<UnityLocationService>();
            logger.Debug("Added UnityLocationService to Controller");


            gameObject.AddComponent<CineastApi>();
            cineast = gameObject.GetComponent<CineastApi>();
            logger.Debug("Added CineastApi to Controller");

            headingDictionary = new HeadingDictionary();

            uiManager.controller = this;
            
            logger.Debug("CineastApi Config: "+JsonUtility.ToJson(CineastUtils.Configuration));
        }

        // maxDist in km
        public void SetMaxDistance(float maxDist) {
            logger.Debug("Max distance pre query filter: " + maxDist * 1000 + "m");
            cineast.AddCineastFilter(new SpatialMaxDistanceFilter(maxDist * 1000, initialGeoLocation.latitude,
                initialGeoLocation.longitude));
        }

        public void StoreHeading(MultimediaObject mmo, int heading) {
            logger.Debug("Writing heading " + heading + " for mmo " + mmo.id);
            headingDictionary.Add(mmo.id, heading);
        }

        public int GetHeading(MultimediaObject mmo) {
            if (float.IsNaN(headingDictionary.GetHeading(mmo.id))) {
                return -1;
            }

            return (int) headingDictionary.GetHeading(mmo.id);
        }

        private void Start() {
            logger.Debug("Start");
            // Before first frame


            if (!unityLocation.IsLocationServiceEnabled()) {
                logger.Warn("No location service available. Using default location");
                initialGeoLocation = new GeoLocation(47.560127, 7.589704);
                InitializeMap(initialGeoLocation);
            } else {
                logger.Debug("Requesting intial location");
                unityLocation.StartSingleRequest(HandleInitialLocationFound);
            }

            arMapper = GameObject.Find("ARMapper").GetComponent<ARMapper>();

            logger.Info("Early initialization done");

            uiManager.EnableMapCam(true);
            
            //EnableARCam(true);
            //uiManager.EnableMapCam(false);
            //uiManager.panelManager.ShowPanel("calibration");
        }

        private void HandleInitialLocationFound(LocationInfo initLoc) {
            InitializeMap(initLoc);
            RestoreState();
        }

        private void InitializeMap(LocationInfo info) {
            logger.Debug("Initialize Map LInfo" + info.latitude + "," + info.longitude);
            SetInitialLocation(info);
            mapController.Initialize(initialGeoLocation.latitude, initialGeoLocation.longitude);
        }

        private void InitializeMap(GeoLocation loc) {
            logger.Debug("Initialize Map GeoLoc" + loc);
            initialGeoLocation = loc;
            mapController.Initialize(initialGeoLocation.latitude, initialGeoLocation.longitude);
        }

        public void DoCineastRequest(double latitude, double longitude) {
            logger.Debug("DoCineastRequest double,double" + latitude + "," + longitude);

            Clean();

            initialLocation = new LocationInfo(); // Why?
            ChangeState(State.CINEAST_REQUEST);
            //cineast.RequestSimilarAndThen(initialLocation.latitude, initialLocation.longitude, HandleCineastResult);
            cineast.RequestSimilarAndThen(QueryFactory.BuildSpatialSimilarQuery(latitude, longitude),
                HandleCineastResult);
        }

        private void Clean() {
            logger.Debug("Clean");
            if ((mmoList != null) && (mmoList.Count > 0)) {
                logger.Debug("Cleaing internal result lists");
                mmoList.Clear();
                uiManager.ClearList();
                cineast.Clean();
                logger.Info("Cleaned internal result container");
            }
        }


        private void DoCineastRequest(LocationInfo initialLocation) {
            logger.Debug("DoCineastRequest LInfo " + initialLocation.latitude + "," + initialLocation.longitude);
            Clean();
            ResetSession();
            SetInitialLocation(initialLocation);
            ChangeState(State.CINEAST_REQUEST);
            //cineast.RequestSimilarAndThen(initialLocation.latitude, initialLocation.longitude, HandleCineastResult);
            cineast.RequestSimilarAndThen(
                QueryFactory.BuildSpatialSimilarQuery(initialLocation.latitude, initialLocation.longitude),
                HandleCineastResult);
        }

        public void Go() {
            logger.Info("Go!");
            unityLocation.StartSingleRequest(DoCineastRequest);
            uiManager.panelManager.ShowPanel("waiting");
        }

        private void SetInitialLocation(LocationInfo loc) {
            logger.Info("Setting initial location (" + loc.latitude + "," + loc.longitude + ")");
            initialLocation = loc;
            initialGeoLocation = new GeoLocation(loc.latitude, loc.longitude);
            uiManager.SetInitialGeoLocation(initialGeoLocation);
        }

        public void GoQuery(SimilarQuery query) {
            logger.Debug("GoQuery");
            ChangeState(State.CINEAST_REQUEST);
            Clean();
            ResetSession();
            cineast.RequestSimilarAndThen(query, HandleCineastResult);
        }

        public void GoMultiTermQuery(SimilarQuery query) {
            logger.Debug("GoMultiQuery");
            ChangeState(State.CINEAST_REQUEST);
            Clean();
            ResetSession();
            uiManager.panelManager.ShowPanel("waiting");
            CategoryRatio cr = CineastUtils.CreateUniformRatio(query);
            logger.Debug("Ratios: " + cr.ToString());
            cineast.RequestWeightedSimilarAndThen(query, cr, HandleCineastResult);
        }

        /*
        public void TestQbE() {
            logger.Debug("Test QbE");
            ChangeState(State.CINEAST_REQUEST);
            Clean();
            ResetSession();
            SimilarQuery query = QueryFactory.BuildGlobalColEdgeSimilarQuery(QueryImageHelper.TEST_DATAURL);
            cineast.RequestWeightedSimilarAndThen(query, CineastUtils.QUERY_BY_EXAMPLE_EDGE_GLOBALCOL, HandleCineastResult);
        }

        public void TestMultiQuery() {
            logger.Debug("Testing multi query");
            ChangeState(State.CINEAST_REQUEST);
            Clean();
            ResetSession();
            SimilarQuery query = QueryFactory.BuildMultiCategoryQuery(
                new[] {TermsObject.EDGE_CATEGORY, TermsObject.GLOBALCOLOR_CATEGORY, TermsObject.LOCALCOLOR_CATEGORY},
                QueryImageHelper.TEST_DATAURL);
            cineast.RequestWeightedSimilarAndThen(query, CineastUtils.QBE_GLOBAL_LOCAL_EDGE_EQUAL_RATIO, HandleCineastResult
            );
        }*/

        private void HandleCineastResult(List<MultimediaObject> list) {
            logger.Debug("HandleCineastResult");
            ChangeState(State.CINEAST_RESPONSE);
            mmoList = list;
            logger.Debug("Internal mmo list set to received one");

            // == SORT DISTANCE ==
            /*
            mmoList.Sort((mmo1, mmo2) => {
                double dist1 = Utilities.HaversineDistance(mmo1.latitude, mmo1.longitude, initialGeoLocation.latitude,
                    initialGeoLocation.longitude);
                double dist2 = Utilities.HaversineDistance(mmo2.latitude, mmo2.longitude, initialGeoLocation.latitude,
                    initialGeoLocation.longitude);
                return dist1.CompareTo(dist2);
            });
            logger.Debug("Sorted mmo list");
            */

            mapController.CreateMarkers(mmoList);
            mapController.ReloadAndCenter(0, 0, null, CreateMarkers());

            // DEBUG
            //InjectDebug();

            uiManager.SetInitialLocation(initialLocation);
            uiManager.SetInitialGeoLocation(initialGeoLocation);
            //uiManager.panelSwitcher.SwitchToChoice();

            uiManager.panelManager.ShowPanel("choice");
            uiManager.viewChoiceHomeBtn.gameObject.SetActive(true);
            activeMmos = list;
            uiManager.SetAndPopulateList(activeMmos);
        }

        private string[] CreateMarkers() {
            List<string> markers = new List<string>();
            foreach (MultimediaObject mmo in mmoList) {
                markers.Add(string.Format("color:red%7Clabel:R%7C{0},{1}", mmo.latitude, mmo.longitude));
            }

            return markers.ToArray();
        }

        private void RestartLocationServices(bool native = true, bool unity = true) {
            logger.Debug("Restarting locaiton services");

            targetList = new LocationTargetList(Convert(activeMmos).ToArray());

            logger.Debug("TargetList: " + JsonUtility.ToJson(targetList));
            if (native) {
                ChangeState(State.LOCATION_STARTED);
                nativeLocation.StartService(CONFIG, targetList);
            }

            if (unity) {
                unityLocation.SetLocationHandler(HandleLocationFound);
                unityLocation.StartService(CONFIG, targetList);
            }

            if (native || unity) {
                uiManager.ShowWatchAll(true);
            }
        }


        public void WatchAll() {
            logger.Debug("WatchAll");

            RestartLocationServices();
            uiManager.resetSessionButton.gameObject.SetActive(true);
            sessionRunning = true;
            // Watch all MmO:
            // -> create LocationTargetList
            // Start LocationAwarenessPlugin
            // Start UnityLocationService
        }

        private void HandleLocationFound(LocationObject target) {
            logger.Debug("HandleLocationFound " + target.latitude + "," + target.longitude);
            ChangeState(State.LOCATION_FOUND);
            active = FindMmo(target);
            Display(active);
            //unityLocation.StopService();
            //nativeLocation.StopService();
        }


        public void Display(MultimediaObject mmo) {
            logger.Debug("Displaying");
            if (DisplayAR) {
                logger.Debug("Trying to display AR style");
                logger.Debug("AR Mapper state: "+arMapper.GetState());
                if (arMapper.IsAvailable()) {
                    Display3D(mmo);
                    return;
                }

                if (!arMapper.IsInitialised()) {
                    toDisplay = mmo;
                    arMapper.SetOnInitialised(Display);
                    uiManager.SwitchCams(true);
                    uiManager.panelManager.ShowPanel("ar-display");
                    arMapper.Initialise();
                    return;
                }
            }

            Display2D(mmo);
        }

        private List<GameObject> ActiveScreens;

        private MultimediaObject toDisplay;

        private void Display() {
            if (toDisplay != null) {
                Display3D(toDisplay);                
            }
            
        }
        

        private void Display3D(MultimediaObject mmo) {
            logger.Debug("Displaying AR style! "+mmo.id);
            var scrn = Instantiate(ScreenPrefab);
            var mmoContainer = scrn.AddComponent<MMOContainer>();
            var textured = scrn.GetComponentInChildren<WebTextured>();
            
            if (textured == null) {
                textured = scrn.AddComponent<WebTextured>();
            }
            textured.url = CineastUtils.GetImageUrl(mmo); // Too high res?
            //textured.url = CineastUtils.GetThumbnailUrl(mmo);
            textured.LoadImageFromWeb();
            mmoContainer.MultimediaObject = mmo;
            var geoposed = scrn.AddComponent<GeoPositioned>();
            geoposed.GeoCoordinates = new GeoArithmetic.GeoCoordinates(mmo.latitude, mmo.longitude);
            geoposed.Bearing = (float) mmo.bearing;
            arMapper.AddGeoPositioned(geoposed);
            ActiveScreens.Add(scrn);
        }

        public void RemoveScreenOf(MultimediaObject mmo) {
            GameObject go = null;
            foreach (GameObject g in ActiveScreens) {
                if (g.GetComponent<MMOContainer>() != null &&
                    g.GetComponent<MMOContainer>().MultimediaObject.Equals(mmo)) {
                    {
                        go = g;
                        break;
                    }
                }
            }

            if (go != null) {
                var geoposed = go.GetComponent<GeoPositioned>();
                if (geoposed != null) {
                    arMapper.RemoveGeoPositioned(geoposed);
                }

                ActiveScreens.Remove(go);
            }
        }

        public void RemoveScreens() {
            foreach (GameObject obj in ActiveScreens) {
                var g = obj.GetComponent<GeoPositioned>();
                arMapper.RemoveGeoPositioned(g);
            }

            ActiveScreens.Clear();
        }

        private void Display2D(MultimediaObject mmo) {
            logger.Debug("Display Index:" + mmo.resultIndex);
            ChangeState(State.DISPLAYING);
            uiManager.Present(mmo);
            uiManager.viewDisplayHomeBtn.gameObject.SetActive(true);

            // Setup Cam
            // Retrieve Image
            // display image and all
        }

        private List<LocationObject> Convert(List<MultimediaObject> input) {
            if (input == null) {
                logger.Warn("Convert - Cannot convert a null-list");
                return null;
            }

            List<LocationObject> output = new List<LocationObject>();
            logger.Debug("Convert - Input: " + input != null ? "" + input.Count : "NULL");

            int count = 0;
            foreach (MultimediaObject mmo in input) {
                if (mmo == null) {
                    logger.Debug("Convert - Skipping a null-mmo");
                } else {
                    output.Add(new LocationObject(mmo.id, mmo.latitude, mmo.longitude));
                    count++;
                }
            }

            logger.Debug("Convert - Converted " + count + " mmos to locobjs");
            return output;
        }

        private void ChangeState(State newState) {
            logger.Debug(string.Format("Ctrl:Changing state from {0} to {1}", currentState, newState));
            currentState = newState;
            if (currentState == State.LOCATION_STARTED) {
                logger.Debug("Setting sessionrunning");
                sessionRunning = true;
            }
        }

        private void OnApplicationPause(bool pause) {
            if (pause) {
                logger.LogPause(pause);

                OnApplicationQuit(); // 'closing' app seems not to call quit//http://answers.unity3d.com/answers/1040173/view.html
            } else {
                Restore(); // order matters, logging works only after restore
                logger.LogPause(pause);
            }
        }

        private void Restore() {
            LogManager.GetInstance().Reopen();
            RestoreState();
            logger.Debug("Restored");
        }

        private void OnApplicationFocus(bool focus) {
            if (focus) {
                Debug.Log("Ctrl:OnApplicationFocus GAINED");
                // running
            } else {
                Debug.Log("Ctrl:OnApplicationFocus LOST");
                // not running
            }
        }

        private void OnApplicationQuit() {
            Debug.Log(":OnApplicationQuit");
            // if LOCATION_STARTED, save location stuff

            StoreState();

            if (sessionRunning) {
                logger.Debug("Starting native service due to running session");
                RestartLocationServices(true, false);
            } else {
                logger.Debug("Removing data");
                IOUtils.RemoveData();
            }

            LogManager.GetInstance().Close();
        }

        private MultimediaObject FindMmo(LocationObject obj) {
            foreach (MultimediaObject mmo in mmoList) {
                if (mmo.id.Equals(obj.id)) {
                    return mmo;
                }
            }

            return null;
        }

        private void StoreState() {
            var save = new SaveState();
            save.state = currentState;
            var dirty = true;
            switch (currentState) {
                case State.LOCATION_STARTED:
                    save.targetList = targetList;
                    save.activeMmos = activeMmos;
                    break;
                case State.LOCATION_FOUND:
                    save.active = active;
                    break;
                case State.DISPLAYING:
                    save.active = active;
                    break;
                default:
                    dirty = false;
                    break;
            }

            if (dirty) {
                IOUtils.StoreData(save);
            }

            if (!headingDictionary.IsEmpty()) {
                logger.Debug(
                    "Nonempty HeadingDictionary. Storing it on disc!\n" + JsonUtility.ToJson(headingDictionary));
                IOUtils.StoreHeadings(headingDictionary);
            }
        }

        public List<MultimediaObject> GetInRange(MultimediaObject mmo, double dist = 75 /*m*/) {
            List<MultimediaObject> ret = new List<MultimediaObject>();
            if (!double.IsNaN(mmo.latitude)) {
                foreach (MultimediaObject mo in activeMmos) {
                    double d = Utilities.HaversineDistance(mmo.latitude, mmo.longitude, mo.latitude, mo.longitude);
                    if (d <= dist) {
                        ret.Add(mo);
                    }
                }
            }

            return ret;
        }

        private void RestoreState() {
            logger.Debug("Restoring state");
            if (IOUtils.ExistsPersitentData()) {
                SaveState save = IOUtils.LoadData();
                logger.Debug("Restored state: " + JsonUtility.ToJson(save));
                switch (save.state) {
                    case State.LOCATION_STARTED:
                        targetList = save.targetList;
                        currentState = save.state;
                        activeMmos = save.activeMmos;
                        Invoke("RestoreChoice", 0.5f);
                        // Restart location service
                        // Run is-close check
                        WatchAll();
                        break;
                    case State.LOCATION_FOUND:
                    case State.DISPLAYING:
                        active = save.active;
                        // Restart display service
                        break;
                }
            }

            if (IOUtils.ExistsHeadingFile()) {
                headingDictionary = IOUtils.LoadHeadings();
                Debug.Log("Loaded headings:\n" + JsonUtility.ToJson(headingDictionary));
            }
        }

        private void RestoreChoice() {
            uiManager.SetAndPopulateList(activeMmos);
            logger.Debug("Successfully restored choice view");
            logger.Debug("Restoring choice panel");
            uiManager.viewChoiceHomeBtn.gameObject.SetActive(true);
        }

        public bool ExistsResultIndex(int index) {
            foreach (MultimediaObject mmo in mmoList) {
                if (mmo.resultIndex == index) {
                    return true;
                }
            }

            return false;
        }

        public MultimediaObject GetMultimediaObject(int index) {
            MultimediaObject ret = null;
            foreach (MultimediaObject mmo in mmoList) {
                if (mmo.resultIndex == index) {
                    ret = mmo;
                }
            }

            return ret;
        }


        public GeoLocation GetInitialGeoLocation() {
            return initialGeoLocation;
        }

        public void SetTemporalRange(int lower, int upper) {
            cineast.AddCineastFilter(new TemporalRangeFilter(lower, upper));
        }

        public GeoLocation GetLastKnownLocation() {
            LocationInfo nfo = unityLocation.GetLastKnownLocation();
            return new GeoLocation(nfo.latitude, nfo.longitude);
        }

        public List<MultimediaObject> GetOriginalList() {
            List<MultimediaObject> ret = new List<MultimediaObject>(cineast.GetOriginalResults());
            ret.Sort((mmo1, mmo2) => mmo1.resultIndex - mmo2.resultIndex); // Lambda, sort according index
            return ret;
        }

        private List<MultimediaObject> activeMmos;

        public void SetActiveList(List<MultimediaObject> mmos) {
            activeMmos = mmos;
            uiManager.SetAndPopulateList(activeMmos);
        }

        public void RemoveFromActiveList(MultimediaObject mmo) {
            activeMmos.Remove(mmo);
        }

        // If already in list, ignored
        public void AddToActiveList(MultimediaObject mmo) {
            if (!activeMmos.Contains(mmo)) {
                activeMmos.Add(mmo);
            }
        }

        public void ResetLists() {
            logger.Debug("Resetting lists");
            Clean();
        }

        public void ResetSession() {
            logger.Debug("Resetting session");
            IOUtils.RemoveData();
            StopLocationServices();
            ResetLists();
            uiManager.resetSessionButton.gameObject.SetActive(false);
            uiManager.viewChoiceHomeBtn.gameObject.SetActive(false);
            sessionRunning = false;
        }

        public void StopLocationServices(bool native = true, bool unity = true) {
            logger.Debug("Stopping location services: native=" + native + ",unity=" + unity);
            if (native) {
                nativeLocation.StopService();
            }

            if (unity) {
                unityLocation.StopService();
            }
        }

        private bool sessionRunning = false;

        public bool IsSessionRunning() {
            return sessionRunning;
        }

        public void EnableARCam(bool enable) {
            arMapper.EnableARCam(enable);
        }

        public void ResetARSession() {
            arMapper.Reset();
        }
    }
}