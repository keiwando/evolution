using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public interface INetworkSettingsViewDelegate {

        void BackButtonPressed(NetworkSettingsView view);
        void ResetButtonPressed(NetworkSettingsView view);
    }

    public class NetworkSettingsView: MonoBehaviour {

        public INetworkSettingsViewDelegate Delegate { get; set; }

        [SerializeField] private Button backButton;
        [SerializeField] private Button resetButton;

        void Start() {

            backButton.onClick.AddListener(delegate () {
                Delegate.BackButtonPressed(this);
            });

            resetButton.onClick.AddListener(delegate () {
                Delegate.ResetButtonPressed(this);
            });
        }
    }
}