using Assets.GoFindMap.Scripts;
using Assets.Scripts.Core;
using Assets.Scripts.UI;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using UnityEngine.UI;

public class ResultLocationDisplay : MonoBehaviour {

    public Text infoLabel;
    public Text titleLabel;
    public ImagePresenter imagePresenter;
    public ToggleButton toggleButton;

    public Controller controller;
    public UIManager uiManager;

    private MultimediaObject active;

    void Start() {
        toggleButton.SetToggleHandler(HandleToggling);
    }

    public void Present(MultimediaObject mmo) {
        DisplayInfo(mmo);
        imagePresenter.LoadImage(CineastUtils.GetThumbnailUrl(mmo));
        active = mmo;
        titleLabel.text = "Result " + mmo.resultIndex;
    }

    private void DisplayInfo(MultimediaObject mmo) {
        string dist = uiManager.GetRoundedDist(new GeoLocation(mmo.latitude, mmo.longitude),
            controller.GetLastKnownLocation());
        infoLabel.text = string.Format("Dist: {0}m\nDate: {1}", dist, UIManager.FormatDate(mmo));
    }

    private void HandleToggling(bool toggled) {
        if (toggled) {
            controller.AddToActiveList(active);
        } else {
            controller.RemoveFromActiveList(active);
        }
    }
}
