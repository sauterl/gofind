using UnityEngine;

namespace Assets.Plugins.LocationAwarenessPlugin
{
    class AndroidLocationAwarenessPlugin :ILocationAwarenessPlugin
    {

        private AndroidJavaClass _class;
        private AndroidJavaObject instance;

        public AndroidLocationAwarenessPlugin()
        {
            Debug.Log("LASPlugin: Android ctor");
            _class = new AndroidJavaClass("ch.unibas.dmi.dbis.las.LocationAwarenessPlugin");
            Debug.Log("LASPlugin - Found class");
            _class.CallStatic("init");
            Debug.Log("LASPlugin - Init called");
            instance = _class.GetStatic<AndroidJavaObject>("instance");
        }

        public void StartService(Configuration config, LocationTargetList targets)
        {
            instance.Call("startService", UnityEngine.JsonUtility.ToJson(config), UnityEngine.JsonUtility.ToJson(targets));
        }

        public void StopService()
        {
            instance.Call("stopService");
        }
    }
}