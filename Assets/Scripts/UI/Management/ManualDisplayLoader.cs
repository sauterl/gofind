using System.Collections;
using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using Assets.Scripts.Core;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using Logger = Assets.Modules.SimpleLogging.Logger;

public class ManualDisplayLoader : MonoBehaviour {

    public Text messageText;

    public UIManager uiManager;

    public LoaderPopupHandler loadPopupHandler;

    public Controller controller;

    private int index;

    private Logger logger;
    private void Awake() {
        logger = LogManager.GetInstance().GetLogger(GetType());
    }

    public void HandleInputUpdate(string input) {
        logger.Debug("Handling input with index  (str) "+index);
        this.index = int.Parse(input);

        if (controller.ExistsResultIndex(this.index)) {
            DisplayPositiveMsg(index);
        } else {
            DisplayNegativeMsg(this.index);
        }
    }

    public void HandleCancel() {
        loadPopupHandler.ShowPopup(false);
    }

    public void HandleOk() {
        logger.Debug("Handling OK button with index "+index);
        uiManager.Present(controller.GetMultimediaObject(index));
        loadPopupHandler.ShowPopup(false);
    }

    private void DisplayNegativeMsg(int index) {
        messageText.text = string.Format("Error:\nThere is no object with index {0}.\nTry another one or look the index up in the choice list.", index);
    }

    private void DisplayPositiveMsg(int index) {
        messageText.text =
            string.Format(
                "By hitting the OK button, the object with index {0} will be loaded and be available as an overlay.", index);
    }
}
