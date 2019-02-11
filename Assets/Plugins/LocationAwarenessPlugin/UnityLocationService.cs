using System;
using System.Collections;
using UnityEngine;

namespace Assets.Plugins.LocationAwarenessPlugin
{
    /**
     * Representation of a Location Service fully implemented in unity.
     * 
     * Needs to be attached to a game object!
     * */
    public class UnityLocationService : MonoBehaviour, ILocationAwarenessPlugin
    {
        public enum Status
        {
            INITIALIZING,
            RUNNING,
            STOPPED,
            TIMED_OUT,
            FAILED,
            PAUSED,
            UNKOWN
        }

        private Configuration config;

        private LocationService loc;

        private Action<LocationObject> locationFoundHandler;

        private LocationTargetList targetList;

        private bool timedOut;
        private bool paused;

        public void StartService(Configuration config, LocationTargetList targets)
        {
            if (locationFoundHandler == null)
            {
                Debug.LogWarning("Starting Location Service without target found handler. Consider invoking SetLocationHandler first!");
            }

            this.config = config;
            this.targetList = targets;

            StartCoroutine(StartLocationService(5f, config.minDistance));
        }

        public void StartSingleRequest(Action<LocationInfo> doneHandler)
        {
            switch (GetStatus())
            {
                case Status.INITIALIZING:
                    // ignore, wait until inizialitation is done
                    Debug.LogWarning("ULS:Init already ongoing");
                    StartCoroutine(AwaitInitialisationDone(doneHandler));
                    break;
                case Status.RUNNING:
                    // lastLocation is directly providable
                    Debug.Log("ULS:Already running");
                    doneHandler.Invoke(lastKnownLocation); // lastKnownLocation should be set correctly
                    break;
                case Status.STOPPED:
                    // (re-start)
                    StartCoroutine(StartLocationService(5f, 10f, doneHandler));
                    break;
                case Status.TIMED_OUT:
                    Debug.LogError("Cannot InitializeLocationProvider due to TIME_OUT");
                    break;
                case Status.FAILED:
                    Debug.LogError("Cannot InitializeLocationProvider due to FAILED");
                    break;
                case Status.PAUSED:
                    // TODO: What to do? restart and get last location?!
                    break;
                case Status.UNKOWN:
                    Debug.LogError("Cannot InitializeLocationProvider due to UNKOWN");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator AwaitInitialisationDone(Action<LocationInfo> doneHandler) {
            while (GetStatus() == Status.INITIALIZING) {
                yield return new WaitForSeconds(5);
            }

            if (GetStatus() == Status.RUNNING) {
                doneHandler.Invoke(GetLastKnownLocation());
                yield break;
            }

            if (GetStatus() == Status.STOPPED) {
                Debug.LogWarning("ULS: Loc stopped");
            } else {
                Debug.LogWarning("ULS: After init status is: "+GetStatus());
            }
        }


        private LocationInfo lastKnownLocation;

        /**
         * Caution! Nullcheck
         * */
        public LocationInfo GetLastKnownLocation()
        {
            return lastKnownLocation;
        }


        public void StopService()
        {
            if (loc != null)
            {
                if (GetStatus() == Status.RUNNING)
                {
                    loc.Stop();
                }
            }
        }

        public bool IsLocationServiceEnabled()
        {
            return loc.isEnabledByUser;
        }

        private void Awake()
        {
            loc = Input.location;

            if (!IsLocationServiceEnabled())
            {
                Debug.LogError("Locationservice not enabled");
            }
        }

        private void Update()
        {
            if (HasTargets() && GetStatus() == Status.RUNNING)
            {
                CheckIfCloseToAnyTarget();
            }
            if (GetStatus() == Status.RUNNING)
            {
                lastKnownLocation = loc.lastData;
            }
        }

        private void CheckIfCloseToAnyTarget()
        {
            foreach (var loc in targetList.targets)
            {
                if (Math.Abs(Utilities.HaversineDistance(this.loc.lastData, loc)) < config.closeDistance)
                {
                    NotifyHandler(loc);
                }
            }
        }

        public void SetLocationHandler(Action<LocationObject> handler)
        {
            locationFoundHandler = handler;
        }

        private void NotifyHandler(LocationObject obj)
        {
            if (locationFoundHandler != null)
            {
                locationFoundHandler.Invoke(obj);
            }
            else
            {
                Debug.LogWarning("No location handler set!");
            }
        }

        private bool HasTargets()
        {
            return targetList != null && targetList.targets.Length > 0;
        }

        private IEnumerator StartLocationService(float desiredAccuracy, float minDistance, Action<LocationInfo> doneHandler = null)
        {
            Debug.Log("ULS:StartLocationService");
            if (!IsLocationServiceEnabled())
            {
                Debug.LogError("ULS:No location service!");
                yield break;
            }


            loc.Start(desiredAccuracy, desiredAccuracy);
            yield return loc;
            Debug.Log("ULS:Started loc update request");

            var maxWaitSeconds = 30; // TODO find value
            while (loc.status == LocationServiceStatus.Initializing && maxWaitSeconds > 0)
            {
                Debug.Log("ULS: Waiting..." + maxWaitSeconds);
                yield return new WaitForSeconds(maxWaitSeconds--);
            }
            Debug.Log("ULS: Late init checks");
            if (loc.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("ULS:Failed initalize unity location service");
                yield break;
            }

            if (maxWaitSeconds <= 0)
            {
                timedOut = true;
                Debug.LogError("ULSLocation service initialization timed out.");
            }

            yield return lastKnownLocation = loc.lastData;
            Debug.Log("ULS:LastKnownLocation: "+lastKnownLocation.ToString());
            if (doneHandler != null)
            {
                Debug.Log("ULS:SingleRequest, send data back");
                doneHandler.Invoke(lastKnownLocation);
                loc.Stop();
                Debug.Log("ULS:Stopped after SingleRequest");
            }
        }

        public Status GetStatus()
        {
            if (loc == null)
            {
                return Status.STOPPED;
            }else if (timedOut)
            {
                return Status.TIMED_OUT;
            }else if (paused)
            {
                return Status.PAUSED;
            }
            switch (loc.status)
            {
                case LocationServiceStatus.Running:
                    return Status.RUNNING;
                case LocationServiceStatus.Failed:
                    return Status.FAILED;
                case LocationServiceStatus.Initializing:
                    return Status.INITIALIZING;
                case LocationServiceStatus.Stopped:
                    return Status.STOPPED;
                default:
                    return Status.UNKOWN;
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                if (GetStatus() == Status.RUNNING)
                {
                    StopService();
                    paused = true;
                }
                
            }
            else
            {
                if (GetStatus() == Status.PAUSED)
                {
                    StartService(config, targetList);
                }
            }
        }
        

    }
}