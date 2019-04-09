using UnityEngine;
using UnityEngine.EventSystems;

public class CreatureEditor: MonoBehaviour {

    public enum Tool {
        Joint,
        Bone,
        Muscle,
        Select,
        Move,
        Delete
    }

    public Tool SelectedTool {
        get { return selectedTool; }
        set { 
            selectedTool = value; 
            // TODO: Update Hoverables
        }
    }
    private Tool selectedTool = Tool.Joint;

    [SerializeField]
    private CreatureBuilder creatureBuilder;
    [SerializeField]
    private EditorViewController viewController;
    [SerializeField]
    private CameraController cameraController;
    [SerializeField]
    private Grid grid;

    [SerializeField]
	private Texture2D mouseDeleteTexture;

    void Start() {
        
    }

    void Update() {

        HandleClicks();
        HandleKeyboardInput();
    }

    /// <summary>
    /// Removes all currently placed body components from the scene
    /// </summary>
    public void Clear() {
        
    }

    

    /// <summary>
    /// Prepares the simulation state and loads the simulation scene
    /// </summary>
    public void StartSimulation () {

        // TODO: Implement
        // Get creature design
        // Get simulation settings
        // Load simulation scene
    }

    // MARK: - Input Management

    /// <summary>
	/// Checks for click / touch events and handles them appropiately depending on the 
	/// currently selected body part.
	/// </summary>
    private void HandleClicks() {

        if (cameraController.IsAdjustingCamera) return;

        // TODO: Implement
        // TODO: Add select tool
        // Mouse Down
        if (Input.GetMouseButtonDown(0)) { 

            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (InputUtils.IsPointerOverUIObject()) return;
            
            switch (selectedTool) {
            case Tool.Bone:
                creatureBuilder.TryStartingBone(Input.mousePosition); break;
            case Tool.Muscle:
                creatureBuilder.TryStartingMuscle(Input.mousePosition); break;
            case Tool.Move:
                creatureBuilder.TryStartComponentMove(); break;
            default: break;
            }
        }
        // Mouse Move
        // TODO: Inject mouse position into creatureBuilder function calls
        else if (InputUtils.MouseHeld()) {

            switch (selectedTool) {
            case Tool.Bone:
                creatureBuilder.UpdateCurrentBoneEnd(); break;
            case Tool.Muscle:
                creatureBuilder.UpdateCurrentMuscleEnd(); break;
            case Tool.Move:
                creatureBuilder.MoveCurrentComponent(); break;
            default: break;
            }
        }

        // Mouse Up
        else if (InputUtils.MouseUp()) {

            switch (selectedTool) {
            case Tool.Joint:
                creatureBuilder.TryPlacingJoint(Input.mousePosition); break;
            case Tool.Bone: 
                creatureBuilder.PlaceCurrentBone(); break;
            case Tool.Muscle:
                creatureBuilder.PlaceCurrentMuscle(); break;
            case Tool.Move: 
                creatureBuilder.MoveEnded(); break;
            default: break;
            }
        }
    }

    
    /// <summary>
	/// Handles all possible keyboard controls / editor shortcuts.
	/// </summary>
    private void HandleKeyboardInput() {

        if (!Input.anyKeyDown) return;

        var input = KeyInputManager.shared;
        // J = place Joint
		if (input.GetKeyDown(KeyCode.J)) {
			selectedTool = Tool.Joint;
		}
		// B = place body connection
		else if (input.GetKeyDown(KeyCode.B)) {
			selectedTool = Tool.Bone;
		}
		// M = place muscle
		else if (input.GetKeyDown(KeyCode.M)) {
			selectedTool = Tool.Muscle;
		}
		// D = Delete component
		else if (input.GetKeyDown(KeyCode.D)) {
			selectedTool = Tool.Delete;
		}
		// E = Go to Evolution Scene
		else if (input.GetKeyDown(KeyCode.E)) {
			StartSimulation();
		}

        viewController.Refresh();
    }
}