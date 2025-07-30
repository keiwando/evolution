using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public class EditorViewController: MonoBehaviour {

        public CreatureEditor editor { get; set; }

        [SerializeField] private TogglingSlidingContainer settingsControlsContainer;
        [SerializeField] private TogglingSlidingContainer advancedBodyControlsContainer;

        [SerializeField] private BasicSettingsView basicSettingsView;
        [SerializeField] private AdvancedBodyControlsViewController advancedBodyControlsViewController;
        
        [SerializeField] private ButtonManager buttonManager;
        [SerializeField] private CreatureDesignControlsView creatureDesignControlsView;

        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;

        [SerializeField] private Button editorActionsMenuButton;
        [SerializeField] private GameObject editorActionsMenu;
        [SerializeField] private Button editorActionBringForwardButton;
        [SerializeField] private Button editorActionSendBackwardButton;

        [SerializeField] private GameObject migrationNotice;

        void Start() {
            basicSettingsView.Delegate = new SettingsManager();

            undoButton.onClick.AddListener(delegate () {
                editor.Undo();
                RefreshAfterUndoRedo();
            });

            redoButton.onClick.AddListener(delegate () {
                editor.Redo();
                RefreshAfterUndoRedo();
            });

            editorActionsMenuButton.onClick.AddListener(delegate () {
                bool decorationIsSelected = editor.selectionManager.SelectionOnlyContainsType(BodyComponentType.Decoration);
                editorActionBringForwardButton.gameObject.SetActive(decorationIsSelected);
                editorActionSendBackwardButton.gameObject.SetActive(decorationIsSelected);
                editorActionsMenu.gameObject.SetActive(true);
            });

            editorActionBringForwardButton.onClick.AddListener(delegate () {
                editor.BringSelectedDecorationsForward();
            });
            editorActionSendBackwardButton.onClick.AddListener(delegate () {
                editor.SendSelectedDecorationsBackward();
            });

            CreatureSerializer.MigrationDidBegin += delegate () {
                migrationNotice.SetActive(true);
            };
            CreatureSerializer.MigrationDidEnd += delegate () {
                migrationNotice.SetActive(false);
            };
        }

        public void ShowBasicSettingsControls() {

            if (settingsControlsContainer.LastSlideDirection == SlidingContainer.Direction.Right) {
                settingsControlsContainer.Slide(SlidingContainer.Direction.Left, 0.3f, false);
            }
            if (advancedBodyControlsContainer.LastSlideDirection == SlidingContainer.Direction.Left) {
                advancedBodyControlsContainer.Slide(SlidingContainer.Direction.Right, 0.3f, false);
            }
        }

        public AdvancedBodyControlsViewController ShowAdvancedSettingsControls() {

            if (advancedBodyControlsContainer.LastSlideDirection == SlidingContainer.Direction.Right) {
                advancedBodyControlsContainer.Slide(SlidingContainer.Direction.Left, 0.3f, false);
            }
            if (settingsControlsContainer.LastSlideDirection == SlidingContainer.Direction.Left) {
                settingsControlsContainer.Slide(SlidingContainer.Direction.Right, 0.3f, false);
            }
            return advancedBodyControlsViewController;
        }

        public bool IsShowingBasicSettingsControls() {
            return settingsControlsContainer.LastSlideDirection == SlidingContainer.Direction.Left;
        }

        public void Refresh() {
            RefreshUndoButtons();
            basicSettingsView.Refresh();
            creatureDesignControlsView.SetCurrentCreatureName(editor?.GetCreatureName() ?? "Unnamed");
            
            bool decorationIsSelected = editor.selectionManager.SelectionOnlyContainsType(BodyComponentType.Decoration);
            editorActionsMenuButton.gameObject.SetActive(decorationIsSelected);
            if (!editorActionsMenuButton.gameObject.activeSelf) {
                editorActionsMenu.gameObject.SetActive(false);
            }
        }

        private void RefreshAfterUndoRedo() {
            RefreshUndoButtons();
            editor?.RefreshAfterUndoRedo();
        }

        private void RefreshUndoButtons() {

            float disabledAlpha = 0.5f;
            var undoEnabled = editor.CanUndo(this);
            undoButton.interactable = undoEnabled;
            var undoCanvasGroup = undoButton.GetComponent<CanvasGroup>();
            // For some reason the CanvasGroups end up getting destroyed sometimes
            if (undoCanvasGroup) {
                undoCanvasGroup.alpha = undoEnabled ? 1f : disabledAlpha;
            }

            var redoEnabled = editor.CanRedo(this);
            redoButton.interactable = redoEnabled;
            var redoCanvasGroup = redoButton.GetComponent<CanvasGroup>();
            if (redoCanvasGroup) {
                redoCanvasGroup.alpha = redoEnabled ? 1f : disabledAlpha;
            }
        }
    }
}