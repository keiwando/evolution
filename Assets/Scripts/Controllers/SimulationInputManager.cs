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
            
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P)) {
                foreach (var bestCreature in evolution.SimulationData.BestCreatures) {
                    Debug.Log(bestCreature.GetFitness());
                }
            } else if (Input.GetKeyDown(KeyCode.L)) {
                var bestCreatures = evolution.SimulationData.BestCreatures;
                if (bestCreatures.Count <= 1) return;
                var lastFitness = bestCreatures[0].GetFitness();
                for (int i = 1; i < bestCreatures.Count; i++) {
                    var fitness = bestCreatures[i].GetFitness();
                    Debug.Log((fitness - lastFitness) * 100);
                    lastFitness = fitness;
                }
            } // else if (Input.GetKeyDown(KeyCode.K)) {
            //     var currentContainer = GameObject.FindGameObjectWithTag("SimulationConfig");
            //     Destroy(currentContainer);
            //     SceneController.LoadSync(SceneController.Scene.Editor);
            //     var simulationData = evolution.SimulationData;
            //     var containerObject = new GameObject("SimulationConfig");
            //     containerObject.tag = "SimulationConfig";
            //     var configContainer = containerObject.AddComponent<SimulationConfigContainer>();
            //     configContainer.SimulationData = simulationData;
            //     // configContainer.SimulationData = SimulationData.Decode(simulationData.Encode().ToString());

            //     Debug.Log(simulationData == simulationData);
            //     Debug.Log(simulationData == SimulationData.Decode(simulationData.Encode().ToString()));
            //     DontDestroyOnLoad(containerObject);
            //     SceneController.LoadSync(SceneController.Scene.SimulationContainer);
            // }
            #endif
        }
    }
}