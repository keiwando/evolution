using UnityEngine;
using UnityEngine.UI;

enum EditorMode {
    Basic = 0,
    Advanced = 1
}

public class EditorViewController: MonoBehaviour {

    [SerializeField]
    private GameObject basicControls;
    [SerializeField]
    private GameObject advancedControls;
    [SerializeField]
    private Button editorModeToggle;
    [SerializeField]
    private ButtonManager buttonManager;
    [SerializeField]
    private CreatureDesignControlsView creatureDesignControlsView;

    [SerializeField]
    private Button undoButton;
    [SerializeField]
    private Button redoButton;
    [SerializeField]
    private HistoryManager historyManager;

    private EditorMode editorMode {
        get { return (EditorMode)Settings.EditorMode; }
        set { Settings.EditorMode = (int)value; }
    }

    void Start() {
        Refresh();
        SetupActions();
    }

    public void Refresh() {
        RefreshEditorViews();
        RefreshUndoButtons();
    }

    private void SetupActions() {
        
        editorModeToggle.onClick.AddListener(delegate () {
            if (editorMode == EditorMode.Basic) {
                editorMode = EditorMode.Advanced;
            } else {
                editorMode = EditorMode.Basic;
            }
            RefreshEditorViews();
        });
    }

    private void RefreshEditorViews() {

        var advancedMode = editorMode == EditorMode.Advanced;

        var buttonAlpha = advancedMode ? 1f : 0.5f;
        var modeToggleImage = editorModeToggle.GetComponent<Image>();
        var oldColor = modeToggleImage.color;
        oldColor.a = buttonAlpha;
        modeToggleImage.color = oldColor;

        basicControls.SetActive(!advancedMode);
        advancedControls.SetActive(advancedMode);
        buttonManager.Refresh();
        // TODO: Refresh Creature Design Controls View
    }

    private void RefreshUndoButtons() {

        float disabledAlpha = 0.5f;
        var undoEnabled = historyManager.CanUndo();
        undoButton.interactable = undoEnabled;
        var undoCanvasGroup = undoButton.GetComponent<CanvasGroup>();
        // For some reason the CanvasGroups end up getting destroyed sometimes
        if (undoCanvasGroup) {
            undoCanvasGroup.alpha = undoEnabled ? 1f : disabledAlpha;
        }

        var redoEnabled = historyManager.CanRedo();
        redoButton.interactable = redoEnabled;
        var redoCanvasGroup = redoButton.GetComponent<CanvasGroup>();
        if (redoCanvasGroup) {
            redoCanvasGroup.alpha = redoEnabled ? 1f : disabledAlpha;
        }
    }
}