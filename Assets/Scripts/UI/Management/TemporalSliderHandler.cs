using System;
using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using Assets.Scripts.Core;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using UnityEngine;
using UnityEngine.UI;
using Logger = Assets.Modules.SimpleLogging.Logger;

namespace Assets.Scripts.UI.Management {
    public class TemporalSliderHandler : MonoBehaviour {
        public Controller controller;

        private readonly Dictionary<int, MultimediaObject> dict = new Dictionary<int, MultimediaObject>();

        private Logger logger;

        public Text lowerLabel;

        public Slider slider;
        public UIManager uiManger;
        public Text upperLabel;

        private void Awake() {
            logger = LogManager.GetInstance().GetLogger(GetType());
        }


        public void HandleSliding(float value) {
            logger.Debug("Handling value: " + value);
            if (!setupFinished) {
                logger.Debug("Setup not finished, returning");
                return;
            }
            
            int index = GetClosestIndex(value);
            int key = new List<int>(dict.Keys)[index];
            logger.Debug("Found key:"+key);
            if (dict.ContainsKey(key)) {
                logger.Debug("Current index: {0}, new index: {1}",currentIndex,index);
                if (currentIndex != index) {
                    logger.Debug("Going to present new");
                    uiManger.Present(dict[key], false);
                    currentIndex = index;
                    SetSilderValue(currentIndex);  
                }
                
            } else {
                logger.Debug("Was not in dict");
            }
        }

        private int currentIndex = 0;

        private void SetSilderValue(int index) {
            slider.value = (float)index / (float)dict.Count;
        }

        private int GetClosestIndex(float value) {
            logger.Debug("Calculating closest for "+value);
            /*
             * slider 0 to 1
             * multiply by #images, round to integer and get key via index
             * */
            List<int> keys = new List<int>(dict.Keys);
            logger.Debug("keys: "+DumpList(keys));
            //keys.Sort((k,l)=>k-l);
            float fIndex = value * keys.Count;
            int index = (int) fIndex;
            logger.Debug("Returning index: "+index);
            return index;
        }

        private string DumpList(List<int> list) {
            string ret = "";
            foreach (int i in list) {
                ret += "" + i+",";
            }
            return ret;
        }

        private bool setupFinished = false;

        public void Setup(List<MultimediaObject> mmos, MultimediaObject active) {
            logger.Debug("Setup");
            setupFinished = false;
            dict.Clear();

            foreach (MultimediaObject mmo in mmos) {
                DateTime dt = DateTime.Parse(mmo.datetime); // mmo.dateitme is null, TODO investigate
                int key = dt.Year*10; // times ten to have multiple entries per year (10)
                while (dict.ContainsKey(key)) {
                    logger.Debug("Key {0} already used. decrease key by one", key, key-1);
                    key--;
                }
                dict.Add(key, mmo);
                logger.Debug("Added {0}/{1} to dict (origin year: {2})", key, mmo.id, dt.Year);
            }
            List<int> keys = new List<int>(dict.Keys);
            keys.Sort((i, j) => i - j); // basic integer sorting
            int min = keys[0];
            int max = keys[keys.Count - 1];
            logger.Debug("Range: {0} to {1}", min, max);
            lowerLabel.text = "" + min/10;//dont disp on label
            upperLabel.text = "" + max/10;//dont disp on label
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
            DateTime adt = DateTime.Parse(active.datetime);
            slider.value = keys.IndexOf(adt.Year * 10);
            logger.Debug("Slider should:({0},{1},{2})", min, adt.Year*10, max);
            logger.Debug("Slider is:({0},{1},{2})", slider.minValue, slider.value, slider.maxValue);
            logger.Debug("Setup finished!");
            setupFinished = true;
        }
    }
}