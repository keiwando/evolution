using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.UI {

    public class LabelledSlider: MonoBehaviour {

        public event System.Action<float> onValueChanged;

        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text valueLabel;

        public string Description {
            get { return descriptionLabel?.text ?? "" }
            set { if (descriptionLabel != null) descriptionLabel.text = value; }
        }

        private Slider slider;

        void Start() {

            this.slider = GetComponentInChildren<Slider>();

            this.slider.onValueChanged.AddListener(delegate (float value) {
                if (this.onValueChanged != null) {
                    onValueChanged(value);
                }
            });
        }

        public void Refresh(float value) {
            slider.value = value;
        }
    }
}