using System;
using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using Assets.Scripts.Core;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Logger = Assets.Modules.SimpleLogging.Logger;

namespace Assets.GoFindMap.Scripts {
    public class MapTouchController : MonoBehaviour {
        private const float POSITION_FIX_FACTOR = 0.5f;

        private Vector3 boundary;

        private Vector3 calcVec;


        public new Camera camera;

        private float camHeight;
        private float camWidth;

        private bool doubleTapEnabled;

        public GameObject doubleTapIndicator;

        private float flipFactor = 1f;

        private readonly List<GameObject> geoPositionedObjects = new List<GameObject>();
        public CustomizeSearchHandler handler;
        public MapController mapController;
        private float planeHeight;
        private float planeWidth;

        private Vector3 prevPos;
        private Vector3 targetPos;

        private bool touchFreeze;
        private Vector3 translation;
        private Vector3 worldDragDelta;
        private Vector3 worldDragEnd;

        private Vector3 worldDragStart;

        private Logger logger;

        private void Awake() {
            logger = LogManager.GetInstance().GetLogger(GetType());
            calcVec = Vector3.zero;

            mapController = GetComponent<MapController>();
            flipFactor = mapController.mapPlaneRotated ? -1f : 1f;

            // Find camera
            var err = false;
            GameObject camObj = GameObject.Find("MapCamera");
            if (camObj != null) {
                camera = camObj.GetComponent<Camera>();
                if (!camera.orthographic) {
                    err = true;
                }
            } else {
                err = true;
            }
            if (err) {
                Debug.LogError("Could not find a orthographic camera called MapCamera.Set in inspector please");
            }

            EnableDoubleTap(false);
            EnableTouch(false);
        }

        private void InitializeBoundary() {
            // CamSize by:
            //http://answers.unity3d.com/questions/230190/how-to-get-the-width-and-height-of-a-orthographic.html
            camHeight = 2f * camera.orthographicSize;
            camWidth = camHeight * camera.aspect;

            Bounds bounds = GetComponent<MeshCollider>().bounds;

            planeHeight = bounds.size.z;
            planeWidth = bounds.size.x;

            boundary.x = Math.Abs((camWidth / 2f) - (planeWidth / 2f));
            boundary.z = Math.Abs((camHeight / 2f) - (planeHeight / 2f));

            Debug.LogFormat("CamSize: {0}/{1}, PlaneSize:{2}/{3}, Boundary:{4}", camWidth, camHeight, planeWidth,
                planeHeight, boundary);
        }

        // Use this for initialization
        private void Start() {
            InitializeBoundary();

            prevPos = Vector3.zero;
        }

        // Update is called once per frame
        private void Update() {
            
            if (EventSystem.current.currentSelectedGameObject != null) {
                return;
            }
            if (touchFreeze)
            {
                return;
            }
            if (Input.touchCount > 0) {
                HandleTouch(Input.GetTouch(0));
            }
        }

        private void HandleTouch(Touch touch) {
            //Debug.LogFormat(":Touch {0}", Time.frameCount);
            if (touch.phase == TouchPhase.Began) {
                HandleTouchBegan(touch);
            } else if (touch.phase == TouchPhase.Moved) {
                HandleTouchMoved(touch);
            } else if (touch.phase == TouchPhase.Ended) {
                HandleTouchEnded(touch);
            }
        }

        private void HandleSingleTap(Touch touch) {
            Debug.LogFormat("SingleTap scrnPos={0}", touch.position);
            Vector3 touchWorld = ConvertTouchToWorld(touch.position);
            touchWorld.y = 10;
            RaycastHit hitInfo;
            Debug.DrawRay(touchWorld, Vector3.down, Color.red);
            Debug.LogFormat("RayStart {0}, RayEnd{1}", touchWorld, (touchWorld + 1000 * Vector3.down));
            bool rayResult = Physics.Raycast(touchWorld, Vector3.down, out hitInfo);
            if (rayResult) {
                Debug.Log("Found collider"+ hitInfo.transform.gameObject.name);
                if (hitInfo.transform.gameObject.GetComponent<ResultMarker>() != null) {
                    hitInfo.transform.gameObject.GetComponent<ResultMarker>().HandleClicked();
                    
                }
                
            } else {
                Debug.Log("No collider");
            }
        }

        private GeoLocation lastSelectedLocation = null;

        public GeoLocation GetLastSelectedLocation()
        {
            return lastSelectedLocation;
        }
        
        private void HandleDoubleTap(Touch touch) {
            Debug.LogFormat("DoubleTap {0}", touch);
            Vector3 touchWorld = ConvertTouchToWorld(touch.position);

            Debug.LogFormat("touchWorld:{0}", touchWorld);

            Vector2 offset = GetLatLonOffset(touchWorld * -1);
            var location = new GeoLocation(offset.x + mapController.GetLatitude(),
                offset.y + mapController.GetLongitude());
            // GeoLocation is accurate !

            Debug.LogFormat("NEW {0},{1}", location.latitude, location.longitude); // correct

            SetTapIndicator(touchWorld, location);

            // DEBUG
            //InduceMapReload(touchWorld * -1);

            /**
             *  What i've learned so far:
             *  to set an object, use the touchWorld vector
             *  to get the lat/lon from it, invert it (*-1) since, axis of texture are flipped!
             */

            lastSelectedLocation = location;

            if (mapController.GetOnLocationSelectedHandler() != null)
            {
                mapController.GetOnLocationSelectedHandler().Invoke(location);
            }
        }
        
        

        private void SetTapIndicator(Vector3 world, GeoLocation loc) {
            if (doubleTapIndicator != null) {
                if (doubleTapIndicator.GetComponent<GeoPosition>() == null) {
                    doubleTapIndicator.AddComponent<GeoPosition>();
                }
                doubleTapIndicator.GetComponent<GeoPosition>().Location = loc;
                Debug.Log("Indicator set");
                world.y = 0.1f; //no flickering with map (map is @ y=0)
                doubleTapIndicator.transform.position = world;
                AddGeoPositionedObject(doubleTapIndicator);
            }
        }

        private void HandleTouchBegan(Touch touch) {
            Debug.LogFormat(":TouchBegun {0}", Time.frameCount);
            worldDragDelta = Vector3.zero;
            worldDragStart = ConvertTouchToWorld(touch.position);

            Debug.LogFormat("Touch:{0}, wDragStart:{1}", touch.position, worldDragStart);
        }

        private void HandleTouchMoved(Touch touch) {
            Debug.LogFormat(":TouchMoved {0}", Time.frameCount);

            // Reset dragEnd for each frame
            worldDragEnd = ConvertTouchToWorld(touch.position);


            // Calculate dragDelta
            worldDragDelta = worldDragEnd - worldDragStart; // Inverted, since plane is flipped
            //worldDragDelta *= -1;

            Debug.LogFormat("DeltaCalc: Start:{0}, Delta:{1}, End:{2}", worldDragStart, worldDragDelta, worldDragEnd);

            // Store previous position
            prevPos.Set(transform.position.x, transform.position.y, transform.position.z);

            Debug.LogFormat("BeforeClamp delta:{0}, prev:{1}, sum:{2}", worldDragDelta, prevPos,
                worldDragDelta + prevPos);

            // Calculate clamped targetPosition
            var clamped = false;
            targetPos = ClampToBoundary(worldDragDelta + prevPos, out clamped);

            // Effetive translation
            translation = (targetPos - prevPos) * -1;

            // Translate to new positiion
            transform.Translate(translation); // *-1 cause flipped axis

            Debug.LogFormat("AfterTranslate PrevPos:{0}, NewPos:{1}, WDrag:{2}, targetPos:{3}, mov:{4}", prevPos,
                transform.position, worldDragDelta, targetPos, (targetPos - prevPos) * -1);

            //if (worldDragDelta != Vector3.zero && targetPos != worldDragDelta - prevPos)
            if (clamped) {
                // Clamped -> near boundary -> Reload map
                InduceMapReload();
            }

            // Reset dragStart for each frame
            worldDragStart = ConvertTouchToWorld(touch.position);
        }

        public void EnableDoubleTap(bool enabled) {
            logger.Debug("Changing doubletap enabled from {0} to {1}", doubleTapEnabled, enabled);
            doubleTapEnabled = enabled;
        }

        public void EnableTouch(bool touchEnabled) {
            logger.Debug("Changing touch enabled from {0} to {1}", !touchFreeze, touchEnabled);
            touchFreeze = !touchEnabled;
        }

        private void InduceMapReload() {
            Debug.Log("Inducing map reload");

            float deltaLatitude = transform.position.z * POSITION_FIX_FACTOR * flipFactor * ScalingUtils.Z_SCALE;
            float deltaLongitude = transform.position.x * POSITION_FIX_FACTOR * flipFactor * ScalingUtils.X_SCALE;

            mapController.ReloadAndCenter(deltaLatitude * ScalingUtils.SCALE_FACTOR,
                deltaLongitude * ScalingUtils.SCALE_FACTOR, HandleMapLoadingFinished);
            touchFreeze = true;
        }

        private void InduceMapReload(Vector3 vec) //test
        {
            Debug.Log("Induce test");
            Vector2 ret = GetLatLonOffset(vec);
            mapController.ReloadAndCenter(ret.x, ret.y, HandleMapLoadingFinished);
            touchFreeze = true;
            /*
            Debug.Log("Inducing map reload");

            var deltaLatitude = vec.z * POSITION_FIX_FACTOR * flipFactor * ScalingUtils.Z_SCALE;
            var deltaLongitude = vec.x * POSITION_FIX_FACTOR * flipFactor * ScalingUtils.X_SCALE;

            mapController.ReloadAndCenter(deltaLatitude * ScalingUtils.SCALE_FACTOR, deltaLongitude * ScalingUtils.SCALE_FACTOR, EnableTouch);
            touchFreeze = true;*/
        }

        public Vector2 GetLatLonOffset(Vector3 vec) {
            Debug.Log("Offset");

            float deltaLatitude = vec.z * POSITION_FIX_FACTOR * flipFactor * ScalingUtils.Z_SCALE;
            float deltaLongitude = vec.x * POSITION_FIX_FACTOR * flipFactor * ScalingUtils.X_SCALE;

            return new Vector2(deltaLatitude * ScalingUtils.SCALE_FACTOR, deltaLongitude * ScalingUtils.SCALE_FACTOR);
        }

        public void HandleMapLoadingFinished(bool enabled) {
            touchFreeze = false;
            RepositionGeoObjects();
        }

        private void HandleTouchEnded(Touch touch) {
            Debug.LogFormat(":TouchEnded {0}", Time.frameCount);
            worldDragEnd = ConvertTouchToWorld(touch.position);
            worldDragDelta = Vector3.zero;
            Debug.LogFormat("Touch:{0}, wDragEnd:{1}", touch.position, worldDragEnd);

            if (touch.tapCount == 1) {
                HandleSingleTap(touch);
            } else if (touch.tapCount == 2) {
                HandleDoubleTap(touch);
            }
        }

        private Vector3 ConvertTouchToWorld(Vector2 touch) {
            calcVec.Set(touch.x, touch.y, 0);
            Vector3 ret = camera.ScreenToWorldPoint(calcVec);
            Debug.LogFormat(":Convert in:{0},calc:{2} out:{1}", touch, ret, calcVec);
            return ret;
        }

        private Vector3 ClampToBoundary(Vector3 vec, out bool clamped) {
            float x = 0;
            float y = 0;
            float z = 0;
            clamped = false;

            if (Math.Abs(vec.x) > boundary.x) {
                x = boundary.x * (vec.x < 0 ? -1f : 1f);
                clamped = true;
            } else {
                x = vec.x;
            }

            if (Math.Abs(vec.z) > boundary.z) {
                z = boundary.z * (vec.z < 0 ? -1f : 1f);
                clamped = true;
            } else {
                z = vec.z;
            }
            var ret = new Vector3(x, y, z);
            Debug.LogFormat(":Clamp in={0}, ref={1}, out={2}", vec, boundary, ret);
            return ret;
        }

        private void RepositionGeoObjects() {
            Vector3 pos = Vector3.zero;
            foreach (GameObject go in geoPositionedObjects) {
                RepositionGeoObject(go, pos);
            }
        }

        private void RepositionGeoObject(GameObject go, Vector3 pos) {
            var geoPos = go.GetComponent<GeoPosition>();
            // Delta to new center
            double deltaLatitude = mapController.GetLatitude() - geoPos.Location.latitude;
            double deltaLongitude = mapController.GetLongitude() - geoPos.Location.longitude;
            // Revert calculation from reloading and centering.
            double posX = deltaLongitude / ScalingUtils.SCALE_FACTOR /
                          (POSITION_FIX_FACTOR * flipFactor * ScalingUtils.X_SCALE);
            double posZ = deltaLatitude / ScalingUtils.SCALE_FACTOR /
                          (POSITION_FIX_FACTOR * flipFactor * ScalingUtils.Z_SCALE);
            // Set position.y always to 0.1f, so those overlay objects stay over-layed
            pos.Set((float)posX, 0.1f, (float)posZ);
            go.transform.position = pos;
        }

        public void AddGeoPositionedObject(GameObject obj) {
            if (obj.GetComponent<GeoPosition>() == null) {
                Debug.LogWarning("Cannot add non geopositioned object!");
            }
            geoPositionedObjects.Add(obj);
            obj.transform.SetParent(transform);
            Vector3 pos = Vector3.zero;
            RepositionGeoObject(obj, pos);
        }

        public void RemoveGeoPositionedObject(GameObject obj) {
            geoPositionedObjects.Remove(obj);
        }

        public void EnableCamera(bool enable) {
            mapController.EnableCamera(enable);
        }
    }
}