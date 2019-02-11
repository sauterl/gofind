using System.Diagnostics;

namespace Assets.Plugins.LocationAwarenessPlugin
{
    public class MockLocationAwarenessPlugin : ILocationAwarenessPlugin
    {
        public void StartService(Configuration config, LocationTargetList targets)
        {
            UnityEngine.Debug.Log(string.Format("Call of `StartService` with config:\n{0}\nand targets:\n{1}", UnityEngine.JsonUtility.ToJson(config), UnityEngine.JsonUtility.ToJson(targets)));
        }

        public void StopService()
        {
            UnityEngine.Debug.Log("Call of `StopService`");
        }
    }
}