using System;
using System.Collections.Generic;
using Assets.GoFindMap.Scripts;
using Assets.Modules.SimpleLogging;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Query;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models.Messages.Query;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using Logger = Assets.Modules.SimpleLogging.Logger;

namespace Assets.Scripts.Core
{
    public class QueryBuilder : MonoBehaviour
    {
        public GameObject QbEDisplayPrefab;
        public GameObject QbLDisplayPrefab;
        public GameObject QbTDisplayPrefab;

        public Transform ScrollContent;

        public string MapControllerGameObjectName = "MapPlane";
        private MapController mapController;
        public string UIManagerGameObjectName = "MainCanvas";
        private UIManager uiManager;
        public string ExampleInputGameObjectName = "QueryImagePanel";
        private ExampleInput exampleInput;
        public string ExampleInputPanelName = "queryImage";
        public string ControllerGameObjectName = "GoFindARController";
        private Controller controller;
        
        private Logger logger;

        private TermsObject locationTerm;
        private TermsObject timeTerm;
        private TermsObject exampleTerm;

        // Unity constructor alike
        private void Awake()
        {
            logger = LogManager.GetInstance().GetLogger(GetType());
        }

        // Before the script starts
        private void Start()
        {
            mapController = UIManager.GetScriptComponentByName<MapController>(MapControllerGameObjectName);
            if (mapController == null)
            {
                logger.Warn("No map controller found. Given name: " + MapControllerGameObjectName);
            }

            uiManager = UIManager.GetScriptComponentByName<UIManager>(UIManagerGameObjectName);
            if (uiManager == null)
            {
                logger.Warn("No UIManager found. Given name: " + UIManagerGameObjectName);
            }

            exampleInput = UIManager.GetScriptComponentByName<ExampleInput>(ExampleInputGameObjectName);
            if (exampleInput == null)
            {
                logger.Warn("No example input found. Given name: " + ExampleInputGameObjectName);
            }

            controller = UIManager.GetScriptComponentByName<Controller>(ControllerGameObjectName);
            if (controller == null)
            {
                logger.Warn("No controller found. Given name: " + ControllerGameObjectName);
            }
        }

        private GameObject qbe;
        private GameObject qbl;
        private GameObject qbt;

        public void AddQbE()
        {
            exampleInput.PhotoCapturedHandler = ProcessExampleImage;
            uiManager.panelManager.ShowPanel(ExampleInputPanelName);
        }

        private string imageDataUrl;

        private void ProcessExampleImage(string dataurl)
        {
            imageDataUrl = dataurl;
            SetExampleTerm(dataurl);
            uiManager.panelManager.ShowLastActivePanel();
            AddQbEDisplay();
        }

        private GeoLocation location;


        public void ProcessLocationSelected() {
            uiManager.panelManager.ShowLastActivePanel();
            AddQbLDisplay();
        }
        
        private void ProcessMapLocation(GeoLocation loc)
        {
            location = loc;
            SetLocationTerm(loc.latitude,loc.longitude);
            //uiManager.panelManager.ShowLastActivePanel();
            //AddQbLDisplay();
        }

        private void AddQbEDisplay()
        {
            if (qbe != null)
            {
                RemoveQbE();
            }

            qbe = Instantiate(QbEDisplayPrefab);
            AttachToScroll(qbe);
            QbEDisplay disp = qbe.GetComponent<QbEDisplay>();
            if (disp != null)
            {
                disp.builder = this;
                disp.image.texture = ExampleInput.FromBase64DataUrl(imageDataUrl);
                // needs to respect the aspect ratio!
            }
        }

        public void RemoveQbE()
        {
            RemoveFromScroll(qbe);
            exampleTerm = null;
        }

        /// <summary>
        /// Adds the query-by-map-location display to the query terms list
        /// </summary>
        private void AddQbLDisplay()
        {
            if (qbl != null)
            {
                RemoveQbL();
            }

            qbl = Instantiate(QbLDisplayPrefab);
            AttachToScroll(qbl);
            QbLDisplay disp = qbl.GetComponent<QbLDisplay>();
            if (disp != null)
            {
                disp.builder = this;
                disp.title.text = "Map Location";
                disp.DisplayLocation(location.latitude,location.longitude);
            }
        }


        public void AddQbL()
        {
            uiManager.panelManager.ShowPanel("map");
            mapController.SetOnLocationSelectedHandler(ProcessMapLocation);
        }

        /// <summary>
        /// Adds the query-by-current-position display to the query terms list
        /// </summary>
        public void AddQbP()
        {
            if (qbl != null)
            {
                RemoveQbL();
            }

            qbl = Instantiate(QbLDisplayPrefab);
            AttachToScroll(qbl);
            QbLDisplay disp = qbl.GetComponent<QbLDisplay>();
            if (disp != null)
            {
                disp.title.text = "Current Position";
                disp.builder = this;
                disp.DisplayLocation(GetCurrentLatitude(), GetCurrentLongitude());
                SetLocationTerm(GetCurrentLatitude(), GetCurrentLongitude());
            }
        }

        private void SetLocationTerm(double lat, double lon)
        {
            locationTerm = QueryFactory.BuildLocationTerm(lat, lon);
        }

        private void SetTimeTerm(int lower, int upper)
        {
            timeTerm = QueryFactory.BuildTimeTerm(CineastUtils.ConvertYearToISO8601((lower + upper) / 2));
        }

        private void SetExampleTerm(string data)
        {
            exampleTerm = QueryFactory.BuildQbETerm(data);
        }

        private double GetCurrentLatitude()
        {
            return mapController != null ? mapController.GetLatitude() : double.NaN;
        }

        private double GetCurrentLongitude()
        {
            return mapController != null ? mapController.GetLongitude() : double.NaN;
        }

        public void SendQueryRequest()
        {
            CollectTimeTerm();

            List<TermsObject> terms = new List<TermsObject>();
            if (locationTerm != null)
            {
                terms.Add(locationTerm);
            }

            if (exampleTerm != null)
            {
                terms.Add(exampleTerm);
            }

            if (timeTerm != null)
            {
                terms.Add(timeTerm);
            }

            SimilarQuery query = QueryFactory.BuildMultiTermQuery(terms.ToArray());
            controller.GoMultiTermQuery(query);
        }

        private void CollectTimeTerm()
        {
            if (qbt == null)
            {
                return; // If no time ui element available - surely no time term exists
            }

            QbTDisplay disp = qbt.GetComponent<QbTDisplay>();
            if (disp != null)
            {
                var lowerContent = disp.lowerBoundInput.text;
                var upperContent = disp.upperBoundInput.text;

                if (!string.IsNullOrEmpty(lowerContent))
                {
                    int lower = int.Parse(lowerContent);
                    int upper = !string.IsNullOrEmpty(upperContent)
                        ? int.Parse(upperContent)
                        : DateTime.Now.Year + 1; // the +1 is so, that the current year fully appears in the query.

                    controller.SetTemporalRange(lower, upper);
                    SetTimeTerm(lower, upper);
                }
            }
        }

        public void AddQbT()
        {
            if (qbt != null)
            {
                RemoveQbT();
            }

            qbt = Instantiate(QbTDisplayPrefab);
            AttachToScroll(qbt);
            QbTDisplay disp = qbt.GetComponent<QbTDisplay>();
            if (disp != null)
            {
                disp.builder = this;
            }
        }

        public void RemoveQbT()
        {
            RemoveFromScroll(qbt);
            timeTerm = null;
        }

        public void RemoveQbL()
        {
            RemoveFromScroll(qbl);
            locationTerm = null;
        }

        private void AttachToScroll(GameObject obj)
        {
            obj.transform.SetParent(ScrollContent, false);
        }

        private void RemoveFromScroll(GameObject obj)
        {
            // obj.transform.SetParent(null);
            Destroy(obj);
        }
    }
}