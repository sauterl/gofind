using System;
using UnityEngine;

/**
 * Simple script to attach to a canvas.
 * Needs three children: "HomePanel", "ChoicePanel" and "DisplayPanel" (preferably Panels, but works with anything else (I guess))
 * Then one can switch between the panels
 * */
[Obsolete("This class got replaced by the more sophisticated PanelManager")]
public class PanelSwitcher : MonoBehaviour
{
    public enum Panel
    {
        HOME,
        CHOICE,
        DISPLAY,
        MAP
    }

    public GameObject choicePanel;
    public GameObject displayPanel;

    public GameObject homePanel;

    public GameObject mapPanel;


    private void Awake()
    {
        homePanel = transform.Find("HomePanel").gameObject;
        choicePanel = transform.Find("ChoicePanel").gameObject;
        displayPanel = transform.Find("DisplayImagePanel").gameObject; // TODO build display panel (camimage, slider, cam on off btn
    }

    public void SwitchTo(Panel p)
    {
        switch (p)
        {
            case Panel.HOME:
                SetActiveIfNotNull(homePanel, true);
                SetActiveIfNotNull(choicePanel, false);
                SetActiveIfNotNull(displayPanel, false);
                SetActiveIfNotNull(mapPanel, false);
                break;
            case Panel.CHOICE:
                SetActiveIfNotNull(homePanel, false);
                SetActiveIfNotNull(choicePanel, true);
                SetActiveIfNotNull(displayPanel, false);
                SetActiveIfNotNull(mapPanel, true);
                break;
            case Panel.DISPLAY:
                SetActiveIfNotNull(homePanel, false);
                SetActiveIfNotNull(choicePanel, false);
                SetActiveIfNotNull(displayPanel, true);
                SetActiveIfNotNull(mapPanel, false);
                break;
            case Panel.MAP:
                SetActiveIfNotNull(homePanel, false);
                SetActiveIfNotNull(choicePanel, false);
                SetActiveIfNotNull(displayPanel, false);
                SetActiveIfNotNull(mapPanel, true);
                break;
        }
    }

    public void SwitchToHome()
    {
        SwitchTo(Panel.HOME);
    }

    public void SwitchToChoice()
    {
        SwitchTo(Panel.CHOICE);
    }

    public void SwitchToDisplay()
    {
        SwitchTo(Panel.DISPLAY);
    }

    public void SwitchToMap()
    {
        SwitchTo(Panel.MAP);
    }

    private void SetActiveIfNotNull(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }
}