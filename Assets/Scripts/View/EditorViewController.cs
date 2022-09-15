using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public interface IEditorViewControllerDelegate {
        string GetCreatureName();
        bool CanUndo(EditorViewController viewController);
        bool CanRedo(EditorViewController viewController);
        void Undo();
        void Redo();
        void RefreshAfterUndoRedo();
    }

    public class EditorViewController: MonoBehaviour {

        public IEditorViewControllerDelegate Delegate { get; set; }

        [SerializeField]
        private TogglingSlidingContainer settingsControlsContainer;
        [SerializeField]
        private TogglingSlidingContainer advancedBodyControlsContainer;

        [SerializeField]
        private BasicSettingsView basicSettingsView;
        [SerializeField]
        private AdvancedBodyControlsViewController advancedBodyControlsViewController;
        
        [SerializeField]
        private ButtonManager buttonManager;
        [SerializeField]
        private CreatureDesignControlsView creatureDesignControlsView;

        [SerializeField]
        private Button undoButton;
        [SerializeField]
        private Button redoButton;

        [SerializeField]
        private GameObject migrationNotice;

        void Start() {
            basicSettingsView.Delegate = new SettingsManager();

            undoButton.onClick.AddListener(delegate () {
                Delegate.Undo();
                RefreshAfterUndoRedo();
            });

            redoButton.onClick.AddListener(delegate () {
                Delegate.Redo();
                RefreshAfterUndoRedo();
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

        public void Refresh() {
            RefreshUndoButtons();
            basicSettingsView.Refresh();
            creatureDesignControlsView.SetCurrentCreatureName(Delegate?.GetCreatureName() ?? "Unnamed");
        }

        private void RefreshAfterUndoRedo() {
            RefreshUndoButtons();
            Delegate?.RefreshAfterUndoRedo();
        }

        private void RefreshUndoButtons() {

            float disabledAlpha = 0.5f;
            var undoEnabled = Delegate.CanUndo(this);
            undoButton.interactable = undoEnabled;
            var undoCanvasGroup = undoButton.GetComponent<CanvasGroup>();
            // For some reason the CanvasGroups end up getting destroyed sometimes
            if (undoCanvasGroup) {
                undoCanvasGroup.alpha = undoEnabled ? 1f : disabledAlpha;
            }

            var redoEnabled = Delegate.CanRedo(this);
            redoButton.interactable = redoEnabled;
            var redoCanvasGroup = redoButton.GetComponent<CanvasGroup>();
            if (redoCanvasGroup) {
                redoCanvasGroup.alpha = redoEnabled ? 1f : disabledAlpha;
            }
        }
    }
}