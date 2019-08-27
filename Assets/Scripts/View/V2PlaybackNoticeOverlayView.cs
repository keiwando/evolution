using UnityEngine.UI;
using UnityEngine;

namespace Keiwando.Evolution.UI {

    public class V2PlaybackNoticeOverlayView: MonoBehaviour {

        [SerializeField] private Toggle dontShowAgainToggle;
        [SerializeField] private Button closeButton;

        void Start() {

            dontShowAgainToggle.isOn = false;
            dontShowAgainToggle.onValueChanged.AddListener(delegate (bool value) {
                Settings.DontShowV2SimulationDeprecationOverlayAgain = value;
            });

            closeButton.onClick.AddListener(delegate () {
                Close();
            });
        }

        public void Show(bool hideToggle = false) {

            dontShowAgainToggle.gameObject.SetActive(!hideToggle);
            InputRegistry.shared.Register(InputType.AndroidBack, this);
		    GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
            this.gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        public void Close() {
            Time.timeScale = 1;
            InputRegistry.shared.Deregister(this);
		    GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
            this.gameObject.SetActive(false);
        }

        private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this)) {
                Close();
            }
        }
    }
}