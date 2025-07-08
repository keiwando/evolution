using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SharedSimulationOverlayView: MonoBehaviour {

    public SimulationViewController simulationViewController { get; set; }

    [SerializeField] private Button inaccuratePlaybackButton;

    [SerializeField] private Button pauseButton;

    [SerializeField] private Toggle autosaveToggle;
    [SerializeField] private Button saveButton;
    [SerializeField] private GameObject saveMenu;
    [SerializeField] private Button saveToGalleryButton;
    [SerializeField] private Button saveSimulationButton;
    [SerializeField] private TMP_Text saveToGalleryWaitingForSimulationLabel;
    [SerializeField] private Button aboutSavingButton;
    [SerializeField] private Button cancelSaveMenuButton;
    [SerializeField] private Text successfulSaveLabel;

    [SerializeField] private Button backButton;

    // MARK: - Animations
    private Coroutine savedLabelFadeRoutine;

    void Start() {

        this.saveMenu.gameObject.SetActive(false);

        backButton.onClick.AddListener(delegate () {
            simulationViewController.BackButtonClicked();
        });

        pauseButton.onClick.AddListener(delegate () {
            simulationViewController.PauseButtonClicked();
        });

        saveButton.onClick.AddListener(delegate () {
            ToggleSaveMenu();
        });

        saveToGalleryButton.onClick.AddListener(delegate () {
            simulationViewController.SaveToGalleryButtonClicked();
            HideSaveMenu();
        });

        saveSimulationButton.onClick.AddListener(delegate () {
            simulationViewController.SaveButtonClicked();
            HideSaveMenu();
        });

        autosaveToggle.onValueChanged.AddListener(delegate (bool value) {
            simulationViewController.AutosaveToggled(value);
        });

        cancelSaveMenuButton.onClick.AddListener(delegate () {
            HideSaveMenu();
        });

        inaccuratePlaybackButton.onClick.AddListener(delegate () {
            simulationViewController.InaccuratePlaybackButtonClicked();
        });
        successfulSaveLabel.gameObject.SetActive(false);

        // Can't refresh here. The simulationViewController reference is not set yet. The view controller
        // calls refresh when ready.
        // Refresh();
    }

    public void Refresh() {

        saveToGalleryButton.interactable = simulationViewController.SaveToGalleryIsPossible();
        saveToGalleryWaitingForSimulationLabel.gameObject.SetActive(!saveToGalleryButton.interactable);
        autosaveToggle.isOn = simulationViewController.IsAutoSaveEnabled();
        inaccuratePlaybackButton.gameObject.SetActive(simulationViewController.IsPlaybackPossiblyInaccurate());
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