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
    public Func<bool> toggleValue;
    public Action<bool> onToggleValueChanged;
    public float sliderMinValue;
    public float sliderMaxValue;
    public Func<float> sliderValue;
    public Func<string> sliderFormattedValue;
    public Action<float> onSliderValueChanged;
    public Action onButtonPressed;
    public Func<string> inputValue;
    public Action<string> onInputValueChanged;
    public string[] multiselectNames;
    public Func<int> multiselectSelectedIndex;
    public Action<int> onMultiselectIndexChanged;

    public Func<bool> disabledIf;

    public string tooltip;
  }

  public struct SettingControlGroup {
    public string name;
    public SettingControl[] controls;
    public GameObject anciliaryView;
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

      tabTemplate.gameObject.SetActive(false);

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
        SettingControlGroup group = controlGroups[tabIndex];
        SettingsTab tab = this.tabs[tabIndex];
        tab.label.SetText(group.name);
        tab.SetSelected(isSelectedTab);
        if (group.anciliaryView != null) {
          group.anciliaryView.SetActive(isSelectedTab);
        }
        for (int controlIndex = 0; controlIndex < group.controls.Length; controlIndex++) {
          SettingControl control = controlGroups[tabIndex].controls[controlIndex];
          AnySettingCell cell = settingCellsPerTab[tabIndex][controlIndex];
          cell.gameObject.SetActive(isSelectedTab);

          bool disabled = false;
          if (control.disabledIf != null) {
            disabled = control.disabledIf();
          } 

          switch (cell.type) {
            case SettingControlType.Toggle:
              cell.toggleControl.toggle.SetIsOnWithoutNotify(control.toggleValue());
              cell.toggleControl.toggle.interactable = !disabled;
              break;
            case SettingControlType.Slider:
              cell.sliderControl.slider.SetValueWithoutNotify(control.sliderValue());
              cell.sliderControl.valueLabel.SetText(control.sliderFormattedValue());
              cell.sliderControl.slider.interactable = !disabled;
              break;
            case SettingControlType.Button:
              cell.buttonControl.button.interactable = !disabled;
              break;
            case SettingControlType.Input:
              cell.inputControl.inputField.SetTextWithoutNotify(control.inputValue());
              cell.inputControl.inputField.interactable = !disabled;
              break;
            case SettingControlType.Multiselect:
              cell.multiselectControl.multiselectControl.SetCurrentIndexWithoutNotify(control.multiselectSelectedIndex());
              cell.multiselectControl.multiselectControl.interactable = !disabled;
              break;
            default:
              break;
          }
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
        int tabIndex = i; // Must copy to not capture a reference to i
        tab.button.onClick.AddListener(delegate () {
          this.selectedTabIndex = tabIndex;
          Refresh();
        });
        tab.gameObject.SetActive(true);
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
          return new AnySettingCell { type = control.type, toggleControl = cell };
        }
        case SettingControlType.Slider: {
          SettingsSliderControlCell cell = Instantiate(sliderCellTemplate.gameObject, sliderCellTemplate.transform.parent).GetComponent<SettingsSliderControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.slider.minValue = control.sliderMinValue;
          cell.slider.maxValue = control.sliderMaxValue;
          cell.slider.onValueChanged.AddListener(delegate (float value) {
            if (control.onSliderValueChanged != null) {
              control.onSliderValueChanged(value);
            }
            Refresh();
          });
          return new AnySettingCell { type = control.type, sliderControl = cell };
        }
        case SettingControlType.Button: {
          SettingsButtonControlCell cell = Instantiate(buttonCellTemplate.gameObject, buttonCellTemplate.transform.parent).GetComponent<SettingsButtonControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.buttonLabel.SetText(control.name);
          cell.button.onClick.AddListener(delegate () {
            if (control.onButtonPressed != null) {
              control.onButtonPressed();
            }
            Refresh();
          });
          return new AnySettingCell { type = control.type, buttonControl = cell };
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
          return new AnySettingCell { type = control.type, inputControl = cell };
        }
        case SettingControlType.Multiselect: {
          SettingsMultiselectControlCell cell = Instantiate(multiselectCellTemplate.gameObject, multiselectCellTemplate.transform.parent).GetComponent<SettingsMultiselectControlCell>();
          cell.gameObject.SetActive(false);
          cell.label.SetText(control.name);
          cell.multiselectControl.items = new MultiselectControlItem[control.multiselectNames.Length];
          for (int i = 0; i < control.multiselectNames.Length; i++) {
            cell.multiselectControl.items[i] = new MultiselectControlItem { Name = control.multiselectNames[i] };
          }
          cell.multiselectControl.CurrentIndex = control.multiselectSelectedIndex();
          cell.multiselectControl.onCurrentIndexChanged.AddListener(delegate (int currentIndex) {
            if (control.onMultiselectIndexChanged != null) {
              control.onMultiselectIndexChanged(currentIndex);
            }
          });
          return new AnySettingCell { type = control.type, multiselectControl = cell };
        }
        default:
          Debug.LogError("Unknown settings control type");
          return new AnySettingCell {};
      }
    }
  }
}