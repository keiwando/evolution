using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Keiwando.UI {

  public enum SettingControlType {
    Toggle,
    Slider,
    Button,
    Input,
    Multiselect
  }

  public struct SettingControl {
    public SettingControlType type;
    public string name;
    public bool toggleValue;
    public Action<bool> onToggleValueChanged;
    public float sliderValue;
    public Action<float> onSliderValueChanged;
    public string inputValue;
    public Action<string> onInputValueChanged;
    public string[] multiselectNames;
    public Action<int> onMultiselectIndexChanged;
    public int multiselectSelectedIndex;

    public string tooltip;
  }

  public struct SettingControlGroup {
    public string name;
    public SettingControl[] controls;
  }

  public class SettingsView: MonoBehaviour {

    public SettingControlGroup[] controlGroups;

    [SerializeField] private SettingsTab tabTemplate;
    [SerializeField] private SettingsToggleControlCell toggleCellTemplate;
    [SerializeField] private SettingsSliderControlCell sliderCellTemplate;
    [SerializeField] private SettingsButtonControlCell buttonCellTemplate;
    [SerializeField] private SettingsInputControlCell inputCellTemplate;
    [SerializeField] private SettingsMultiselectControlCell multiselectCellTemplate;

    private SettingsTab[] tabs;
    private int selectedTabIndex = 0;

    struct AnySettingCell {
      public SettingControlType type;
      public SettingsToggleControlCell toggleControl;
      public SettingsSliderControlCell sliderControl;
      public SettingsButtonControlCell buttonControl;
      public SettingsInputControlCell inputControl;
      public SettingsMultiselectControlCell multiselectControl;

      public GameObject gameObject {
        get { 
          switch (type) {
            case SettingControlType.Toggle:
              return toggleControl.gameObject;
            case SettingControlType.Slider:
              return sliderControl.gameObject;
            case SettingControlType.Button:
              return buttonControl.gameObject;
            case SettingControlType.Input:
              return inputControl.gameObject;
            case SettingControlType.Multiselect:
              return multiselectControl.gameObject;
            default:
              return null;
          }
        }
      }
    }
    private AnySettingCell[][] settingCellsPerTab;

    void Start() {

      toggleCellTemplate.gameObject.SetActive(false);
      sliderCellTemplate.gameObject.SetActive(false);
      buttonCellTemplate.gameObject.SetActive(false);
      inputCellTemplate.gameObject.SetActive(false);
      multiselectCellTemplate.gameObject.SetActive(false);
    }

    public void Refresh() {
      if (this.tabs == null) {
        return;
      }
      for (int tabIndex = 0; tabIndex < this.tabs.Length; tabIndex++) {
        bool isSelectedTab = tabIndex == selectedTabIndex;
        SettingsTab tab = this.tabs[tabIndex];
        tab.SetSelected(isSelectedTab);
        foreach (AnySettingCell cell in settingCellsPerTab[tabIndex]) {
          cell.gameObject.SetActive(isSelectedTab);
        }
      }
    }

    public void SetupControls() {
      if (controlGroups == null) {
        Debug.LogWarning("controlGroups must be set before SetupControls is called!");
        return;
      }

      this.tabs = new SettingsTab[controlGroups.Length];
      this.settingCellsPerTab = new AnySettingCell[this.tabs.Length][];

      for (int i = 0; i < controlGroups.Length; i++) {
        SettingControlGroup group = controlGroups[i];
        SettingsTab tab = Instantiate(tabTemplate.gameObject, tabTemplate.transform.parent).GetComponent<SettingsTab>();
        tabs[i] = tab;

        this.settingCellsPerTab[i] = new AnySettingCell[group.controls.Length];
        for (int controlIndex = 0; controlIndex < group.controls.Length; controlIndex++) {
          SettingControl control = group.controls[controlIndex];
          AnySettingCell cell = createSettingCellForControl(control);
          this.settingCellsPerTab[i][controlIndex] = cell;
        }
      }

      Refresh();
    }

    private AnySettingCell createSettingCellForControl(SettingControl control) {
      switch (control.type) {
        case SettingControlType.Toggle: {
          SettingsToggleControlCell cell = Instantiate(toggleCellTemplate.gameObject, toggleCellTemplate.transform.parent).GetComponent<SettingsToggleControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.toggle.onValueChanged.AddListener(delegate (bool isOn) {
            if (control.onToggleValueChanged != null) {
              control.onToggleValueChanged(isOn);
            }
            Refresh();
          });
          return new AnySettingCell { toggleControl = cell };
        }
        case SettingControlType.Slider: {
          SettingsSliderControlCell cell = Instantiate(sliderCellTemplate.gameObject, sliderCellTemplate.transform.parent).GetComponent<SettingsSliderControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.slider.onValueChanged.AddListener(delegate (float value) {
            if (control.onSliderValueChanged != null) {
              control.onSliderValueChanged(value);
            }
            Refresh();
          });
          return new AnySettingCell { sliderControl = cell };
        }
        case SettingControlType.Button: {
          SettingsButtonControlCell cell = Instantiate(buttonCellTemplate.gameObject, buttonCellTemplate.transform.parent).GetComponent<SettingsButtonControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.button.onClick.AddListener(delegate () {
            if (control.onInputValueChanged != null) {
              control.onInputValueChanged(control.inputValue);
            }
            Refresh();
          });
          return new AnySettingCell { buttonControl = cell };
        }
        case SettingControlType.Input: {
          SettingsInputControlCell cell = Instantiate(inputCellTemplate.gameObject, inputCellTemplate.transform.parent).GetComponent<SettingsInputControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.inputField.onEndEdit.AddListener(delegate (string value) {
            if (control.onInputValueChanged != null) {
              control.onInputValueChanged(value);
            }
            Refresh();
          });
          return new AnySettingCell { inputControl = cell };
        }
        case SettingControlType.Multiselect: {
          SettingsMultiselectControlCell cell = Instantiate(multiselectCellTemplate.gameObject, multiselectCellTemplate.transform.parent).GetComponent<SettingsMultiselectControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.multiselectControl.items = new MultiselectControlItem[control.multiselectNames.Length];
          for (int i = 0; i < control.multiselectNames.Length; i++) {
            cell.multiselectControl.items[i] = new MultiselectControlItem { Name = control.multiselectNames[i] };
          }
          cell.multiselectControl.CurrentIndex = control.multiselectSelectedIndex;
          return new AnySettingCell { multiselectControl = cell };
        }
        default:
          Debug.LogError("Unknown settings control type");
          return new AnySettingCell {};
      }
    }
  }
}