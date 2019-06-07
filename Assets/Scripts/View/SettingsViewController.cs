using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public class SettingsViewcontroller: 
        MonoBehaviour,
        INetworkSettingsViewDelegate {

        [SerializeField] private GeneralSettingsView generalSettingsView;
        [SerializeField] private NetworkSettingsView NetworkSettingsView;

        [SerializeField] private SlidingContainer container;

        private bool isShowingGeneralSettings {
            get => container.LastSlideDirection != SlidingContainer.Direction.Down;
        }

        void Start() {
            
        }        

        // MARK: - INetworkSettingsViewDelegate

        public void BackButtonPressed(NetworkSettingsView view) {

            
        }

        public void ResetButtonPressed(NetworkSettingsView view) {

        }
    }
}