using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Keiwando.Evolution.UI;

namespace Keiwando.UI {

    public class LabelledInput: MonoBehaviour {

        public event System.Action<string> onValueChanged;

        [SerializeField] private TMP_InputField input;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private ClickableTooltip tooltip;

        public string Description {
            get { return descriptionLabel?.text ?? ""; }
            set { if (descriptionLabel != null) descriptionLabel.text = value; }
        }

        public TooltipData TooltipData { get; set; }

        void Start() {

            this.input.onEndEdit.AddListener(delegate (string value) {
                if (this.onValueChanged != null) {
                    onValueChanged(value);
                }
            });
            if (TooltipData == null) {
                this.tooltip.gameObject.SetActive(false);
            }
        }

        public void Refresh(string value) {
            input.text = value;
            tooltip?.SetData(TooltipData);
        }
    }
}