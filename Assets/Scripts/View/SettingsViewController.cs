using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public interface ISettingsViewControllerDelegate: 
        IGeneralSettingsViewDelegate, INetworkSettingsViewDelegate {}

    public class SettingsViewController: MonoBehaviour {

        public ISettingsViewControllerDelegate Delegate { 
            set { 
                generalSettingsView.Delegate = value;
                networkSettingsView.Delegate = value;
            } 
        }

        [SerializeField] private GeneralSettingsView generalSettingsView;
        [SerializeField] private NetworkSettingsView networkSettingsView;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button brainButton;
        [SerializeField] private Button backFromNetworkSettingsButton;

        [SerializeField] private SlidingContainer container;

        private bool isShowingGeneralSettings {
            get => container.LastSlideDirection != SlidingContainer.Direction.Down;
        }

        void Start() {

            
            
            closeButton.onClick.AddListener(delegate () {
                Hide();
            });

            brainButton.onClick.AddListener(delegate () {
                if (isShowingGeneralSettings) {
                    container.Slide(SlidingContainer.Direction.Down, 0.3f, 1f - container.AnimationProgress);
                }
            });

            backFromNetworkSettingsButton.onClick.AddListener(delegate () {
                if (!isShowingGeneralSettings) {
                    container.Slide(SlidingContainer.Direction.Up, 0.3f, 1f - container.AnimationProgress);
                }
            });
        }

        public void Refresh() {
            generalSettingsView.Refresh();
            networkSettingsView.Refresh();
        }

        public void Show() {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide() {
            gameObject.SetActive(false);
        }
    }
}