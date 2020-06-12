using System;
using Assets.Scripts.Core;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
  public class SettingsDialog : MonoBehaviour
  {

    private InputField cineastInput;
    private InputField imagesInput;

    private void Awake()
    {
      cineastInput = GameObject.Find("CineastInputField").GetComponent<InputField>();
      imagesInput = GameObject.Find("ImagesInputField").GetComponent<InputField>();

      var okBtn = transform.Find("BottomContainer/OkButton").gameObject.GetComponent<Button>();
      var cancelBtn = transform.Find("BottomContainer/CancelButton").gameObject.GetComponent<Button>();

      okBtn.onClick.RemoveAllListeners();
      cancelBtn.onClick.RemoveAllListeners();
      okBtn.onClick.AddListener(Store);
      cancelBtn.onClick.AddListener(Cancel);
    }

    public void Init()
    {
      cineastInput.text = CineastUtils.Configuration.cineastHost;
      imagesInput.text = CineastUtils.Configuration.imagesHost;
    }

    public void Store()
    {
      // TODO Regex for real ip check
      CineastUtils.Configuration.cineastHost = cineastInput.text;
      CineastUtils.Configuration.imagesHost = imagesInput.text;
      CineastUtils.Configuration.Store();
      
      UIManager.Instance.panelManager.ShowNext();
    }

    public void Cancel()
    {
      Init();
      UIManager.Instance.panelManager.ShowPrevious();
    }

  }
}