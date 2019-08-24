using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Keiwando;
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
    private Grid grid;

    [SerializeField]
	private Texture2D mouseDeleteTexture;
    [SerializeField]
    private UnityEngine.Transform selectionArea;

    private CreatureBuilder creatureBuilder;
    private HistoryManager<CreatureDesign> historyManager;

    private EditorSelectionManager selectionManager;
    private BodyComponentSettingsManager advancedSettingsManager;

    // MARK: - Movement
    private Vector3 lastDragPosition;
    private HashSet<Joint> jointsToMove = new HashSet<Joint>();

    void Start() {

        viewController.Delegate = this;
        historyManager = new HistoryManager<CreatureDesign>(this);

        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        
        var lastDesign = EditorStateManager.LastCreatureDesign;
        if (!lastDesign.IsEmpty) {
            creatureBuilder = new CreatureBuilder(lastDesign);
        } else {
            creatureBuilder = new CreatureBuilder();
        }
        
        selectionManager = new EditorSelectionManager(this, selectionArea, mouseDeleteTexture);

        var simulationConfigs = GameObject.FindGameObjectsWithTag("SimulationConfig");
        foreach (var configContainer in simulationConfigs) {
            Destroy(configContainer);
        }

        var editorSettings = EditorStateManager.EditorSettings;
        grid.Size = editorSettings.GridSize;
        grid.gameObject.SetActive(editorSettings.GridEnabled);

        viewController.Refresh();

        InputRegistry.shared.Register(InputType.Click | InputType.Key | InputType.Touch, this, EventHandleMode.PassthroughEvent);

        var androidBackButton = GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer();
        androidBackButton.OnGesture += delegate (AndroidBackButtonGestureRecognizer recognizer) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
                Application.Quit();
        };
    }

    void Update() {

        InputUtils.UpdateTouches();
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
        viewController.Refresh();
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
        creatureBuilder.Name = name;
        var design = creatureBuilder.GetDesign();
        SaveDesign(design);
    }

    
    public void SaveDesign(CreatureDesign design) {
        CreatureSerializer.SaveCreatureDesign(design);
    }

    public void Undo() {
        historyManager.Undo();
    }

    public void Redo() {
        historyManager.Redo();
    }
    
    public string GetCreatureName() {
        return creatureBuilder.Name;
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

        EditorStateManager.LastCreatureDesign = creatureDesign;

        var sceneDescription = DefaultSimulationScenes.DefaultSceneForObjective(simulationSettings.Objective);
        
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

        InputRegistry.shared.Deregister(this);
        
        // Load simulation scene
        SceneController.LoadSync(SceneController.Scene.SimulationContainer);
    }

    #region State Management

    public void SetState(CreatureDesign design) {

        creatureBuilder.Reset();
        creatureBuilder = new CreatureBuilder(design);
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

        var pinchRecognizer = GestureRecognizerCollection.shared.GetPinchGestureRecognizer();
        if (pinchRecognizer.State != GestureRecognizerState.Ended) return;

        if (!InputRegistry.shared.MayHandle(InputType.Click | InputType.Touch, this)) return;

        bool isPointerOverUI = false;
        isPointerOverUI |= EventSystem.current.IsPointerOverGameObject();
        if (Input.touchCount > 0) {
            isPointerOverUI |= InputUtils.IsTouchOverUI(Input.GetTouch(0).fingerId);
        }

        var clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickWorldPos.z = 0;

        // Snap to grid for joints and movement
        if (grid.gameObject.activeSelf && (selectedTool == Tool.Joint || selectedTool == Tool.Move)) {
            clickWorldPos = grid.ClosestPointOnGrid(clickWorldPos);
        }

        // Mouse Down
        if (Input.GetMouseButtonDown(0)) { 

            if (isPointerOverUI) return;
            
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
                if (isPointerOverUI) break;
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
                if (isPointerOverUI) return;
                if (Input.touchCount > 1) return;
                #if UNITY_IOS || UNITY_ANDROID
                if (Input.touchCount == 0) return;
                #endif
                creatureEdited = creatureBuilder.TryPlacingJoint(clickWorldPos); 
                break;

            case Tool.Bone:
                var hoveringJoint = selectionManager.GetSingleSelected<Joint>();
                if (hoveringJoint != null) {
                    creatureEdited = creatureBuilder.PlaceCurrentBone(); 
                } else {
                    creatureEdited = false;
                    creatureBuilder.CancelCurrentBone();
                }
                break; 

            case Tool.Muscle:
                creatureEdited = creatureBuilder.PlaceCurrentMuscle(); 
                break;

            case Tool.Move: 
                creatureEdited = creatureBuilder.MoveEnded(jointsToMove); 
                jointsToMove.Clear();
                if (!isPointerOverUI) {
                    selectionManager.DeselectAll();
                }
                break;

            case Tool.Delete:
                if (isPointerOverUI) return;
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
                if (selectedTool == Tool.Move) {
                    creatureBuilder.RefreshMuscleColliders();
                }
            }
        }

        viewController.Refresh();
    }

    
    /// <summary>
	/// Handles all possible keyboard controls / editor shortcuts.
	/// </summary>
    private void HandleKeyboardInput() {

        if (!Input.anyKeyDown) return;

        if (!InputRegistry.shared.MayHandle(InputType.Key, this)) return;

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

        // DEBUG

        // else if (Input.GetKeyDown(KeyCode.K)) {
        //     Debug.Log(historyManager.GetDebugState());   
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
        // UpdateHoverables();
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
            var deleted = selectionManager.DeleteSelection(this.creatureBuilder);
            if (deleted) {
                historyManager.Push(oldDesign);
                viewController.Refresh();
            }
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