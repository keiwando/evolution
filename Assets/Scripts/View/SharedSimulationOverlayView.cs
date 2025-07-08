using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface ISharedSimulationOverlayViewDelegate {   
    
    bool IsAutoSaveEnabled(SharedSimulationOverlayView view);
    bool IsPlaybackPossiblyInaccurate(SharedSimulationOverlayView view);

    void InaccuratePlaybackButtonClicked(SharedSimulationOverlayView view);
    void PauseButtonClicked(SharedSimulationOverlayView view);
    void BackButtonClicked(SharedSimulationOverlayView view);
    void SaveToGalleryButtonClicked(SharedSimulationOverlayView view);
    void SaveButtonClicked(SharedSimulationOverlayView view);
    
    void AutosaveToggled(SharedSimulationOverlayView view, bool autosaveEnabled);
}

public class SharedSimulationOverlayView: MonoBehaviour {

    public ISharedSimulationOverlayViewDelegate Delegate { get; set; }

    [SerializeField] private Button inaccuratePlaybackButton;

    [SerializeField] private Button pauseButton;

    [SerializeField] private Toggle autosaveToggle;
    [SerializeField] private Button saveButton;
    [SerializeField] private GameObject saveMenu;
    [SerializeField] private Button saveToGalleryButton;
    [SerializeField] private Button saveSimulationButton;
    [SerializeField] private Button aboutSavingButton;
    [SerializeField] private Button cancelSaveMenuButton;
    [SerializeField] private Text successfulSaveLabel;

    [SerializeField] private Button backButton;

    // MARK: - Animations
    private Coroutine savedLabelFadeRoutine;

    void Start() {

        this.saveMenu.gameObject.SetActive(false);

        backButton.onClick.AddListener(delegate () {
            Delegate.BackButtonClicked(this);
        });

        pauseButton.onClick.AddListener(delegate () {
            Delegate.PauseButtonClicked(this);
        });

        saveButton.onClick.AddListener(delegate () {
            ToggleSaveMenu();
        });

        saveToGalleryButton.onClick.AddListener(delegate () {
            Delegate.SaveToGalleryButtonClicked(this);
            HideSaveMenu();
        });

        saveSimulationButton.onClick.AddListener(delegate () {
            Delegate.SaveButtonClicked(this);
            HideSaveMenu();
        });

        autosaveToggle.onValueChanged.AddListener(delegate (bool value) {
            Delegate.AutosaveToggled(this, value);
        });

        cancelSaveMenuButton.onClick.AddListener(delegate () {
            HideSaveMenu();
        });

        inaccuratePlaybackButton.onClick.AddListener(delegate () {
            Delegate.InaccuratePlaybackButtonClicked(this);
        });
        successfulSaveLabel.gameObject.SetActive(false);

        // Refresh();
    }

    public void Refresh() {

        autosaveToggle.isOn = Delegate.IsAutoSaveEnabled(this);
        inaccuratePlaybackButton.gameObject.SetActive(Delegate.IsPlaybackPossiblyInaccurate(this));
    }

    private void ToggleSaveMenu() {
        this.saveMenu.gameObject.SetActive(!this.saveMenu.gameObject.activeSelf);
    }

    private void HideSaveMenu() {
        this.saveMenu.gameObject.SetActive(false);
    }

    public void ShowSuccessfulSaveAlert() {

        if (savedLabelFadeRoutine != null) {
			StopCoroutine(savedLabelFadeRoutine);	
		}

        var canvasGroup = successfulSaveLabel.GetComponent<CanvasGroup>();
        savedLabelFadeRoutine = StartCoroutine(AnimationUtils.Flash(canvasGroup, 1f, 3.5f));
    }
}