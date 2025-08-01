using System;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public interface IPauseViewControllerDelegate {
        void DidDismiss(PauseViewController viewController);
    }

    public class PauseViewController: MonoBehaviour {

        public IPauseViewControllerDelegate Delegate { get; set; }

        [SerializeField] private SettingsView settingsView;
        [SerializeField] private NeuralNetworkSettingsUIManager neuralNetworkSettingsUIManager; 

        [SerializeField] private Button continueButton;
        
        private SettingsManager settingsManager;

        void Start() {

            var evolution = FindAnyObjectByType<Evolution>();
            this.settingsManager = new SettingsManager(
                evolution: evolution, 
                neuralNetworkSettingsUIManager: neuralNetworkSettingsUIManager
            );
            settingsManager.Setup(settingsView, setupForPauseScreen: true);

            continueButton.onClick.AddListener(delegate () {
                Hide();
            });

            Refresh();
        }

        public void Refresh() {
            settingsView.Refresh();
        }

        public void Show() {
            this.gameObject.SetActive(true);
            InputRegistry.shared.Register(InputType.AndroidBack, this);
            GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
            Refresh();
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