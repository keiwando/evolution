using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Keiwando.UI {

    public class LabelledToggle: MonoBehaviour {

        public event System.Action<bool> onValueChanged;

        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text descriptionLabel;

        public string Description {
            get { return descriptionLabel?.text ?? ""; }
            set { if (descriptionLabel != null) descriptionLabel.text = value; }
        }

        void Start() {
            this.toggle.onValueChanged.AddListener(delegate (bool enabled) {
                if (this.onValueChanged != null) {
                    onValueChanged(enabled);
                }
            });
        }

        public void Refresh(bool enabled) {
            toggle.isOn = enabled;
        }
    }
}