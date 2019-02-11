using UnityEngine;

namespace Assets.Plugins.LocationAwarenessPlugin
{
    /**
     * The LocationAwarenessPlugin main class.
     * 
     * Differentiates per target build which plugin to use
     * */
    public class LocationAwarenessPlugin : ILocationAwarenessPlugin
    {
        private readonly ILocationAwarenessPlugin pluginImpl;

        private static LocationAwarenessPlugin instance = null;

        private LocationAwarenessPlugin()
        {
#if UNITY_ANDROID
            if (Application.isEditor)
            {
                pluginImpl = new MockLocationAwarenessPlugin();
            }
            else
            {
                pluginImpl = new AndroidLocationAwarenessPlugin();
            }
#else
            pluginImpl = new MockLocationAwarenessPlugin();
#endif
        }

        public static LocationAwarenessPlugin GetInstance()
        {
            if (instance == null)
            {
                instance = new LocationAwarenessPlugin();
            }
            return instance;
        }


        public void StartService(Configuration config, LocationTargetList targets)
        {
            pluginImpl.StartService(config, targets);
        }

        public void StopService()
        {
            pluginImpl.StopService();
        }
    }
}