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
        Decoration,
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
    public DecorationType SelectedDecorationType = DecorationType.GooglyEye;

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
    private HashSet<int> selectionAtBeginningOfDrag = new HashSet<int>();
    private BodyComponentSettingsManager advancedSettingsManager;

    // MARK: - Movement
    private Vector3 lastDragPosition;
    private HashSet<Joint> jointsToMove = new HashSet<Joint>();
    // TODO: Make the move tool work for decorations as well

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

        InputRegistry.shared.Register(InputType.Click | InputType.Key | InputType.Touch | InputType.AndroidBack, this, EventHandleMode.PassthroughEvent);

        var androidBackButton = GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer();
        androidBackButton.OnGesture += delegate (AndroidBackButtonGestureRecognizer recognizer) {
            if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
                Application.Quit();
        };
        
        GestureRecognizerCollection.shared.CreateClickGestureRecognizerIfNecessary();

        // Physics.autoSimulation = true;
        Physics.autoSyncTransforms = true;
    }

    void Update() {

        InputUtils.UpdateTouches();
        if (selectedTool == Tool.Select && Input.GetMouseButtonDown(0)) {
            this.selectionAtBeginningOfDrag = selectionManager.GetSelectedIds();
        }
        selectionManager.Update(InputUtils.GetMousePosition());
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
        viewController.Refresh();
    }
    
    /// <summary>
    /// Saves the current creature design to a local file with the given name.
    /// </summary>
    public void SaveCurrentDesign(string name) {
        creatureBuilder.Name = name;
        var design = creatureBuilder.GetDesign();
        CreatureSerializer.SaveCreatureDesign(design, true);
        viewController.Refresh();
    }

    public void Undo() {
        creatureBuilder.CancelTemporaryBodyParts();
        historyManager.Undo();
    }

    public void Redo() {
        creatureBuilder.CancelTemporaryBodyParts();
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

        var sceneDescription = DefaultSimulationScenes.DefaultSceneForObjective(simulationSettings.Objective);
        
        var simulationData = new SimulationData(simulationSettings, 
                                                networkSettings,
                                                creatureDesign,
                                                sceneDescription);
        StartSimulation(simulationData);
    }

    public void StartSimulation(SimulationData simulationData, SimulationOptions options = new SimulationOptions()) {
        
        // We remember the creature design here instead of in StartSimulation so that it gets 
        // loaded in the editor even when you load a previously saved simulation and not just 
        // when you start one with your own creature design.
        EditorStateManager.LastCreatureDesign = simulationData.CreatureDesign;

        var containerObject = new GameObject("SimulationConfig");
        containerObject.tag = "SimulationConfig";
        var configContainer = containerObject.AddComponent<SimulationConfigContainer>();
        configContainer.Config = new SimulationConfig();
        configContainer.Config.SimulationData = simulationData;
        configContainer.Config.Options = options;
        DontDestroyOnLoad(containerObject);

        InputRegistry.shared.Deregister(this);

        Physics.simulationMode = SimulationMode.Script;
        Physics.autoSyncTransforms = false;
        
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
        if (pinchRecognizer.State != GestureRecognizerState.Ended) {
            selectionManager.CancelSelection();
            creatureBuilder.CancelTemporaryBodyParts();
            bool needsCreatureReset = jointsToMove.Count > 0;
            jointsToMove.Clear();
            if (needsCreatureReset) {
                SetState(creatureBuilder.GetDesign());
            }
            return;
        }

        if (!InputRegistry.shared.MayHandle(InputType.Click | InputType.Touch, this)) return;

        bool isPointerOverUI = false;
        isPointerOverUI |= EventSystem.current.IsPointerOverGameObject();
        if (Input.touchCount > 0) {
            isPointerOverUI |= InputUtils.IsTouchOverUI(Input.GetTouch(0).fingerId);
        }

        var clickWorldPos = Camera.main.ScreenToWorldPoint(InputUtils.GetMousePosition());
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

            case Tool.Decoration:
                var hoveringBone = selectionManager.GetSingleSelected<Bone>();
                var closestBone = hoveringBone != null ? hoveringBone : FindClosestBone(clickWorldPos);
                if (closestBone != null) {
                    creatureBuilder.CreateDecorationFromBone(closestBone, clickWorldPos, SelectedDecorationType);
                }
                // TODO: If there is no bone yet, notify the user that they should add one
                break;

            case Tool.Move:
                selectionManager.AddCurrentHoveringToSelection();
                jointsToMove = selectionManager.GetJointsToMoveFromSelection();
                lastDragPosition = clickWorldPos;
                if (grid.gameObject.activeSelf && jointsToMove.Count > 0) {
                    // Snap the closest joint to the grid
                    Joint closestJoint = null;
                    float closestDistance = float.MaxValue;
                    foreach (var jointToMove in jointsToMove) {
                        var distance = Vector3.Distance(jointToMove.center, clickWorldPos);
                        if (distance < closestDistance) {
                            closestJoint = jointToMove;
                            closestDistance = distance;
                        }
                    }
                    lastDragPosition = closestJoint.center + (clickWorldPos - grid.ClosestPointOnGrid(closestJoint.center));
                }
                break;

            case Tool.Select:
                selectionManager.DeselectAll();
                selectionManager.StartSelection(clickWorldPos); 
                break;
            default: break;
            
            }

            viewController.Refresh();
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

            case Tool.Decoration:
                creatureBuilder.UpdateCurrentDecoration(clickWorldPos);
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
                #if !UNITY_EDITOR
                #if UNITY_IOS || UNITY_ANDROID
                if (Input.touchCount == 0) return;
                #endif
                #endif
                if (GestureRecognizerCollection.shared.GetClickGestureRecognizer().ClickEndedOnThisFrame()) {
                    creatureEdited = creatureBuilder.TryPlacingJoint(clickWorldPos); 
                }
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
            
            case Tool.Decoration:
                creatureEdited = creatureBuilder.PlaceCurrentDecoration();
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
                var newSelection = selectionManager.GetSelectedIds();
                if (viewController.IsShowingBasicSettingsControls () || 
                    !newSelection.SetEquals(this.selectionAtBeginningOfDrag)) {
                    OnSelectionChanged();
                }
                break;

            default: break;
            }

            if (creatureEdited) {
                historyManager.Push(oldDesign);
                if (selectedTool == Tool.Move) {
                    creatureBuilder.RefreshMuscleColliders();
                }
            }

            viewController.Refresh();
        }

        if (InputUtils.MouseNotDown()) {
            creatureBuilder.CancelTemporaryBodyParts();
        }
    }

    
    /// <summary>
	/// Handles all possible keyboard controls / editor shortcuts.
	/// </summary>
    private void HandleKeyboardInput() {

        if (!Input.anyKeyDown) return;

        if (!InputRegistry.shared.MayHandle(InputType.Key, this)) return;

        if (EventSystem.current.currentSelectedGameObject != null) return;

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

        // DEBUG:
        else if (input.GetKeyDown(KeyCode.C)) {
            SelectedTool = Tool.Decoration;
        }

        else if (input.GetKeyDown(KeyCode.Escape)) {
            selectionManager.DeselectAll();
            OnSelectionChanged();
        }

        
        else if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            
            // Cmd + Z = Undo
            if (input.GetKeyDown(KeyCode.Z)) {
                Undo();
                
            } 

            // Cmd + Shift + Z = Redo
            else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && 
                    input.GetKeyDown(KeyCode.Z)) {
                Redo();
            }
            
            // Cmd + Y = Redo
            else if (input.GetKeyDown(KeyCode.Y)) {
                Redo();
            }
        }

        // DEBUG

        // else if (Input.GetKeyDown(KeyCode.K)) {
        //     var simulationFileManager = FindObjectOfType<SimulationFileManager>();
        //     simulationFileManager.LoadButtonClicked(null);
        // }

        // else if (Input.GetKeyDown(KeyCode.K)) {
        //     Debug.Log(historyManager.GetDebugState());   
        // }

        viewController.Refresh();
    }

    private void OnToolChanged(Tool tool) {
        if (tool == Tool.Joint || tool == Tool.Bone || tool == Tool.Muscle || tool == Tool.Decoration) {
            selectionManager.DeselectAll();
            OnSelectionChanged();
        }
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

    private void OnSelectionChanged() {
        ReloadSettingsControls();
    }

    private void DeleteCurrentSelection() {

        if (selectionManager.IsAnythingSelected()) {
            var oldDesign = creatureBuilder.GetDesign();
            var deleted = selectionManager.DeleteSelection(this.creatureBuilder);
            if (deleted) {
                OnSelectionChanged();
                historyManager.Push(oldDesign);
                viewController.Refresh();
            }
        }
    }

    public BodyComponent FindBodyComponentWithId(int id) {
        return creatureBuilder.FindWithId(id);
    }

    private Bone FindClosestBone(Vector3 position) {
        var closestDistance = float.PositiveInfinity;
        int? closestBoneIndex = null;
        for (int i = 0; i < this.creatureBuilder.bones.Count; i++) {
            float distToBone = Vector3.Distance(this.creatureBuilder.bones[i].Center, position);
            if (distToBone < closestDistance) {
                closestDistance = distToBone;
                closestBoneIndex = i;
            }
        } 
        if (closestBoneIndex.HasValue) {
            return this.creatureBuilder.bones[closestBoneIndex.Value];
        }
        return null;
    }

    private void ReloadSettingsControls() {
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
        else if (part is Decoration) {
            advancedSettingsManager = new DecorationSettingsManager(part as Decoration, viewController.ShowAdvancedSettingsControls());
        }
        advancedSettingsManager.dataWillChange += delegate () {
            historyManager.Push(creatureBuilder.GetDesign());
        };
    }

    #endregion
    #region IEditorViewControllerDelegate 

    public bool CanUndo(EditorViewController viewController) {
        return historyManager.CanUndo();
    }

    public bool CanRedo(EditorViewController viewController) {
        return historyManager.CanRedo();
    }

    public void RefreshAfterUndoRedo() {
        // Refresh the current body part instance of the advancedSettingsManager if necessary
        selectionManager.RefreshSelectionAfterUndoRedo();
        if (advancedSettingsManager != null) {
            ReloadSettingsControls();
        }
    }

    #endregion
}