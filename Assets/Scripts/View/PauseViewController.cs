using System;
using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public interface IPauseViewControllerDelegate {
        void DidDismiss(PauseViewController viewController);
    }

    public class PauseViewController: MonoBehaviour {

        public IPauseViewControllerDelegate Delegate { get; set; }

        [SerializeField] private Button continueButton;
        
        [SerializeField] private InputField generationTimeInput;
        [SerializeField] private InputField populationSizeInput;

        [SerializeField] private GeneralSettingsView generalSettingsView;

        private SettingsManager settingsManager;

        void Start() {

            var evolution = FindObjectOfType<Evolution>();
            this.settingsManager = new SettingsManager(evolution);

            continueButton.onClick.AddListener(delegate () {
                Hide();
            });

            generationTimeInput.onEndEdit.AddListener(delegate (string text) {
                int simulationTime = 0;
                try {
                    simulationTime = int.Parse(text);
                } catch { 
                    Refresh();
                    return; 
                }
                settingsManager.SimulationTimeDidChange(simulationTime);
                Refresh();
            });

            populationSizeInput.onEndEdit.AddListener(delegate (string text) {
                int populationSize = 10;
                try {
                    populationSize = int.Parse(text);
                } catch {
                    Refresh();
                    return;
                }
                int batchSize = Math.Min(settingsManager.GetBatchSize(generalSettingsView), populationSize);
                settingsManager.BatchSizeChanged(generalSettingsView, batchSize);
                settingsManager.PopulationSizeDidChange(populationSize);
                Refresh();
            });

            generalSettingsView.Delegate = settingsManager;

            Refresh();
        }

        public void Refresh() {

            generationTimeInput.text = settingsManager.GetSimulationTime(this).ToString();
            populationSizeInput.text = settingsManager.GetPopulationSize().ToString();

            generalSettingsView.Refresh();
        }

        public void Show() {
            this.gameObject.SetActive(true);
            InputRegistry.shared.Register(InputType.AndroidBack, this);
            GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
        }

        public void Hide() {
            this.gameObject.SetActive(false);
            InputRegistry.shared.Deregister(this);
            GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
            Delegate?.DidDismiss(this);
        }

        private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
                Hide();
        } 
    }
}