using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

	public class BestCreaturesController : MonoBehaviour {

		#region Events

		public event Action PlaybackDidBegin;

		#endregion

		[SerializeField] 
		private TrackedCamera trackedCamera;

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

		private Coroutine autoplayRoutine;
		private Coroutine playbackRoutine;

		private Evolution evolution;

		private UnityEngine.SceneManagement.Scene playbackScene;
		private PhysicsScene physicsScene;

		void Start () {

			Physics.simulationMode = SimulationMode.Script;

			evolution = FindAnyObjectByType<Evolution>();
			evolution.BestCreaturesController = this;

			AutoplayEnabled = true;

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
			if (generation < 1 || generation > lastSimulatedGeneration) {
				Debug.LogError(string.Format("Attempted to show invalid generation: {0}. Simulated up to {1}", generation, lastSimulatedGeneration));
				yield break;
			}

			if (this.playbackScene != null && this.playbackScene.IsValid()) {
				yield return SceneController.UnloadAsync(this.playbackScene);
			}

			this.CurrentGeneration = generation;

			var context = new SceneController.SimulationSceneLoadContext();
			var sceneContext = new PlaybackSceneContext(this.evolution, this);

			yield return SceneController.LoadSimulationScene(
				creatureDesign: this.evolution.SimulationData.CreatureDesign,
				creatureSpawnCount: 1,
				sceneDescription: this.evolution.SimulationData.SceneDescription,
				sceneType: SceneController.SimulationSceneType.BestCreatures,
				legacyOptions: evolution.GetLegacySimulationOptions(),
				context: context, 
				sceneContext: sceneContext
			);
			this.physicsScene = context.PhysicsScene;
			this.playbackScene = context.Scene;

			evolution.LoadBestCreatureOfGenerationIfNecessary(generation);
			var chromosome = evolution.SimulationData.BestCreatures[generation - 1]?.Chromosome;
			this.CurrentBest = context.Creatures[0];

			this.CurrentBest.recordingPlayer = null;
			evolution.ApplyBrain(this.CurrentBest, chromosome);

			trackedCamera.Target = this.CurrentBest;
			
			this.CurrentBest.SetOnBestCreatureLayer();

			this.CurrentBest.Alive = false;
			this.CurrentBest.gameObject.SetActive(false);

			this.CurrentBest.Alive = true;
			this.CurrentBest.gameObject.SetActive(true);

			AutoPlay();

			if (PlaybackDidBegin != null) PlaybackDidBegin();
		}

		private void AutoPlay() {

			StopAutoPlay();
			if (this.CurrentBest == null) return;
			if (!AutoplayEnabled) return;

			float autoplayDuration = evolution.Settings.SimulationTime;
			autoplayRoutine = StartCoroutine(ShowNextGenerationAfterTime(autoplayDuration));
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

		public void RefreshMuscleContractionVisibility() {
			if (CurrentBest != null) 
				CurrentBest.RefreshMuscleContractionVisibility(Settings.ShowMuscleContraction);
		}

		public void RemoveUnneededBestCreatures(SimulationData simulationData) {
			for (int i = 0; i < simulationData.BestCreatures.Count - 2; i++) {
				if (i != CurrentGeneration - 1) {
					simulationData.BestCreatures[i] = null;
				}
			}
		}
	}
}