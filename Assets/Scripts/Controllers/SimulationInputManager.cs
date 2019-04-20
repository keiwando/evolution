using UnityEngine;

public class SimulationInputManager: MonoBehaviour {

    private Evolution evolution;
    private SimulationViewController viewController;

    void Start() {
        evolution = FindObjectOfType<Evolution>();
        viewController = FindObjectOfType<SimulationViewController>();
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

		} else if (Input.GetKeyDown(KeyCode.Escape)) {
			viewController.GoBackToEditor();
		}

		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Backspace)) {
			viewController.GoBackToEditor();
		}	
    }
}