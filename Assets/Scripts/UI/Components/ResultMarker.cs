using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using UnityEngine;

namespace Assets.Scripts.UI {
    public class ResultMarker : MonoBehaviour {

        public MultimediaObject result;

        public UIManager uiManager;

        public void HandleClicked() {
            Debug.Log("Clicked and showing"+ result.resultIndex);
            uiManager.ShowResultPanel(result);
        }
    }
}
