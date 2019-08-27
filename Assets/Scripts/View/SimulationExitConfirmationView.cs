using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public class SimulationExitConfirmationView: MonoBehaviour {

        public delegate void DidConfirmExit();
        public delegate void DidCancelExit();

        [SerializeField] private Toggle dontAskAgainToggle;

        [SerializeField] private Button exitButton;
        [SerializeField] private Button cancelButton;

        private DidCancelExit onCancel;

        public void Show(DidConfirmExit onConfirm, DidCancelExit onCancel) {

            InputRegistry.shared.Register(InputType.AndroidBack, this);
		    GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;

            #if UNITY_WEBGL
            onConfirm();
            return;
            #endif

            if (Settings.DontShowExitConfirmationOverlayAgain) {
                onConfirm();
                return;
            }
            
            exitButton.onClick.AddListener(delegate () {
                onConfirm();
            });

            cancelButton.onClick.AddListener(delegate () {
                onCancel();
            });
            this.onCancel = onCancel;

            dontAskAgainToggle.isOn = Settings.DontShowExitConfirmationOverlayAgain;
            dontAskAgainToggle.onValueChanged.AddListener(delegate (bool value) {
                Settings.DontShowExitConfirmationOverlayAgain = value;
            });

            gameObject.SetActive(true);
        }

        public void Close() {

            exitButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            InputRegistry.shared.Deregister(this);
		    GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
            gameObject.SetActive(false);
        }

        private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this)) {
                onCancel();
            }
        }
    }
}