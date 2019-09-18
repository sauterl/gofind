using System.Collections.Generic;
using Assets.Plugins.LocationAwarenessPlugin;
using Assets.Scripts.Core;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;

namespace Assets.Scripts.IO
{
    [System.Serializable]
    public class SaveState
    {
        // Ensure that everything in this class is serializable
        public LocationTargetList targetList = null;

        public Controller.State state = Controller.State.UNKOWN;

        public MultimediaObject active = null;

        public List<MultimediaObject> activeMmos = null;



    }
}