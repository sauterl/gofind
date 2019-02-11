namespace Assets.Plugins.LocationAwarenessPlugin
{
    /**
    * Configuration for native location service.
    */
    [System.Serializable]
    public class Configuration
    {
        /**
         * The minimal distance between updates.
         * in meters
         * */
        public float minDistance;

        /**
         * The distance to be considered as 'close'.
         * In meters.
         * Less than 10 meters is not encouraged
         * */
        public float closeDistance;

        /**
         * The minimal time between updates.
         * In milliseconds
         **/
        public long minTime;

        public Configuration()
        {
            
        }

        public Configuration(float minDistance, float closeDistance, long minTime)
        {
            this.minDistance = minDistance;
            this.closeDistance = closeDistance;
            this.minTime = minTime;
        }



    }
}