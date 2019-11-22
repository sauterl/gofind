using Assets.Modules.SimpleLogging;
using Assets.Scripts.Core;
using UnityEngine;
using Logger = Assets.Modules.SimpleLogging.Logger;

namespace Assets.Scripts.UI {
    public class LoaderPopupHandler : MonoBehaviour {

        public GameObject popupPanel;

        private bool popupShown = false;

        private Logger logger;

        void Awake() {
            logger = LogManager.GetInstance().GetLogger(GetType());
        }

        // Use this for initialization
        void Start () {
		    ShowPopup(false);
        }
	
        public void ShowPopup(bool show) {
            popupShown = show;
            if (popupShown) {
                popupPanel.SetActive(true);
            } else {
                popupPanel.SetActive(false); // TODO Investigate why Null
            }
            logger.Debug("Set popup to {0}", popupShown ? "active" : "inactive");
        }

        public void Popup() {
            ShowPopup(!popupShown);
        }

    }
}
