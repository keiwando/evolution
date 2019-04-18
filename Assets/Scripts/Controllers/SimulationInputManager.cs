using UnityEngine;

public class SimulationInputManager: MonoBehaviour {

    [SerializeField]
    private Evolution evolution;

    [SerializeField]
    private CameraFollowController cameraFollowController;

    void Update () {

		HandleKeyboardInput();
	}

    private void HandleKeyboardInput() {

        if (!Input.anyKeyDown) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {

			FocusOnPreviousCreature();
		
		} else if (Input.GetKeyDown(KeyCode.RightArrow)) {

			FocusOnNextCreature();

		} else if (Input.GetKeyDown(KeyCode.Escape)) {
			GoBackToEditor();
		}

		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Backspace)) {
			GoBackToEditor();
		}	
    }

    private void GoBackToEditor() {
        evolution.Finish();
        SceneController.LoadSync(SceneController.Scene.Editor);
    }

    private void FocusOnPreviousCreature() {
        cameraFollowController.FocusOnPreviousCreature();
    }

    private void FocusOnNextCreature() {
        cameraFollowController.FocusOnNextCreature();
    }
}