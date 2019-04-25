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

	private Evolution evolution;

	void Start () {

		evolution = FindObjectOfType<Evolution>();

		AutoplayEnabled = true;
		AutoplayDuration = 10;

		AutoplayDuration = evolution.Settings.SimulationTime;

		evolution.NewGenerationDidBegin += delegate () {
			if (CurrentBest == null && GenerationHasBeenSimulated(1)) {
				ShowBestCreature(evolution.SimulationData.BestCreatures.Count);
			}
		};
	}

	public void RefreshMuscleContractionVisibility(bool visible) {
		if (CurrentBest != null) 
			CurrentBest.RefreshMuscleContractionVisibility(visible);
	}

	public void ShowBestCreature(int generation) {

		var lastSimulatedGeneration = evolution.SimulationData.BestCreatures.Count;
		if (generation < 1 || generation > lastSimulatedGeneration) {
			Debug.LogError(string.Format("Attempted to show invalid generation: {0}. Simulated up to {1}", generation, lastSimulatedGeneration));
			return;
		}
		
		var chromosome = evolution.SimulationData.BestCreatures[generation - 1].Chromosome;
		SpawnCreature(chromosome);
		AutoPlay();

		CurrentGeneration = generation;
	}

	private void SpawnCreature(string chromosome) {

		var obstacle = CurrentBest.Obstacle;

		if (CurrentBest != null) {
			Destroy(CurrentBest.gameObject);
		}

		var creature = evolution.CreateCreature();
		evolution.ApplyBrain(creature, chromosome);
		creature.Obstacle = obstacle;
	
		camera.toFollow = creature;
		this.CurrentBest = creature;

		creature.SetOnBestCreatureLayer();

		// THIS IS NEEDED! DO NOT REMOVE! IMPORTANT!
		creature.Alive = false;
		creature.gameObject.SetActive(false);

		creature.Alive = true;
		creature.gameObject.SetActive(true);
	}

	private void AutoPlay() {

		StopAutoPlay();
		if (!AutoplayEnabled) return;

		autoplayRoutine = StartCoroutine(ShowNextGenerationAfterTime(AutoplayDuration));
	}

	private IEnumerator ShowNextGenerationAfterTime(float time) {

		yield return new WaitForSeconds(time);

		// Check to see if the next generation has been simulated yet,
		// otherwise wait for 1 / 3 of time again.
		while (!GenerationHasBeenSimulated(CurrentGeneration + 1)) {
			yield return new WaitForSeconds(time / 3);
		}

		ShowBestCreature(CurrentGeneration + 1);
	}

	private void StopAutoPlay() {
		
		if (autoplayRoutine != null) StopCoroutine(autoplayRoutine);
	}

	private bool GenerationHasBeenSimulated(int generation) {
		return generation > 0 && generation < evolution.SimulationData.BestCreatures.Count;
	}
}
