using UnityEngine;

namespace Keiwando.Evolution {

    public class SimulationInputManager: MonoBehaviour {

        private Evolution evolution;
        private SimulationViewController viewController;

        void Start() {
            evolution = FindObjectOfType<Evolution>();
            viewController = FindObjectOfType<SimulationViewController>();

            InputRegistry.shared.RegisterForAndroidBackButton(delegate () {
                InputRegistry.shared.DeregisterBackButton();
                viewController.GoBackToEditor();
            });
        }

        void Update () {

            HandleKeyboardInput();
        }

        private void HandleKeyboardInput() {

            if (!Input.anyKeyDown) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow)) {

                viewController.FocusOnPreviousCreature();
            
            } else if (Input.GetKeyDown(KeyCode.RightArrow)) {

                viewController.FocusOnNextCreature();
            }
        }
    }
}