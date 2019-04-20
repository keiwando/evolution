using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationViewController : MonoBehaviour, 
										IEvolutionOverlayViewDelegate, 
										ISharedSimulationOverlayViewDelegate,
										IBestCreaturesOverlayViewDelegate  {

	enum VisibleScreen {
		Simulation,
		BestCreatures
	}

	private Evolution evolution;
	private CameraFollowController cameraFollowController;
	private BestCreaturesController bestCreatureController;
	private EvolutionPauseMenu pauseView;

	private EvolutionOverlayView evolutionOverlayView;
	private BestCreaturesOverlayView bestCreatureOverlayView;
	private SharedSimulationOverlayView sharedOverlayView;

	private VisibleScreen visibleScreen = VisibleScreen.Simulation;

	void Start () {

		bestCreatureController = FindObjectOfType<BestCreaturesController>();
		cameraFollowController = FindObjectOfType<CameraFollowController>();
		evolution = FindObjectOfType<Evolution>();

		evolutionOverlayView = FindObjectOfType<EvolutionOverlayView>();
		evolutionOverlayView.Delegate = this;
		bestCreatureOverlayView = FindObjectOfType<BestCreaturesOverlayView>();
		bestCreatureOverlayView.Delegate = this;
		sharedOverlayView = FindObjectOfType<SharedSimulationOverlayView>();
		sharedOverlayView.Delegate = this;

		Refresh();

		showOneAtATimeToggle.onValueChanged.AddListener(delegate(bool arg0) {
			evolution.Settings.showOneAtATime = arg0;
			cameraFollowController.RefreshVisibleCreatures();
		});

		showMuscleContractionToggle.isOn = PlayerPrefs.GetInt(PlayerPrefsKeys.SHOW_MUSCLE_CONTRACTION, 0) == 1;
		showMuscleContractionToggle.onValueChanged.AddListener(delegate(bool arg0) {
			PlayerPrefs.SetInt(PlayerPrefsKeys.SHOW_MUSCLE_CONTRACTION, arg0 ? 1 : 0);
			PlayerPrefs.Save();
			cameraFollowController.RefreshVisibleCreatures();
			bcController.RefreshMuscleContractionVisibility();
		});
	}

	public void Refresh() {

		sharedOverlayView.Refresh();

		evolutionOverlayView.gameObject.SetActive(visibleScreen == VisibleScreen.Simulation);
		bestCreatureOverlayView.gameObject.SetActive(visibleScreen == VisibleScreen.BestCreatures);

		// TODO: Connect cameras to correct render textures 

		if (visibleScreen == VisibleScreen.Simulation) {
			evolutionOverlayView.Refresh();
		} else {
			bestCreatureOverlayView.Refresh();
		}
	}

	public void FocusOnNextCreature() {
		cameraFollowController.FocusOnNextCreature();
	}

	public void FocusOnPreviousCreature() {
		cameraFollowController.FocusOnPreviousCreature();
	}

	public void GoBackToEditor() {
		// TODO: Show confirmation
		evolution.Finish();
        SceneController.LoadSync(SceneController.Scene.Editor);
	}

	private void Pause() {
		pauseView.Pause();
	}

	private void SaveSimulation() {
		// TODO: Implement
	}
	

	#region ISharedSimulationOverlayViewDelegate

	public bool IsAutoSaveEnabled(SharedSimulationOverlayView view) {
		return evolution.AutoSaver.Enabled;
	}

    public void PauseButtonClicked(SharedSimulationOverlayView view) {
		Pause();
	}

    public void BackButtonClicked(SharedSimulationOverlayView view) {
		GoBackToEditor();
	}

    public void SaveButtonClicked(SharedSimulationOverlayView view) {
		SaveSimulation();
	}
    
    public void AutosaveToggled(SharedSimulationOverlayView view, bool autosaveEnabled) {
		evolution.AutoSaver.Enabled = autosaveEnabled;
	}

	#endregion
	#region IEvolutionOverlayViewDelegate

	public void DidClickOnPIPView(EvolutionOverlayView view) {

		if (visibleScreen == VisibleScreen.Simulation) {
			visibleScreen = VisibleScreen.BestCreatures;
		} else {
			visibleScreen = VisibleScreen.Simulation;
		} 
		Refresh();
	}

    public int GetCurrentGenerationNumber(EvolutionOverlayView view) {
		return evolution.CurrentGenerationNumber;
	}

    public int GetGurrentBestOfGenerationNumber(EvolutionOverlayView view) {
		return bestCreatureController.CurrentGeneration;
	}

    public int GetCurrentBatchNumber(EvolutionOverlayView view) {
		return evolution.CurrentBatchNumber;
	}

    public int GetTotalBatchCount(EvolutionOverlayView view) {
		return (int)Math.Ceiling(((float)evolution.Settings.populationSize / evolution.CurrentCreatureBatch.Length));
	}

    public bool IsSimulatingInBatches(EvolutionOverlayView view) {
		return evolution.IsSimulatingInBatches;
	}

    public bool ShouldShowOneAtATime(EvolutionOverlayView view) {
		return true;
		// TODO: Replace after Compile
		// return Settings.ShowOneAtATime;
	}

    public bool ShouldShowMuscleContraction(EvolutionOverlayView view) {
		return true;
		// TODO: Replace after Compile
		// return Settings.ShowMuscleContraction;
	}

	#endregion
	#region IBestCreaturesOverlayViewDelegate

	public void SelectedGeneration(BestCreaturesOverlayView view, int generation) {
		bestCreatureController.GenerationSelected(generation);
	}

    public void DidChangeAutoplayEnabled(BestCreaturesOverlayView view, bool enabled) {
		bestCreatureController.AutoplayEnabled = enabled;
	}

    public void DidChangeAutoplayDuration(BestCreaturesOverlayView view, int duration) {
		bestCreatureController.AutoplayDuration = duration;
	}

    public void DidClickOnPIPView(BestCreaturesOverlayView view) {

	}
    
    bool IsAutoplayEnabled(BestCreaturesOverlayView view);
    int GetAutoplayDuration(BestCreaturesOverlayView view);
    int GetMaxAutoplayDuration(BestCreaturesOverlayView view);
    
    int GetCurrentSimulationGeneration(BestCreaturesOverlayView view);
    int GetGenerationOfCurrentBest(BestCreaturesOverlayView view);
    CreatureStats GetCreatureStatsOfCurrentBest(BestCreaturesOverlayView view);
    NeuralNetworkSettings GetNetworkSettingsOfCurrentBest(BestCreaturesOverlayView view);
    int GetNumberOfNetworkInputs(BestCreaturesOverlayView view);

	#endregion

}
