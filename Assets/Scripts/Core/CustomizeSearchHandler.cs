using System;
using Assets.GoFindMap.Scripts;
using Assets.Modules.SimpleLogging;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models.Messages.Query;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using UnityEngine.UI;
using Logger = Assets.Modules.SimpleLogging.Logger;

namespace Assets.Scripts.Core
{
    [Obsolete("Due to recent refactoring, this class is no longer needed, as the QueryBuilder now is repsonsible for customized search")]
    public class CustomizeSearchHandler : MonoBehaviour
    {

        public Text locLabel;
        public InputField startInput;
        public InputField endInput;

        public MaxDistanceLabel maxDistProvider;
        public Controller controller;

        public GeoLocation TargetLocation { get; set; }

        private Logger logger;

        private void Awake() {
            logger = LogManager.GetInstance().GetLogger(GetType());
            logger.Debug("Awake");
        }

        private bool initialLoc = true;

        public void SetGeoLocation(GeoLocation loc) {
            logger.Debug("Setting target location {0}" + loc);            
            TargetLocation = loc;
            SetLocationLabel(TargetLocation);
        }

        void Start()
        {
            //SetGeoLocation(controller.GetInitialGeoLocation() );
            Invoke("InitLocationLabel", 5);
        }

        private void InitLocationLabel()
        {
            SetGeoLocation(controller.GetInitialGeoLocation());
        }

        private void SetLocationLabel(GeoLocation loc) {
            logger.Debug("Set location label :"+loc);
            initialLoc = false;
            locLabel.text = string.Format("{0},{1}", loc.latitude, loc.longitude);
        }

        public void CreateAndPassQuery()
        {
            logger.Debug("CreateAndPassQuery");
            controller.uiManager.panelManager.ShowPanel("waiting");
            SimilarQuery spatialQuery = null;
            if (TargetLocation != null)
            {
                logger.Debug("Found spatial input, building spatial query");
                spatialQuery = QueryFactory.BuildSpatialSimilarQuery(TargetLocation.latitude, TargetLocation.longitude);
            }
            SimilarQuery temporalQuery = null;

            logger.Debug("Start and End time input s:{0}, e:{1}",startInput.text, endInput.text);

            if (!string.IsNullOrEmpty(startInput.text))
            {
                int lower = int.Parse(startInput.text);
                int upper = !string.IsNullOrEmpty(endInput.text) ? int.Parse(endInput.text) : 9990;

                controller.SetTemporalRange(lower, upper);

                logger.Debug("Found at least start input, building termporal query");
                temporalQuery = QueryFactory.BuildTemporalSimilarQuery(ConvertYearToISO8601((lower+upper)/2));

                /*if (spatialQuery != null)
                {
                    temporalQuery.With(spatialQuery);
                }*/
            }

            

            logger.Debug("Sending query to controller");
            if (temporalQuery != null)
            {
                controller.GoQuery(temporalQuery);
            }
            else
            {
                if (maxDistProvider.IsModified())
                {
                    controller.SetMaxDistance(maxDistProvider.GetMaxDistance());
                }
                controller.GoQuery(spatialQuery);
            }
            
        }

        public static string ConvertYearToISO8601(int year)
        {
            return "" + year + "-01-01T12:00:00Z"; // year-month-day[THH:MM:SSZ]
        }
    }
}
