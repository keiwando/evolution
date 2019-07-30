using UnityEngine;

namespace Keiwando.Evolution {

    public class SimulationInputManager: MonoBehaviour {

        private Evolution evolution;
        private SimulationViewController viewController;

        void Start() {
            evolution = FindObjectOfType<Evolution>();
            viewController = FindObjectOfType<SimulationViewController>();

            InputRegistry.shared.Register(InputType.AndroidBack, this); 
            var androidRecognizer = GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer();
            androidRecognizer.OnGesture += delegate (AndroidBackButtonGestureRecognizer recognizer) {
                if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this)) {
                    InputRegistry.shared.Deregister(this);
                    viewController.GoBackToEditor();
                }
            };

            InputRegistry.shared.Register(InputType.Key, this);
        }

        void Update () {

            HandleKeyboardInput();
        }

        private void HandleKeyboardInput() {

            if (!Input.anyKeyDown) return;
            if (!InputRegistry.shared.MayHandle(InputType.Key, this)) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow)) {

                viewController.FocusOnPreviousCreature();
            
            } else if (Input.GetKeyDown(KeyCode.RightArrow)) {

                viewController.FocusOnNextCreature();
            }
        }
    }
}