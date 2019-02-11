using System;
using UnityEngine;

namespace DefaultNamespace.GoFindScripts {
    public class GeoArithmetic {


        public struct GeoCoordinates {
            public double latitude;
            public double longitude;

            public GeoCoordinates(double latitude, double longitude) {
                this.latitude = latitude;
                this.longitude = longitude;
            }
            
        }

        public static GeoCoordinates CalculateOffset(GeoCoordinates origin, GeoCoordinates other) {
            double latOff = other.latitude - origin.latitude;
            double lonOff = other.longitude - origin.longitude;
            return new GeoCoordinates(latOff, lonOff);
        }

        public static double DegToRad(double d) {
            return d * (Math.PI / 180d);
        }

        public static double RadToDeg(double d) {

            return d * (180d / Math.PI);
        }

        public static GeoCoordinates ConvertToRadians(GeoCoordinates coords) {
            return new GeoCoordinates(DegToRad(coords.latitude), DegToRad(coords.longitude));
        }

        public static GeoCoordinates ConvertToDegrees(GeoCoordinates coords) {
            return new GeoCoordinates(RadToDeg(coords.latitude), RadToDeg(coords.longitude));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param> in degrees
        /// <param name="bearing"></param> in degrees from north, clockwise
        /// <param name="distance"></param> in meters
        /// <returns></returns>
        public static GeoCoordinates CalculateDestination(GeoCoordinates src, double bearing, double distance) {
            double R = 6.371E6;
            src = ConvertToRadians(src);
            bearing = DegToRad(bearing);
            double latDest = Math.Asin(Math.Sin(src.latitude) * Math.Cos(distance / R) +
                                       Math.Cos(src.latitude) * Math.Sin(distance / R) * Math.Cos(bearing));
            double lonDest = src.longitude + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance/R) * Math.Cos(src.latitude), Math.Cos(distance/R)-Math.Sin(src.latitude)*Math.Sin(latDest));
            double finalBearing = RadToDeg((bearing + 180d) % 360d);
            return ConvertToDegrees(new GeoCoordinates(latDest, lonDest));
        }
    }
}