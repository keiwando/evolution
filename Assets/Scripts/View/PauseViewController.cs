using UnityEngine;
using UnityEngine.UI;
using Keiwando.Evolution;

namespace Keiwando.Evolution.UI {

    public interface IPauseViewControllerDelegate {
        void DidDismiss(PauseViewController viewController);
    }

    public class PauseViewController: MonoBehaviour {

        public IPauseViewControllerDelegate Delegate { get; set; }

        [SerializeField] private Button continueButton;
        
        [SerializeField] private InputField generationTimeInput;

        [SerializeField] private GeneralSettingsView generalSettingsView;

        private SettingsManager settingsManager;

        void Start() {

            var evolution = FindObjectOfType<Evolution>();
            this.settingsManager = new SettingsManager(evolution);

            continueButton.onClick.AddListener(delegate () {
                Hide();
            });

            generationTimeInput.onValueChanged.AddListener(delegate (string text) {
                int simulationTime = 0;
                try {
                    int.TryParse(text, out simulationTime);
                } catch { 
                    Refresh();
                    return; 
                }
                settingsManager.SimulationTimeChanged(this, simulationTime);
            });

            generalSettingsView.Delegate = settingsManager;

            Refresh();
        }

        public void Refresh() {

            generationTimeInput.text = settingsManager.GetSimulationTime(this).ToString();

            generalSettingsView.Refresh();
        }

        public void Show() {
            this.gameObject.SetActive(true);
            InputRegistry.shared.RegisterForAndroidBackButton(delegate () {
                Hide();
            });
        }

        public void Hide() {
            this.gameObject.SetActive(false);
            InputRegistry.shared.DeregisterBackButton();
            Delegate?.DidDismiss(this);
        }
    }
}