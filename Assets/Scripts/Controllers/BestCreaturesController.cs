using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BestCreaturesController : MonoBehaviour {

	[SerializeField] new private CameraFollowScript camera;

	public Creature CurrentBest { get; private set; }

	/// <summary>
	/// The generation of the currently showing best creature.
	/// </summary>
	public int CurrentGeneration { get; private set; }

	public bool AutoplayEnabled { 
		get { return autoplayEnabled; } 
		set {
			autoplayEnabled = value;
			if (value) {
				AutoPlay();
			} else {
				StopAutoPlay();
			}
		}
	}
	private bool autoplayEnabled;

	public int AutoplayDuration { get; set; }

	private Coroutine autoplayRoutine;
	private Coroutine playbackRoutine;

	private Evolution evolution;

	private UnityEngine.SceneManagement.Scene playbackScene;
	private PhysicsScene physicsScene;

	void Start () {

		Physics.autoSimulation = false;

		evolution = FindObjectOfType<Evolution>();

		AutoplayDuration = 10;
		AutoplayEnabled = true;

		evolution.InitializationDidEnd += delegate () {
			AutoplayDuration = evolution.Settings.SimulationTime;
		};

		evolution.NewGenerationDidBegin += delegate () {
			if (CurrentBest == null && GenerationHasBeenSimulated(1)) {
				ShowBestCreature(evolution.SimulationData.BestCreatures.Count);
			}
		};
	}

	void FixedUpdate() {
		if (physicsScene != null && physicsScene.IsValid()) {
			physicsScene.Simulate(Time.fixedDeltaTime);
		}
	}

	public void ShowBestCreature(int generation) {
		
		if (this.playbackRoutine != null) 
			StopCoroutine(this.playbackRoutine);
		this.playbackRoutine = StartCoroutine(StartPlayback(generation));
	}

	public IEnumerator StartPlayback(int generation) {

		var lastSimulatedGeneration = evolution.SimulationData.BestCreatures.Count;
		if (generation < 1 || generation > lastSimulatedGeneration) {
			Debug.LogError(string.Format("Attempted to show invalid generation: {0}. Simulated up to {1}", generation, lastSimulatedGeneration));
			yield break;
		}

		if (this.playbackScene != null && this.playbackScene.IsValid()) {
			yield return SceneController.UnloadAsync(this.playbackScene);
		}

		var sceneLoadConfig = new SceneController.SimulationSceneLoadConfig(
			this.evolution.SimulationData.CreatureDesign,
			1,
			this.evolution.SimulationData.SceneDescription,
			SceneController.SimulationSceneType.BestCreatures
		);
		var context = new SceneController.SimulationSceneLoadContext();

		yield return SceneController.LoadSimulationScene(sceneLoadConfig, context);
		this.physicsScene = context.PhysicsScene;
		this.playbackScene = context.Scene;

		var chromosome = evolution.SimulationData.BestCreatures[generation - 1].Chromosome;
		this.CurrentBest = context.Creatures[0];
		evolution.ApplyBrain(this.CurrentBest, chromosome);
		// TODO: Set Obstacle if needed

		camera.toFollow = this.CurrentBest;
		
		this.CurrentBest.SetOnBestCreatureLayer();

		this.CurrentBest.Alive = false;
		this.CurrentBest.gameObject.SetActive(false);

		this.CurrentBest.Alive = true;
		this.CurrentBest.gameObject.SetActive(true);

		AutoPlay();
		CurrentGeneration = generation;
	}

	private void AutoPlay() {

		StopAutoPlay();
		if (this.CurrentBest == null) return;
		if (!AutoplayEnabled) return;

		autoplayRoutine = StartCoroutine(ShowNextGenerationAfterTime(AutoplayDuration));
	}

	private IEnumerator ShowNextGenerationAfterTime(float time) {

		yield return new WaitForSeconds(time);

		// Check to see if the next generation has been simulated yet,
		// otherwise wait for a shorter period of time again.
		while (!GenerationHasBeenSimulated(CurrentGeneration + 1)) {
			// yield return new WaitForSeconds(time / 3);
			yield return new WaitForSeconds(time / 30.0f);
		}

		ShowBestCreature(CurrentGeneration + 1);
	}

	private void StopAutoPlay() {

		if (autoplayRoutine != null) StopCoroutine(autoplayRoutine);
	}

	private bool GenerationHasBeenSimulated(int generation) {

		return generation > 0 && generation <= evolution.SimulationData.BestCreatures.Count;
	}

	public void RefreshMuscleContractionVisibility(bool visible) {
		if (CurrentBest != null) 
			CurrentBest.RefreshMuscleContractionVisibility(visible);
	}
}