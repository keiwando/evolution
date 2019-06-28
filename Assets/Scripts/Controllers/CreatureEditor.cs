using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Keiwando.Evolution.Scenes;
using Keiwando.Evolution;
using Keiwando.Evolution.UI;

public class CreatureEditor: MonoBehaviour, 
                             HistoryManager<CreatureDesign>.IStateProvider,
                             IEditorViewControllerDelegate {

    public event System.Action<Tool> onToolChanged;

    public enum Tool {
        Joint,
        Bone,
        Muscle,
        Move,
        Delete,
        Select
    }

    public Tool SelectedTool {
        get { return selectedTool; }
        set { 
            selectedTool = value;
            OnToolChanged(value);
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
    [SerializeField]
    private UnityEngine.Transform selectionArea;

    public CreatureBuilder creatureBuilder { get; private set; }
    private HistoryManager<CreatureDesign> historyManager;

    private EditorSelectionManager selectionManager;
    private BodyComponentSettingsManager advancedSettingsManager;

    // MARK: - Movement
    private Vector3 lastDragPosition;
    private HashSet<Joint> jointsToMove;

    void Start() {

        viewController.Delegate = this;
        historyManager = new HistoryManager<CreatureDesign>(this);

        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        creatureBuilder = new CreatureBuilder();
        selectionManager = new EditorSelectionManager(this, selectionArea, mouseDeleteTexture);

        var simulationConfigs = GameObject.FindGameObjectsWithTag("SimulationConfig");
        foreach (var configContainer in simulationConfigs) {
            Destroy(configContainer);
        }

        var editorSettings = EditorStateManager.EditorSettings;
        grid.gameObject.SetActive(editorSettings.GridEnabled);
        grid.Size = editorSettings.GridSize;

        viewController.Refresh();
    }

    void Update() {

        selectionManager.Update(Input.mousePosition);
        HandleClicks();
        HandleKeyboardInput();
    }

    /// <summary>
    /// Removes all currently placed body components from the scene
    /// </summary>
    public void Clear() {
        
        historyManager.Push(GetState(historyManager));
        creatureBuilder.Reset();
    }

    public void LoadDesign(CreatureDesign design) {
        historyManager.Push(GetState(historyManager));
        creatureBuilder.Reset();
        creatureBuilder = new CreatureBuilder(design);
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

    public void Undo() {
        historyManager.Undo();
    }

    public void Redo() {
        historyManager.Redo();
    }
    

    /// <summary>
    /// Prepares the simulation state and loads the simulation scene
    /// </summary>
    public void StartSimulation () {

        var simulationSettings = EditorStateManager.SimulationSettings;
        var networkSettings = EditorStateManager.NetworkSettings;
        var creatureDesign = creatureBuilder.GetDesign();

        // Don't start the simulation if the creature design is empty
        if (creatureDesign.IsEmpty) return;

        var sceneDescription = DefaultSimulationScenes.DefaultSceneForTask(simulationSettings.Task);
        
        var simulationData = new SimulationData(simulationSettings, 
                                                networkSettings,
                                                creatureDesign,
                                                sceneDescription);
        StartSimulation(simulationData);
    }

    public void StartSimulation(SimulationData simulationData) {
        
        var containerObject = new GameObject("SimulationConfig");
        containerObject.tag = "SimulationConfig";
        var configContainer = containerObject.AddComponent<SimulationConfigContainer>();
        configContainer.SimulationData = simulationData;
        DontDestroyOnLoad(containerObject);
        
        // Load simulation scene
        SceneController.LoadSync(SceneController.Scene.SimulationContainer);
    }

    #region State Management

    public void SetState(CreatureDesign design) {

        creatureBuilder.Reset();
        creatureBuilder = new CreatureBuilder(design);
        // TODO: Implement non creature building related state changes
    }

    public CreatureDesign GetState(HistoryManager<CreatureDesign> manager) {
        return creatureBuilder.GetDesign();
    }

    #endregion
    #region Input Management

    /// <summary>
	/// Checks for click / touch events and handles them appropiately depending on the 
	/// currently selected body part.
	/// </summary>
    private void HandleClicks() {

        if (cameraController.IsAdjustingCamera) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        var clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickWorldPos.z = 0;

        // Snap to grid for joints and movement
        if (grid.gameObject.activeSelf && (selectedTool == Tool.Joint || selectedTool == Tool.Move)) {
            clickWorldPos = grid.ClosestPointOnGrid(clickWorldPos);
        }

        // TODO: Implement
        // Mouse Down
        if (Input.GetMouseButtonDown(0)) { 

            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (InputUtils.IsPointerOverUIObject()) return;
            
            switch (selectedTool) {

            case Tool.Bone:
                var joint = selectionManager.GetSingleSelected<Joint>();
                if (joint != null)
                    creatureBuilder.TryStartingBone(joint); 
                break;

            case Tool.Muscle:
                var bone = selectionManager.GetSingleSelected<Bone>();
                if (bone != null)
                    creatureBuilder.TryStartingMuscle(bone); 
                break;

            case Tool.Move:
                selectionManager.AddCurrentHoveringToSelection();
                jointsToMove = selectionManager.GetJointsToMoveFromSelection(); 
                lastDragPosition = clickWorldPos;
                break;

            case Tool.Select:
                selectionManager.DeselectAll();
                selectionManager.StartSelection(clickWorldPos); 
                break;
            default: break;
            
            }
        }
        // Mouse Move
        else if (InputUtils.MouseHeld()) {

            switch (selectedTool) {

            case Tool.Bone:
                var hoveringJoint = selectionManager.GetSingleSelected<Joint>();
                creatureBuilder.UpdateCurrentBoneEnd(clickWorldPos, hoveringJoint); 
                break;

            case Tool.Muscle:
                var hoveringBone = selectionManager.GetSingleSelected<Bone>();
                creatureBuilder.UpdateCurrentMuscleEnd(clickWorldPos, hoveringBone); 
                break;

            case Tool.Move:
                if (jointsToMove.Count > 0) {
                    creatureBuilder.MoveSelection(jointsToMove, clickWorldPos - lastDragPosition);
                    lastDragPosition = clickWorldPos;
                }    
                break;

            case Tool.Select:
                selectionManager.UpdateSelection(clickWorldPos); break;
            default: break;
            }
        }

        // Mouse Up
        else if (InputUtils.MouseUp()) {

            var creatureEdited = false;
            var oldDesign = creatureBuilder.GetDesign();

            switch (selectedTool) {

            case Tool.Joint:
                if (EventSystem.current.IsPointerOverGameObject()) return;
                if (InputUtils.IsPointerOverUIObject()) return;
                if (Input.touchCount > 1) return;
                creatureEdited = creatureBuilder.TryPlacingJoint(clickWorldPos); 
                break;

            case Tool.Bone:
                creatureEdited = creatureBuilder.PlaceCurrentBone(); break;

            case Tool.Muscle:
                creatureEdited = creatureBuilder.PlaceCurrentMuscle(); break;

            case Tool.Move: 
                creatureEdited = creatureBuilder.MoveEnded(jointsToMove); 
                selectionManager.DeselectAll();
                break;

            case Tool.Delete:
                selectionManager.AddCurrentHoveringToSelection();
                var selection = selectionManager.GetSelection();
                creatureEdited = selection.Count > 0;
                creatureBuilder.Delete(selection);
                selectionManager.DeselectAll();
                break;

            case Tool.Select:
                selectionManager.EndSelection(); 
                OnSelectionEnded();
                break;

            default: break;
            }

            if (creatureEdited) {
                historyManager.Push(oldDesign);
            }
        }

        viewController.Refresh();
    }

    
    /// <summary>
	/// Handles all possible keyboard controls / editor shortcuts.
	/// </summary>
    private void HandleKeyboardInput() {

        if (!Input.anyKeyDown) return;

        var input = KeyInputManager.shared;
        // J = place Joint
		if (input.GetKeyDown(KeyCode.J)) {
			SelectedTool = Tool.Joint;
		}
		// B = place body connection
		else if (input.GetKeyDown(KeyCode.B)) {
			SelectedTool = Tool.Bone;
		}
        // V = move
        else if (input.GetKeyDown(KeyCode.V)) {
            SelectedTool = Tool.Move;   
        }
		// M = place muscle
		else if (input.GetKeyDown(KeyCode.M)) {
			SelectedTool = Tool.Muscle;
		}
        // S = select
        else if (input.GetKeyDown(KeyCode.S)) {
            SelectedTool = Tool.Select;
        }

		// D = Delete component
		else if (input.GetKeyDown(KeyCode.D)) {
			SelectedTool = Tool.Delete;
		}
		// E = Go to Evolution Scene
		else if (input.GetKeyDown(KeyCode.E)) {
			StartSimulation();
		}

        else if (input.GetKeyDown(KeyCode.Escape)) {
            selectionManager.DeselectAll();
        }

        
        else if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            
            // Cmd + Z = Undo
            if (input.GetKeyDown(KeyCode.Z)) {
                historyManager.Undo();
            } 

            // Cmd + Shift + Z = Redo
            else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && 
                    input.GetKeyDown(KeyCode.Z)) {
                historyManager.Redo();
            }
            
            // Cmd + Y = Redo
            else if (input.GetKeyDown(KeyCode.Y)) {
                historyManager.Redo();
            }
            
        } 

        // TODO: Remove
        // else if (input.GetKeyDown(KeyCode.P)) {
        //     Debug.Log(historyManager.GetDebugState());
        // }
        // else if (input.GetKeyDown(KeyCode.Q)) {
        //     Debug.Log(GetState().CreatureDesign.GetDebugDescription());
        // }

        viewController.Refresh();
    }

    private void OnToolChanged(Tool tool) {
        if (tool == Tool.Delete) {
            DeleteCurrentSelection();
        }
        if (tool != Tool.Select) {
            viewController.ShowBasicSettingsControls();
        }
        UpdateHoverables();
        if (onToolChanged != null) {
            onToolChanged(tool);
        }
    }

    private void OnSelectionEnded() {
        // Check if we need to show the advanced body settings UI
        var selection = selectionManager.GetSelection();
        if (selection.Count != 1) {
            viewController.ShowBasicSettingsControls();
            return;
        };

        var part = selection[0];

        if (part is Joint) {
            advancedSettingsManager = new JointSettingsManager(part as Joint, viewController.ShowAdvancedSettingsControls());
        }
        else if (part is Bone) {
            advancedSettingsManager = new BoneSettingsManager(part as Bone, viewController.ShowAdvancedSettingsControls());
        }
        else if (part is Muscle) {
            advancedSettingsManager = new MuscleSettingsManager(part as Muscle, viewController.ShowAdvancedSettingsControls());
        }
        advancedSettingsManager.dataWillChange += delegate () {
            historyManager.Push(creatureBuilder.GetDesign());
        };
    }

    private void DeleteCurrentSelection() {

        if (selectionManager.IsAnythingSelected()) {
            var oldDesign = creatureBuilder.GetDesign();
            var deleted = selectionManager.DeleteSelection();
            if (deleted) {
                historyManager.Push(oldDesign);
                viewController.Refresh();
            }
        }
    }

    // MARK: - Highlighting on Hover

    // TODO: Remove this

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
            // creatureBuilder.EnableHighlighting(true, true, true); break;
            break;
        case Tool.Delete:
            creatureBuilder.SetMouseHoverTextures(mouseDeleteTexture);
            creatureBuilder.EnableHighlighting(true, true, true);
            break;
        }
    }

    #endregion
    #region IEditorViewControllerDelegate 

    public bool CanUndo(EditorViewController viewController) {
        return historyManager.CanUndo();
    }

    public bool CanRedo(EditorViewController viewController) {
        return historyManager.CanRedo();
    }

    #endregion
}