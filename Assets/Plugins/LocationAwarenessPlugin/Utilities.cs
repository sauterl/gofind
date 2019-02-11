using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.Plugins.LocationAwarenessPlugin
{
    public class Utilities
    {
        private Utilities()
        {
            // no instance needed
        }

        public static double DegreesToRadians(double angle)
        {
            return angle * (Math.PI / 180.0);
        }

        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3;
            var phi1 = DegreesToRadians(lat1);
            var phi2 = DegreesToRadians(lat2);
            var deltaPhi = DegreesToRadians(lat2 - lat1);
            var deltaLambda = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(deltaPhi / 2.0) * Math.Sin(deltaPhi / 2.0) + Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2.0) * Math.Sin(deltaLambda / 2.0);
            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;

        }

        public static double HaversineDistance(LocationInfo loc1, LocationInfo loc2)
        {
            return HaversineDistance(loc1.latitude, loc1.longitude, loc2.latitude, loc2.longitude);
        }

        public static double HaversineDistance(LocationInfo loc1, LocationObject loc2)
        {
            return HaversineDistance(loc1.latitude, loc1.longitude, loc2.latitude, loc2.longitude);
        }

        private const string SAVE_FILE = "las-tl.data";


        public static void StoreData(LocationTargetList list) 
        {
            Debug.Log(":StoreData");
            var bf = new BinaryFormatter();
            var file = File.Create(Application.persistentDataPath + "/" + SAVE_FILE);
            bf.Serialize(file, list);
            file.Close();
            Debug.Log("Stored Data");
        }

        public static LocationTargetList LoadData()
        {
            Assets.Plugins.LocationAwarenessPlugin.LocationTargetList list = null;
            Debug.Log(":ReadData");
            if (File.Exists(Application.persistentDataPath + "/" + SAVE_FILE))
            {
                Debug.Log("Datafile existent");
                var bf = new BinaryFormatter();
                var file = File.Open(Application.persistentDataPath + "/" + SAVE_FILE, FileMode.Open);
                list = bf.Deserialize(file) as LocationTargetList;
                file.Close();
                File.Delete(Application.persistentDataPath + "/" + SAVE_FILE); // Delete if loaded
                Debug.Log("Datafile read and deleted");
            }
            return list;
        }

        public static bool CanLoadData()
        {
            return File.Exists(Application.persistentDataPath + "/" + SAVE_FILE);
        }
    }
}