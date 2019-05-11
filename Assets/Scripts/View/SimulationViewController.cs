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
	
	[SerializeField] private EvolutionPauseMenu pauseView;

	[SerializeField] private EvolutionOverlayView evolutionOverlayView;
	[SerializeField] private BestCreaturesOverlayView bestCreatureOverlayView;
	[SerializeField] private SharedSimulationOverlayView sharedOverlayView;

    [SerializeField] private Camera simulationCamera;
    [SerializeField] private Camera bestCreatureCamera;

	private VisibleScreen visibleScreen = VisibleScreen.Simulation;

	void Start () {

		bestCreatureController = FindObjectOfType<BestCreaturesController>();
		cameraFollowController = FindObjectOfType<CameraFollowController>();
		evolution = FindObjectOfType<Evolution>();

		evolutionOverlayView.Delegate = this;
		bestCreatureOverlayView.Delegate = this;
		sharedOverlayView.Delegate = this;

		evolution.NewBatchDidBegin += delegate () {
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

    private void ConnectCameraOutputs() {
        
        if (visibleScreen == VisibleScreen.Simulation) {
            simulationCamera.targetTexture = null;
            bestCreatureCamera.targetTexture = evolutionOverlayView.GetPipRenderTexture();
        } else {
            simulationCamera.targetTexture = bestCreatureOverlayView.GetPipRenderTexture();
            bestCreatureCamera.targetTexture = null;
        }
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

	public void DidClickOnPipView(EvolutionOverlayView view) {

		visibleScreen = VisibleScreen.BestCreatures;
		Refresh();
	}

    public void ShowOneAtATimeDidChange(EvolutionOverlayView view, bool showOneAtATime) {

        Settings.ShowOneAtATime = showOneAtATime;
        cameraFollowController.RefreshVisibleCreatures();
    }

    public void ShowMuscleContractionDidChange(EvolutionOverlayView view, bool showMuscleContraction) {
        
        Settings.ShowMuscleContraction = showMuscleContraction;
        cameraFollowController.RefreshVisibleCreatures();
        
        bestCreatureController.RefreshMuscleContractionVisibility(Settings.ShowMuscleContraction);
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

    public bool ShouldShowOneAtATime(EvolutionOverlayView view) {
		
		return Settings.ShowOneAtATime;
	}

    public bool ShouldShowMuscleContraction(EvolutionOverlayView view) {
		
		return Settings.ShowMuscleContraction;
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
        var info = evolution.SimulationData.BestCreatures[generation - 1];
        return info.Stats;
    }

    public NeuralNetworkSettings GetNetworkSettingsOfCurrentBest(BestCreaturesOverlayView view) {
        return evolution.NetworkSettings;
    }

    public int GetNumberOfNetworkInputs(BestCreaturesOverlayView view) {
        if (bestCreatureController.CurrentBest != null) {
            return bestCreatureController.CurrentBest.brain.NumberOfInputs;
        }
        // TODO: Improve this implementation
        return 0;
    }

	#endregion
}
