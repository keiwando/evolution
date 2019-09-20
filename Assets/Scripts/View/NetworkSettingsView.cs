using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public interface INetworkSettingsViewDelegate {

        void ResetButtonPressed(NetworkSettingsView view);
    }

    public class NetworkSettingsView: MonoBehaviour {

        public INetworkSettingsViewDelegate Delegate { get; set; }

        [SerializeField] private Button resetButton;

        void Start() {

            resetButton.onClick.AddListener(delegate () {
                Delegate.ResetButtonPressed(this);
            });
        }

        public void Refresh() {
            
        }
    }
}