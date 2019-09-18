using System;
using System.Collections.Generic;
using Assets.GoFindMap.Scripts;
using Assets.Modules.SimpleLogging;
using Assets.Scripts.Core;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Processing;
using UnityEngine;
using UnityEngine.UI;
using Logger = Assets.Modules.SimpleLogging.Logger;

public class FilterManager : MonoBehaviour {
    public Text maxDistLabel;
    public Controller controller;
    public UIManager uiManager;
    public Slider maxDistSlider;
    public InputField lowerInput;
    public InputField upperInput;

    private double maxDist;
    private int lowerBound;
    private int upperBound;

    private Logger logger;

    private FilterEngine filterEngine;

    private void Awake() {
        logger = LogManager.GetInstance().GetLogger(GetType());
        filterEngine = new FilterEngine();
    }


    public void HandleMaxDistChanged(float newValue) {
        maxDist = newValue;
        logger.Trace("Handling new value: "+newValue);
        DisplayMaxDist();
    }

    public void DisplayMaxDist() {
        maxDistLabel.text = string.Format("{0} m", UIManager.Round(maxDist*1000, 0)); // Disp in m, value in km
    }

    public void ChangeMaxDist(bool add = true) {
        maxDistSlider.value += (add ? 1f : -1f )* 10f / 1000f;//10m in km
        maxDistSlider.value = (float)Math.Round(maxDistSlider.value, 2, MidpointRounding.AwayFromZero);
        HandleMaxDistChanged(maxDistSlider.value);
    }

    private void GatherTimeRange() {
        lowerBound = defaultMinValue;
        upperBound = defaultMaxValue;
        if (!string.IsNullOrEmpty(lowerInput.text)) {
            try {
                lowerBound = int.Parse(lowerInput.text);
                logger.Debug("Lower Bound {0}(str), parsed: {1}", lowerInput.text, lowerBound);
            } catch (ArgumentException e) {
                // Silently ignore, since value is set smartyl
            } catch (FormatException ex) {
                // Silently ignore, since value is set smartly
            }
        }
        if (!string.IsNullOrEmpty(upperInput.text)) {
            try {
                upperBound = int.Parse(upperInput.text);
                logger.Debug("Upper Bound {0}(str), parsed: {1}", upperInput.text, upperBound);
            } catch (ArgumentException e) {
                // Silently ignoring
            } catch (FormatException ex) {
                // Silently ignoring
            }
        }
        logger.Debug("Gathered time range. From {0} to {1}", lowerBound, upperBound);
    }

    public void Apply() {
        GatherTimeRange();
        TemporalRangeFilter tf = new TemporalRangeFilter(lowerBound, upperBound);
        GeoLocation lastKnownLoc = controller.GetLastKnownLocation();
        SpatialMaxDistanceFilter sf = new SpatialMaxDistanceFilter(maxDist*1000, lastKnownLoc.latitude, lastKnownLoc.longitude);
        logger.Debug("Created filters:\nTime: {0}\nDist: {1}",tf,sf);
        filterEngine.AddFilterStrategy(tf);
        filterEngine.AddFilterStrategy(sf);

        List<MultimediaObject> filteredList = filterEngine.ApplyFilters(controller.GetOriginalList());

        logger.Debug("Filtered list, notifying ui...");
        controller.SetActiveList(filteredList);
        uiManager.panelManager.ShowPanel("choice");
    }

    public void Reset() {
        logger.Debug("Resetting choice and filters");
        lowerBound = defaultMinValue;
        upperBound = defaultMaxValue;
        maxDistSlider.value = defaultDist;
        
        controller.SetActiveList(controller.GetOriginalList());
    }

    public void Cancel() {
        lowerBound = defaultMinValue;
        upperBound = defaultMaxValue;
        maxDistSlider.value = defaultDist;
        uiManager.panelManager.ShowPanel("choice");
    }

    private float defaultDist;
    private const int defaultMinValue = 1;
    private const int defaultMaxValue = 9999;

    // Use this for initialization
    void Start() {
        HandleMaxDistChanged(maxDistSlider.value);
        defaultDist = maxDistSlider.value;
    }

    // Update is called once per frame
    void Update() { }
}