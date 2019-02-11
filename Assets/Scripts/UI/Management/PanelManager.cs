using System;
using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using UnityEngine;
using Logger = Assets.Modules.SimpleLogging.Logger;

public class PanelManager : MonoBehaviour {
    private Panel active;

    private Dictionary<string, Panel> panels;
    private Panel lastActive;
    
    
    private Logger logger;

    private void Awake() {
        logger = LogManager.GetInstance().GetLogger(GetType());
        panels = new Dictionary<string, Panel>();
    }


    public Panel GetActive() {
        return active;
    }

    public void RegisterAll(Panel[] panels) {
        foreach (Panel p in panels) {
            RegisterPanel(p);
        }
    }

    public void RegisterPanel(Panel panel, bool initiallyActive =false) {
        if (panel.name == null) {
            throw new ArgumentException("Cannot accept null-named panels");
        }
        if (panel.obj == null) {
            throw new ArgumentException("Cannot accept panels with obj == null (Name: " + panel.name + ")");
        }
        logger.Debug("Adding panel with panelname={0} and gameobject name={1}", panel.name, panel.obj.name);
        panels.Add(panel.name, panel);
        if (initiallyActive) {
            active = panel;
        }
    }
    
    public void SetInitial(Panel p) {
        active = p;
    }

    public void UnregisterPanel(Panel panel) {
        logger.Debug("Unregistering panel with name " + panel.name);
        panels.Remove(panel.name);
    }

    public void ShowNext() {
        if (active.next != null) {
            logger.Debug("Showing next panel");
            ShowPanel(active.next);
        }
    }

    public void ShowPrevious() {
        if (active.previous != null) {
            logger.Debug("Showing previous panel");
            ShowPanel(active.previous);
        }
    }

    public void ShowPanel(Panel p) {
        logger.Debug("Showing panel "+p.name);
        
        logger.Debug("Last Active panel: "+ (active != null ? active.name : "N/A"));
        lastActive = active;
        
        // Call active handler
        logger.Debug("Going to call onVisibilityChanged(false) on "+(active != null ? active.name : "N/A"));
        if (active != null) {
            GetActive().OnVisibilityChanged(false);
        }

        DeactivateAll(); // Mainly because at the beginning are all activated

        SetVisible(p, true);
        logger.Debug("Going to call onVisibilityChanged(true) on "+p.name);
        p.OnVisibilityChanged(true);
        active = p;
    }

    /// <summary>
    /// Shows the panel that was active before this one.
    /// This might be handy, if the 'previous' of a panel is kind of context sensitive
    /// </summary>
    public void ShowLastActivePanel() {
        ShowPanel(lastActive);
    }

    public void ShowPanel(string name) {
        if (panels.ContainsKey(name)) {
            //DeactivateAll(); Code dupe -> L101
            Panel p;
            panels.TryGetValue(name, out p);
            ShowPanel(p);
        }
    }

    private void DeactivateAll() {
        logger.Debug("Deactivating all panels");
        foreach (Panel panel in panels.Values) {
            SetVisible(panel, false);
        }
    }


    private void SetVisible(Panel p, bool visible) {
        if (p.obj != null) {
            logger.Debug("Setting "+p.name+"'s visibility to "+visible);
            p.obj.SetActive(visible);
        }
    }

    public class Panel {
        public string name;
        public Panel next;
        public GameObject obj;
        public Panel previous;
        public Action<bool> visibilityChangedHandler = null;

        public Panel(string name, GameObject obj) {
            this.name = name;
            this.obj = obj;
        }

        public Panel(string name, GameObject obj, Panel prev, Panel next) {
            this.name = name;
            this.obj = obj;
            previous = prev;
            this.next = next;
        }

        public void OnVisibilityChanged(bool visible) {
            if (visibilityChangedHandler != null) {
                visibilityChangedHandler.Invoke(visible);
            }
        }
    }
}