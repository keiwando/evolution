using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public class SimulationExitConfirmationView: MonoBehaviour {

        public delegate void DidConfirmExit();
        public delegate void DidCancelExit();

        [SerializeField] private Toggle dontAskAgainToggle;

        [SerializeField] private Button exitButton;
        [SerializeField] private Button cancelButton;

        public void Show(DidConfirmExit onConfirm, DidCancelExit onCancel) {

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

            dontAskAgainToggle.isOn = Settings.DontShowExitConfirmationOverlayAgain;
            dontAskAgainToggle.onValueChanged.AddListener(delegate (bool value) {
                Settings.DontShowExitConfirmationOverlayAgain = value;
            });

            gameObject.SetActive(true);
        }

        public void Close() {

            exitButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }
    }
}