using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface ISharedSimulationOverlayViewDelegate {   
    
    bool IsAutoSaveEnabled(SharedSimulationOverlayView view);

    void PauseButtonClicked(SharedSimulationOverlayView view);
    void BackButtonClicked(SharedSimulationOverlayView view);
    void SaveButtonClicked(SharedSimulationOverlayView view);
    
    void AutosaveToggled(SharedSimulationOverlayView view, bool autosaveEnabled);
}

public class SharedSimulationOverlayView: MonoBehaviour {

    public ISharedSimulationOverlayViewDelegate Delegate { get; set; }

    [SerializeField]
    private Button pauseButton;

    [SerializeField]
    private Toggle autosaveToggle;
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private Text successfulSaveLabel;

    [SerializeField]
    private Button backButton;

    // MARK: - Animations
    private Coroutine savedLabelFadeRoutine;

    void Start() {

        backButton.onClick.AddListener(delegate () {
            Delegate.BackButtonClicked(this);
        });

        pauseButton.onClick.AddListener(delegate () {
            Delegate.PauseButtonClicked(this);
        });

        saveButton.onClick.AddListener(delegate () {
            Delegate.SaveButtonClicked(this);
        });

        autosaveToggle.onValueChanged.AddListener(delegate (bool value) {
            Delegate.AutosaveToggled(this, value);
        });
    }

    public void Refresh() {

        autosaveToggle.isOn = Delegate.IsAutoSaveEnabled(this);
    }

    public void ShowSuccessfulSaveAlert() {

        if (savedLabelFadeRoutine != null) {
			StopCoroutine(savedLabelFadeRoutine);	
		}

        var canvasGroup = successfulSaveLabel.GetComponent<CanvasGroup>();
        savedLabelFadeRoutine = StartCoroutine(AnimationUtils.Flash(canvasGroup, 1f, 3.5f));
    }
}