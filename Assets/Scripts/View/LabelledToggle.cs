using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Keiwando.Evolution.UI;

namespace Keiwando.UI {

    public class LabelledToggle: MonoBehaviour {

        public event System.Action<bool> onValueChanged;

        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private ClickableTooltip tooltip;

        public string Description {
            get { return descriptionLabel?.text ?? ""; }
            set { if (descriptionLabel != null) descriptionLabel.text = value; }
        }

        public TooltipData TooltipData { get; set; }

        void Start() {
            this.toggle.onValueChanged.AddListener(delegate (bool enabled) {
                if (this.onValueChanged != null) {
                    onValueChanged(enabled);
                }
            });
            if (TooltipData == null) {
                this.tooltip.gameObject.SetActive(false);
            }
        }

        public void Refresh(bool enabled) {
            toggle.isOn = enabled;

            if (tooltip != null) {
                if ((TooltipData == null) == tooltip.gameObject.activeSelf) {
                    tooltip.gameObject.SetActive(TooltipData != null);
                }
                if (tooltip.gameObject.activeSelf) {
                    tooltip.SetData(TooltipData);
                }
            }
        }
    }
}