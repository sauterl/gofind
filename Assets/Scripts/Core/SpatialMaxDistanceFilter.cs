using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using Assets.Plugins.LocationAwarenessPlugin;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Processing;

namespace Assets.Scripts.Core {
    public class SpatialMaxDistanceFilter : FilterStrategy{

        private double maxDist = double.MaxValue;

        private double tarLAT;
        private double tarLON;

        private Logger logger = LogManager.GetInstance().GetLogger(typeof(SpatialMaxDistanceFilter));

        public SpatialMaxDistanceFilter(double maxDist, double lat, double lon) {
            this.maxDist = maxDist;
            tarLAT = lat;
            tarLON = lon;
            logger.Debug("Created with maxDist={0}, loc={1},{2}",maxDist,tarLAT,tarLON);
        }

        

        public List<MultimediaObject> applyFilter(List<MultimediaObject> list) {
            List<MultimediaObject> ret = new List<MultimediaObject>();
            foreach (MultimediaObject mmo in list) {
                var dist = Utilities.HaversineDistance(tarLAT, tarLON, mmo.latitude, mmo.longitude);
                logger.Debug("Currently checking MultimediaObject (index:{0}): {1}m", mmo.resultIndex, dist);
                if (dist <= maxDist) {
                    logger.Debug("Adding MultimediaObject with index: "+mmo.resultIndex);
                    ret.Add(mmo);
                }
            }
            return ret;
        }

        public override string ToString() {
            return string.Format("SpatialMaxDistanceFilter [maxDist={0}, tarLAT={1}, tarLON={2}]",maxDist,tarLAT,tarLON);
        }
    }
}