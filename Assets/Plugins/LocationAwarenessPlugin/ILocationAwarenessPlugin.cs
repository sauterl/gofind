namespace Assets.Plugins.LocationAwarenessPlugin
{
    interface ILocationAwarenessPlugin
    {


        void StartService(Configuration config, LocationTargetList targets);

        void StopService();

    }
}