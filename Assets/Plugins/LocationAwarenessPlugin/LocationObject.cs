
namespace Assets.Plugins.LocationAwarenessPlugin
{
    /**
     * 
     * A class representing location objects.
     * */
    [System.Serializable]
    public class LocationObject
    {
        public LocationObject()
        {

        }

        public LocationObject(string id, double latitude, double longitude)
        {
            this.id = id;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public double latitude;
        public double longitude;

        public string id;
    }
}