using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public interface ISettingsViewControllerDelegate: 
        IGeneralSettingsViewDelegate, INetworkSettingsViewDelegate {}

    public class SettingsViewController: MonoBehaviour {

        [SerializeField] private SettingsView settingsView;
        [SerializeField] private NeuralNetworkSettingsUIManager neuralNetworkSettingsUIManager;

        // TODO: Remove
        [SerializeField] private GeneralSettingsView generalSettingsView;
        [SerializeField] private NetworkSettingsView networkSettingsView;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button brainButton;
        [SerializeField] private Button backFromNetworkSettingsButton;

        [SerializeField] private SlidingContainer container;

        [SerializeField] private Grid grid;

        private bool isShowingGeneralSettings {
            get => container.LastSlideDirection != SlidingContainer.Direction.Up;
        }

        void Start() {

            var settingsManager = new SettingsManager(neuralNetworkSettingsUIManager: neuralNetworkSettingsUIManager);
            settingsManager.grid = grid;
            generalSettingsView.Delegate = settingsManager;
            networkSettingsView.Delegate = settingsManager;

            settingsManager.Setup(settingsView, setupForPauseScreen: false);
            
            closeButton.onClick.AddListener(delegate () {
                Hide();
            });

            brainButton.onClick.AddListener(delegate () {
                if (isShowingGeneralSettings) {
                    container.Slide(SlidingContainer.Direction.Up, 0.3f, false);
                }
            });

            backFromNetworkSettingsButton.onClick.AddListener(delegate () {
                if (!isShowingGeneralSettings) {
                    container.Slide(SlidingContainer.Direction.Down, 0.3f, false);
                }
            });

            Refresh();
        }

        public void Refresh() {
            generalSettingsView.Refresh();
            networkSettingsView.Refresh();
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
        }

        private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
                Hide();
        }
    }
}