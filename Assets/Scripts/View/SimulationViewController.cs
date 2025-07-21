using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Keiwando;
using Keiwando.Evolution;
using Keiwando.Evolution.UI;

public class SimulationViewController : MonoBehaviour, 
										IEvolutionOverlayViewDelegate, 
										IBestCreaturesOverlayViewDelegate,
										ISimulationVisibilityOptionsViewDelegate,
										IPauseViewControllerDelegate  {

	enum VisibleScreen {
		Simulation,
		BestCreatures
	}

	/// <summary>
	/// The number of seconds that have to pass since the most recent save after which a confirmation
	/// dialog should be displayed if the user tries to exit the simulation.
	/// </summary>
	const int EXIT_CONFIRMATION_TIME_DELTA = 15 * 10;

	private Evolution evolution;
	private CameraFollowController cameraFollowController;
	private BestCreaturesController bestCreatureController;
	
	[SerializeField] private PauseViewController pauseViewController;

	[SerializeField] private EvolutionOverlayView evolutionOverlayView;
	[SerializeField] private BestCreaturesOverlayView bestCreatureOverlayView;
	[SerializeField] private SharedSimulationOverlayView sharedOverlayView;

	[SerializeField] private SimulationVisibilityOptionsView visibilityOptionsView;

	[SerializeField] private SimulationExitConfirmationView exitConfirmationPopup;

    [SerializeField] private Camera simulationCamera;
    [SerializeField] private Camera bestCreatureCamera;

	private VisibleScreen visibleScreen = VisibleScreen.Simulation;

	void Start () {

		bestCreatureController = FindAnyObjectByType<BestCreaturesController>();
		cameraFollowController = FindAnyObjectByType<CameraFollowController>();
		evolution = FindAnyObjectByType<Evolution>();

		evolutionOverlayView.Delegate = this;
		bestCreatureOverlayView.Delegate = this;
		sharedOverlayView.simulationViewController = this;
		visibilityOptionsView.Delegate = this;

		evolution.NewBatchDidBegin += delegate () {
			Refresh();
		};

		evolution.SimulationWasSaved += delegate () {
			sharedOverlayView.ShowSuccessfulSaveAlert();
		};

		evolution.CreatureWasSavedToGallery += delegate () {
			sharedOverlayView.ShowSuccessfulSaveAlert();
		};

		bestCreatureController.PlaybackDidBegin += delegate () {
			Refresh();
		};
	}

	public void Refresh() {

		sharedOverlayView.Refresh();

		evolutionOverlayView.gameObject.SetActive(visibleScreen == VisibleScreen.Simulation);
		bestCreatureOverlayView.gameObject.SetActive(visibleScreen == VisibleScreen.BestCreatures);

		ConnectCameraOutputs();

		if (visibleScreen == VisibleScreen.Simulation) {
			evolutionOverlayView.Refresh();
			visibilityOptionsView.Refresh();
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
		
		int fullGenerationsSinceLastSave = Math.Max(0, evolution.CurrentGenerationNumber - 1 - evolution.LastSavedGeneration); 
		int batchesPerGeneration = evolution.Settings.SimulateInBatches ? evolution.Settings.BatchSize : 1;
		int secondsPerGeneration = evolution.Settings.SimulationTime * batchesPerGeneration;
		int secondsSinceLastSave = secondsPerGeneration * fullGenerationsSinceLastSave;
		if (fullGenerationsSinceLastSave < 1 || secondsSinceLastSave < EXIT_CONFIRMATION_TIME_DELTA) {
			evolution.Finish();
			SceneController.LoadSync(SceneController.Scene.Editor);		
			return;
		}

		exitConfirmationPopup.Show(delegate () {
			evolution.Finish();
			SceneController.LoadSync(SceneController.Scene.Editor);		
		}, delegate () {
			exitConfirmationPopup.Close();
		});
	}

	private void Pause() {
		
		evolution.Pause();
		pauseViewController.Delegate = this;
		pauseViewController.Show();
	}

	private void SaveToGallery() {
		evolution.SaveToGallery();
	}

	private void SaveSimulation() {
		evolution.SaveSimulation();
	}

	private void ConnectCameraOutputs() {
			
			if (visibleScreen == VisibleScreen.Simulation) {
					simulationCamera.targetTexture = null;
					bestCreatureCamera.targetTexture = evolutionOverlayView.GetPipRenderTexture();
			} else {
					simulationCamera.targetTexture = bestCreatureOverlayView.GetPipRenderTexture();
					bestCreatureCamera.targetTexture = null;
			}
	}
	
	public bool IsAutoSaveEnabled() {
		return evolution.AutoSaver.Enabled;
	}

	public bool SaveToGalleryIsPossible() {
		return evolution.BestCreatureRecording != null;
	}

  public void PauseButtonClicked() {
		Pause();
	}

  public void BackButtonClicked() {
		GoBackToEditor();
	}

	public void SaveToGalleryButtonClicked() {
		SaveToGallery();
	}

  public void SaveButtonClicked() {
		SaveSimulation();
	}

  public void AutosaveToggled(bool autosaveEnabled) {
		evolution.AutoSaver.Enabled = autosaveEnabled;
	}

	#region IEvolutionOverlayViewDelegate

	public void DidClickOnPipView(EvolutionOverlayView view) {

		visibleScreen = VisibleScreen.BestCreatures;
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
		return (int)Math.Ceiling(((float)evolution.Settings.PopulationSize / evolution.CurrentCreatureBatch.Length));
	}

  public bool IsSimulatingInBatches(EvolutionOverlayView view) {
		return evolution.IsSimulatingInBatches;
	}

    

	#endregion
	#region IBestCreaturesOverlayViewDelegate

	public void SelectedGeneration(BestCreaturesOverlayView view, int generation) {

        generation = Math.Max(1, generation);
        // Check if the selected generation has been simulated yet.
        var lastSimulatedGeneration = evolution.SimulationData.BestCreatures.Count;
        if (lastSimulatedGeneration < generation) {
            view.ShowErrorMessage(string.Format("Generation {0} has not been simulated yet.\n\nCurrently Simulated up to Generation {1}", generation, lastSimulatedGeneration));
            return;
        }

        view.HideErrorMessage();
        bestCreatureController.ShowBestCreature(generation);
        Refresh();
	}

  public void DidChangeAutoplayEnabled(BestCreaturesOverlayView view, bool enabled) {
		bestCreatureController.AutoplayEnabled = enabled;
	}

  public void DidChangeAutoplayDuration(BestCreaturesOverlayView view, int duration) {
		bestCreatureController.AutoplayDuration = duration;
	}

  public void DidClickOnPipView(BestCreaturesOverlayView view) {
		visibleScreen = VisibleScreen.Simulation;
		Refresh();
	}
    
  public bool IsAutoplayEnabled(BestCreaturesOverlayView view) {
		return bestCreatureController.AutoplayEnabled;
	}

  public int GetAutoplayDuration(BestCreaturesOverlayView view) {
		return bestCreatureController.AutoplayDuration;
	}
	
  public int GetCurrentSimulationGeneration(BestCreaturesOverlayView view) {
		return evolution.CurrentGenerationNumber;
	}

  public int GetGenerationOfCurrentBest(BestCreaturesOverlayView view) {
		return bestCreatureController.CurrentGeneration;
	}

  public CreatureStats GetCreatureStatsOfCurrentBest(BestCreaturesOverlayView view) {

			var generation = bestCreatureController.CurrentGeneration;
			evolution.LoadBestCreatureOfGenerationIfNecessary(generation - 1);
			ChromosomeData? bestChromosomeData = evolution.SimulationData.BestCreatures[generation - 1];
			if (bestChromosomeData.HasValue) {
				return bestChromosomeData.Value.Stats;	
			} else {
				Debug.LogError("Best creature data not loaded");
				return new CreatureStats();
			}
	}

	public NeuralNetworkSettings GetNetworkSettingsOfCurrentBest(BestCreaturesOverlayView view) {
			return evolution.NetworkSettings;
	}

	public int GetNumberOfNetworkInputs(BestCreaturesOverlayView view) {
		return evolution.GetNumberOfCurrentBrainInputs();
	}

	public int GetNumberOfNetworkOutputs(BestCreaturesOverlayView view) {
		var currentBest = bestCreatureController.CurrentBest;
		if (currentBest == null) return 0;
		return currentBest.brain.NumberOfOutputs;
	}

	#endregion

	#region ISimulationVisibilityOptionsViewDelegate 

  public void ShowMuscleContractionDidChange(SimulationVisibilityOptionsView view, bool showMuscleContraction) {
		cameraFollowController.RefreshVisibleCreatures();
		bestCreatureController.RefreshMuscleContractionVisibility();
	}
	
	public void ShowMusclesDidChange(SimulationVisibilityOptionsView view, bool showMuscles) {
		cameraFollowController.RefreshVisibleCreatures();
	}

	public Objective GetCurrentTask(SimulationVisibilityOptionsView view) {
		if (evolution == null) {
			return Objective.Running;
		}
		return evolution.SimulationData.Settings.Objective;
	}

	#endregion

	#region IPauseViewControllerDelegate

	public void DidDismiss(PauseViewController viewController) {
		evolution.Resume();
	}

	#endregion
}
