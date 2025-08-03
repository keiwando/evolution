using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public class SettingsViewController: MonoBehaviour {

        [SerializeField] private SettingsView settingsView;
        [SerializeField] private NeuralNetworkSettingsUIManager neuralNetworkSettingsUIManager;
        [SerializeField] private EditorViewController editorViewController;

        [SerializeField] private Button closeButton;
        [SerializeField] private Grid grid;

        void Start() {

            var settingsManager = new SettingsManager(neuralNetworkSettingsUIManager: neuralNetworkSettingsUIManager);
            settingsManager.grid = grid;
            settingsManager.Setup(settingsView, setupForPauseScreen: false);
            
            closeButton.onClick.AddListener(delegate () {
                Hide();
            });

            Refresh();
        }

        public void Refresh() {
            settingsView.Refresh();
        }

        public void Show() {
            gameObject.SetActive(true);
            InputRegistry.shared.Register(InputType.AndroidBack, this);
            GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
            Refresh();
        }

        public void Hide() {
            InputRegistry.shared.Deregister(this);
            GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
            gameObject.SetActive(false);
            editorViewController.Refresh();
        }

        private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
                Hide();
        }
    }
}