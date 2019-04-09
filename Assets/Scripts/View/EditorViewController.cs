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

    private EditorMode editorMode {
        get { return (EditorMode)Settings.EditorMode; }
        set { Settings.EditorMode = (int)value; }
    }

    void Start() {
        RefreshEditorViews();
        SetupActions();
    }

    public void Refresh() {
        RefreshEditorViews();
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
}