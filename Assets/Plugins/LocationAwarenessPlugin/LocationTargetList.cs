using System;

namespace Assets.Plugins.LocationAwarenessPlugin
{
    /**
     * Class to pass a list of targets to the native part of the plugin.
     *
     */

    [System.Serializable]
    public class LocationTargetList
    {

        public LocationTargetList()
        {

        }
        /**
         * Constructrs a new LocationTargetList with the specified targets and the current time.
         * */
        public LocationTargetList(LocationObject[] targets)
        {
            timestamp = GetCurrentTimestamp();
            this.targets = targets;
        }

        /**
         * Returns the current timestamp.
         * */
        private double GetCurrentTimestamp()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public double timestamp; //ms since 1970/01/01@00:00:00 unix timestamp
        public LocationObject[] targets;
    }
}