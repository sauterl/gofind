using System;
using UnityEngine;

namespace Assets.GoFindMap.Scripts
{
    [System.Serializable]
    public class GeoLocation
    {
        public double latitude;
        public double longitude;

        public GeoLocation()
        {
            latitude = double.NaN;
            longitude = double.NaN;
        }

        public GeoLocation(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public override string ToString()
        {
            return string.Format("GeoLocation [latitude={0}, longitude={1}]", latitude, longitude);
        }

        public static GeoLocation FromLocationInfo(LocationInfo loc)
        {
            return new GeoLocation(loc.latitude, loc.longitude);
        }
    }
}