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
            UpdateHoverables();
        }
    }
    private Tool selectedTool = Tool.Joint;

    [SerializeField]
    private EditorViewController viewController;
    [SerializeField]
    private CameraController cameraController;
    [SerializeField]
    private Grid grid;

    [SerializeField]
	private Texture2D mouseDeleteTexture;

    private CreatureBuilder creatureBuilder;
    private HistoryManager historyManager;

    void Start() {
        creatureBuilder = new CreatureBuilder();
        historyManager = new HistoryManager();
    }

    void Update() {

        HandleClicks();
        HandleKeyboardInput();
    }

    /// <summary>
    /// Removes all currently placed body components from the scene
    /// </summary>
    public void Clear() {
        
        creatureBuilder.Reset();
    }

    public void LoadDesign(CreatureDesign design) {
        creatureBuilder.Reset();
        creatureBuilder = new CreatureBuilder(design);
        print("LoadDesign");
    }
    
    /// <summary>
    /// Saves the current creature design to a local file
    /// </summary>
    public void SaveCurrentDesign() {
        
        SaveDesign(creatureBuilder.GetDesign());
    }

    /// <summary>
    /// Saves the current creature design to a local file with the given name.
    /// </summary>
    public void SaveCurrentDesign(string name) {
        var design = creatureBuilder.GetDesign();
        design.Name = name;
        SaveDesign(design);
    }

    
    public void SaveDesign(CreatureDesign design) {
        // TODO: Implement
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

    public void StartSimulation(SimulationData simulationData) {
        
    }

    #region State Management

    public void Refresh(EditorState state) {

        creatureBuilder.Reset();
        creatureBuilder = new CreatureBuilder(state.CreatureDesign);
        // TODO: Implement non creature building related state changes
    }

    #endregion
    #region Input Management

    /// <summary>
	/// Checks for click / touch events and handles them appropiately depending on the 
	/// currently selected body part.
	/// </summary>
    private void HandleClicks() {

        if (cameraController.IsAdjustingCamera) return;

        var clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickWorldPos.z = 0;

        // Snap to grid for joints and movement
        if (grid.gameObject.activeSelf && (selectedTool == Tool.Joint || selectedTool == Tool.Move)) {
            clickWorldPos = grid.ClosestPointOnGrid(clickWorldPos);
        }

        // TODO: Implement
        // TODO: Add select tool
        // Mouse Down
        if (Input.GetMouseButtonDown(0)) { 

            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (InputUtils.IsPointerOverUIObject()) return;
            
            switch (selectedTool) {

            case Tool.Bone:
                creatureBuilder.TryStartingBone(clickWorldPos); break;
            case Tool.Muscle:
                creatureBuilder.TryStartingMuscle(clickWorldPos); break;
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
                creatureBuilder.UpdateCurrentBoneEnd(clickWorldPos); break;
            case Tool.Muscle:
                creatureBuilder.UpdateCurrentMuscleEnd(clickWorldPos); break;
            case Tool.Move:
                creatureBuilder.MoveCurrentComponent(clickWorldPos); break;
            default: break;
            }
        }

        // Mouse Up
        else if (InputUtils.MouseUp()) {

            switch (selectedTool) {

            case Tool.Joint:
                if (EventSystem.current.IsPointerOverGameObject()) return;
                if (InputUtils.IsPointerOverUIObject()) return;
                if (Input.touchCount > 1) return;
                creatureBuilder.TryPlacingJoint(clickWorldPos); 
                break;

            case Tool.Bone: 
                creatureBuilder.PlaceCurrentBone(); break;
            case Tool.Muscle:
                creatureBuilder.PlaceCurrentMuscle(); break;
            case Tool.Move: 
                creatureBuilder.MoveEnded(); break;
            case Tool.Delete:
                creatureBuilder.DeleteHoveringBodyComponent(); break;

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

    // MARK: - Highlighting on Hover

    /// <summary>
	/// Sets the shouldHighlight variable appropiately on every hoverable object
	/// depending on the currently selected part.  
	/// </summary>
    private void UpdateHoverables() {

        creatureBuilder.SetMouseHoverTextures(null);

        switch (selectedTool) {
        case Tool.Joint: 
            creatureBuilder.EnableHighlighting(false, false, false); break;
        case Tool.Bone:
            creatureBuilder.EnableHighlighting(true, false, false); break;
        case Tool.Muscle:
            creatureBuilder.EnableHighlighting(false, true, false); break;
        case Tool.Move:
            creatureBuilder.EnableHighlighting(true, true, false); break;
        case Tool.Select:
            creatureBuilder.EnableHighlighting(true, true, true); break;
        case Tool.Delete:
            creatureBuilder.SetMouseHoverTextures(mouseDeleteTexture);
            creatureBuilder.EnableHighlighting(true, true, true);
            break;
        }
    }

    #endregion
}