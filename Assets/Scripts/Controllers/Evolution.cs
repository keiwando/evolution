﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

	public class Evolution : MonoBehaviour {

		public struct Solution {
			public IChromosomeEncodable<float[]> Encodable;
			public CreatureStats Stats;
			public int NumberOfNetworkOutputs;
			public CreatureRecorder Recorder;
		}

		#region Events

		public event Action NewGenerationDidBegin;
		public event Action NewBatchDidBegin;
		public event Action SimulationWasSaved;
		public event Action CreatureWasSavedToGallery;
		public event Action InitializationDidEnd;

		#endregion
		#region Settings

		public SimulationSettings Settings { 
			get { return SimulationData.Settings; } 
			set { SimulationData.Settings = value; } 
		}

		public NeuralNetworkSettings NetworkSettings {
			get { return SimulationData.NetworkSettings; }
			set { SimulationData.NetworkSettings = value; }
		}

		// The user can change simulation settings during a running simulation via the pause menu,
		// but we must ensure to only apply those changes starting with the next generation. The
		// settings of our SimulationData must be kept in sync with the rest of its state 
		// (e.g. current chromosomes length = population size etc.)
		public SimulationSettings SettingsForNextGeneration;

		public bool IsSimulatingInBatches { get { return Settings.SimulateInBatches; } }

		/// <summary>
		/// The number of creatures that are currently being simulated at once. Cached at the beginning of
		/// each generation.
		/// </summary>
		/// <value></value>
		public int CurrentBatchSize { get { return Settings.BatchSize; } }

		/// <summary>
		/// The simulation config with which the simulation was started.
		/// </summary>
		private SimulationConfig config;

		#endregion
		#region Global Simulation Data

		public SimulationData SimulationData { get; private set; }
		
		/// <summary>
		/// The creature body template, from which the entire generation is instantiated. 
		/// Has no brain by default.
		/// </summary>
		private Creature creature;

		public int CurrentGenerationNumber { get { return currentGenerationNumber; } }
		/// <summary>
		/// The number of the currently simulating generation. Starts at 1.
		/// </summary>
		private int currentGenerationNumber = 1;

		#endregion
		#region Per Generation Data

		/// <summary>
		/// The currently simulating batch of creatures (a subset of currentGeneration).
		/// </summary>
		public Creature[] CurrentCreatureBatch {
			get { return currentCreatureBatch; }
		}
		private Creature[] currentCreatureBatch = new Creature[0];

		/// <summary>
		/// The number of the currently simulating batch. Between 1 and Ceil(populationSize / batchSizeCached)
		/// </summary>
		public int CurrentBatchNumber { 
			get { return currentBatchNumber; } 
		}
		private int currentBatchNumber;

		private PhysicsScene batchPhysicsScene;

		#endregion

		public AutoSaver AutoSaver { get; private set; }
		public int LastSavedGeneration { get; private set; }
    public CreatureRecording BestCreatureRecording { get; set; }
		private string lastSaveFilePath { get; set; }
		private string loadedFromSimulationFilePath => config.Options.loadedFromSimulationFilePath;
		public string CurrentSaveFilePath {
			get {
				string path = lastSaveFilePath;
				if (string.IsNullOrEmpty(path)) {
					path = loadedFromSimulationFilePath;
				}
				return path;
			}
		}

		public BestCreaturesController BestCreaturesController { get; set; }
		private Coroutine simulationRoutine;
    private Brain.UniqueMusclesContext uniqueMusclesContext;

		private List<CreatureRecorder> recorders = new List<CreatureRecorder>();
		
		void Start() {
			
			Physics.simulationMode = SimulationMode.Script;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			this.AutoSaver = new AutoSaver();

			// Find the configuration
			var configContainer = FindAnyObjectByType<SimulationConfigContainer>();
			if (configContainer == null) {
				Debug.LogError("No simulation config was found");
				return;
			}

			StartSimulation(configContainer.Config);
		}
		
		/// <summary>
		/// Performs cleanup necessary to completely stop the simulation.
		/// </summary>
		public void Finish() {}

		public void Pause() {
			Time.timeScale = 0f;
		}

		public void Resume() {
			Time.timeScale = 1f;
		}

		/// <summary>
		/// Continues the simulation from the state given by data.
		/// </summary>
		private void StartSimulation(SimulationConfig config) {

			this.config = config;
			var data = config.SimulationData;
			this.SimulationData = data;
			this.SettingsForNextGeneration = data.Settings;
			this.LastSavedGeneration = data.BestCreatures.Count;
			
			this.currentGenerationNumber = data.BestCreatures.Count + 1;
      this.uniqueMusclesContext = Brain.CalculateUniqueMusclesContext(data.CreatureDesign.Muscles);

			if (this.InitializationDidEnd != null) InitializationDidEnd();

			this.simulationRoutine = StartCoroutine(Simulate());
		}

		private IEnumerator Simulate() {

			while (true) {
				yield return SimulateGeneration();
			}
		}

		private IEnumerator SimulateGeneration() {

			this.Settings = this.SettingsForNextGeneration;
			var solutions = new Solution[Settings.PopulationSize];
			var solutionIndex = 0;
			// Prepare batch simulation
			int actualBatchSize = Settings.SimulateInBatches ? Settings.BatchSize : Settings.PopulationSize;
			int numberOfBatches = (int)Math.Ceiling((double)this.Settings.PopulationSize / actualBatchSize);
			int firstChromosomeIndex = 0;

			if (NewGenerationDidBegin != null) NewGenerationDidBegin();

			// We have to immediately create missing chromosomes here so that the Chromosomes array
			// is always complete for serialization purposes
      if (SimulationData.CurrentChromosomes.Length < Settings.PopulationSize) {
        float[][] allChromosomes = new float[Settings.PopulationSize][];
        for (int i = 0; i < SimulationData.CurrentChromosomes.Length; i++) {
          allChromosomes[i] = SimulationData.CurrentChromosomes[i];
        }
        BrainType brainType = GetBrainTypeForSimulation(Settings.Objective, SimulationData.LastV2SimulatedGeneration);
        int chromosomeLength = CalculateChromosomeLengthForBrainType(brainType, SimulationData.NetworkSettings, this.uniqueMusclesContext);
        for (int i = SimulationData.CurrentChromosomes.Length; i < allChromosomes.Length; i++) {
          float[] randomWeights = new float[chromosomeLength];
					FeedForwardNetwork.PopulateRandomWeights(randomWeights);
					allChromosomes[i] = randomWeights;
        }
				SimulationData.CurrentChromosomes = allChromosomes;
      }

			for (int i = 0; i < numberOfBatches; i++) {
				
				this.currentBatchNumber = i + 1;
				int remainingCreatures = Settings.PopulationSize - (i * actualBatchSize);
				int currentBatchSize = Math.Min(actualBatchSize, remainingCreatures);

				var context = new SceneController.SimulationSceneLoadContext();
				var sceneContext = new SimulationSceneContext(this);

				yield return SceneController.LoadSimulationScene(
					creatureDesign: this.SimulationData.CreatureDesign,
					creatureSpawnCount: currentBatchSize,
					sceneDescription: this.SimulationData.SceneDescription,
					sceneType: SceneController.SimulationSceneType.Simulation,
					legacyOptions: GetLegacySimulationOptions(),
					context: context, 
					sceneContext: sceneContext
				);
				
				this.batchPhysicsScene = context.PhysicsScene;
				
				var batch = context.Creatures;
				this.currentCreatureBatch = batch;

				var chromosomeCount = Math.Min(this.SimulationData.CurrentChromosomes.Length, batch.Length);
				var chromosomes = new float[chromosomeCount][];
				for (int c = 0; c < chromosomeCount; c++) {
					chromosomes[c] = this.SimulationData.CurrentChromosomes[c + firstChromosomeIndex];
				}
				firstChromosomeIndex += batch.Length;
				ApplyBrains(batch, chromosomes);
				
				// Create missing or remove excessive recorders
				int missingRecorders = actualBatchSize - recorders.Count;
				if (missingRecorders > 0) {
					for (int missingRecorderIndex = 0; missingRecorderIndex < missingRecorders; missingRecorderIndex++) {
						recorders.Add(null);
					}
				}
				int excessiveRecorders = recorders.Count - actualBatchSize;
				if (excessiveRecorders > 0) {
					recorders.RemoveRange(recorders.Count - excessiveRecorders, excessiveRecorders);
				}
				if (batch.Length > 0) {
					int numberOfJoints = batch[0].joints.Count;
					int numberOfMuscles = batch[0].muscles.Count;
					int recordingDurationInSeconds = (int)Math.Min(10f, Settings.SimulationTime);
					for (int recorderIndex = 0; recorderIndex < recorders.Count; recorderIndex++) {
						bool needsNewRecorder = false; 
						if (recorders[recorderIndex] == null) {
							needsNewRecorder = true;
						} else {
							needsNewRecorder |= recorders[recorderIndex].recordingDurationInSeconds != recordingDurationInSeconds;
							needsNewRecorder |= recorders[recorderIndex].numberOfJoints != numberOfJoints;
							needsNewRecorder |= recorders[recorderIndex].numberOfMuscles != numberOfMuscles;
						}
						if (needsNewRecorder) {
							recorders[recorderIndex] = new CreatureRecorder(
								recordingDurationInSeconds: recordingDurationInSeconds,
								numberOfJoints: numberOfJoints,
								numberOfMuscles: numberOfMuscles
							);
						}
					}
				}

				for (int creatureIndex = 0; creatureIndex < batch.Length; creatureIndex++) {
					// Assign the recorder to the creature
					if (creatureIndex < recorders.Count) {
						batch[creatureIndex].recorder = recorders[creatureIndex];
						batch[creatureIndex].recorder.reset();
					} else {
						batch[creatureIndex].recorder = null;
					}
				}

				yield return SimulateBatch();

				// Evaluate creatures and destroy the scene after extracting all 
				// required performance statistics
				for (int j = 0; j < batch.Length; j++) {
					var creature = batch[j];
					solutions[solutionIndex++] = new Solution() { 
						Encodable = creature.brain.Network,
						Stats = creature.GetStatistics(this.Settings.SimulationTime),
						NumberOfNetworkOutputs = creature.brain.Network.NumberOfOutputs,
						Recorder = creature.recorder
					};
				}
				
				yield return SceneManager.UnloadSceneAsync(context.Scene);
			}

			EvaluateSolutions(solutions);
		}

		private IEnumerator SimulateBatch() {

			foreach (Creature creature in currentCreatureBatch) {
				creature.Alive = false;
				creature.gameObject.SetActive(false);
			}

			foreach (Creature creature in currentCreatureBatch) {
				creature.Alive = true;
				creature.gameObject.SetActive(true);
			}

			if (NewBatchDidBegin != null) NewBatchDidBegin();

			yield return new WaitForSeconds(Settings.SimulationTime);
		}

		private void EvaluateSolutions(Solution[] solutions) {

			SortGenerationByFitness(solutions);

			if (config.Options.onEvaluatedSolutions != null) {
				config.Options.onEvaluatedSolutions(solutions);
			}

			// Save the best solution
			Solution best = solutions[0];
			SimulationData.BestCreatures.Add(new ChromosomeData(best.Encodable.ToChromosome(), best.Stats));

			CreatureRecordingMovementData recordedMovementData = best.Recorder.toRecordingMovementData();
			BestCreatureRecording = new CreatureRecording(
				creatureDesign: SimulationData.CreatureDesign,
				sceneDescription: SimulationData.SceneDescription,
				movementData: recordedMovementData,
				task: SimulationData.Settings.Objective,
				generation: this.currentGenerationNumber,
				stats: best.Stats,
				networkInputCount: GetNumberOfCurrentBrainInputs(),
				networkOutputCount: best.NumberOfNetworkOutputs,
				networkSettings: SimulationData.NetworkSettings
			);

			// Autosave if necessary
			bool saved = AutoSaver.Update(this.currentGenerationNumber, this);
			if (saved && SimulationWasSaved != null) {
				SimulationWasSaved();
			}

			this.SimulationData.CurrentChromosomes = CreateNewChromosomes(Settings.PopulationSize, solutions, Settings.KeepBestCreatures);
			this.currentGenerationNumber += 1;
		}

		void FixedUpdate() {
			if (batchPhysicsScene != null && batchPhysicsScene.IsValid()) {
				batchPhysicsScene.Simulate(Time.fixedDeltaTime);
			}
		}

		private static void SortGenerationByFitness(Solution[] generation) {
			Array.Sort(generation, delegate(Solution lhs, Solution rhs) { return rhs.Stats.unclampedFitness.CompareTo(lhs.Stats.unclampedFitness); } );
		}

		private float[][] CreateNewChromosomes(int nextGenerationSize, Solution[] solutions, bool keepBest) {

			float[][] result = new float[nextGenerationSize][];

			var lazyChromosomes = new List<LazyChromosomeData<float[]>>();
			foreach (var solution in solutions) {
				lazyChromosomes.Add(new LazyChromosomeData<float[]>(solution.Encodable, solution.Stats));
			}
			var selection = new Selection<LazyChromosomeData<float[]>>(Settings.SelectionAlgorithm, lazyChromosomes);

			int start = 0;
			if (keepBest) {

				// keep the two best creatures
				var best = selection.SelectBest(2);
				result[0] = best[0].Chromosome;
				result[1] = best[1].Chromosome;
				start = 2;
			}

			float[][] recombinationResult = new float[2][];

			for (int i = start; i < result.Length; i += 2) {

				var parent1 = selection.Select();
				var parent2 = selection.Select();

				float[] chrom1 = parent1.Chromosome;
				float[] chrom2 = parent2.Chromosome;
				
				Recombination<float>.Recombine(chrom1, chrom2, recombinationResult, Settings.RecombinationAlgorithm);

				result[i] = Mutated(recombinationResult[0]);
				if (i + 1 < result.Length) {
					result[i + 1] = Mutated(recombinationResult[1]);
				}
			}

			return result;

		}

		private float[] Mutated(float[] chromosome) {

			bool shouldMutate = UnityEngine.Random.Range(0, 100.0f) < Settings.MutationRate * 100f;
			if (!shouldMutate) return chromosome;

			return Mutation.Mutate(chromosome, Settings.MutationAlgorithm);
		}

		private void ApplyBrains(Creature[] creatures, float[][] chromosomes) {

			for (int i = 0; i < creatures.Length; i++) {

				if (i < chromosomes.Length) {
					ApplyBrain(creatures[i], chromosomes[i]);
				} else {
					// Random brain
					ApplyBrain(creatures[i]);
				}
			}
		}

		public void ApplyBrain(Creature creature, float[] chromosome = null) {
			
			Brain brain = creature.GetComponent<Brain>();
			
			if (brain == null) {
				BrainType brainType = GetBrainTypeForSimulation(Settings.Objective, lastV2SimulatedGeneration: SimulationData.LastV2SimulatedGeneration);
				AddObjectiveTracker(Settings.Objective, creature);
				brain = AddBrainComponent(brainType, creature);
			}
			brain.Init(NetworkSettings, creature.muscles.ToArray(), this.uniqueMusclesContext, chromosome);
			
			creature.brain = brain;
		}

		private static BrainType GetBrainTypeForSimulation(Objective objective, int lastV2SimulatedGeneration) {
			bool useLegacyBrains = lastV2SimulatedGeneration > 0;
			if (!useLegacyBrains) {
				return BrainType.Universal;
			}

			switch (objective) {
				case Objective.Running: return BrainType.LegacyRunningBrain;
				case Objective.Jumping: return BrainType.LegacyJumpingBrain;
				case Objective.ObstacleJump: return BrainType.LegacyObstacleJumpBrain;
				case Objective.Climbing: return BrainType.LegacyClimbingBrain;
			}
			return BrainType.Universal;
		}

		private static Brain AddBrainComponent(BrainType brainType, Creature creature) {

			var gameObject = creature.gameObject;

			switch (brainType) {
				case BrainType.Universal:
					return gameObject.AddComponent<UniversalBrain>();
				case BrainType.LegacyRunningBrain:
					return gameObject.AddComponent<RunningBrain>();
				case BrainType.LegacyJumpingBrain:
					return gameObject.AddComponent<JumpingBrain>();
				case BrainType.LegacyObstacleJumpBrain:
					return gameObject.AddComponent<ObstacleJumpingBrain>();
				case BrainType.LegacyClimbingBrain:
					return gameObject.AddComponent<ClimbingBrain>();
				default:
					throw new System.ArgumentException(string.Format("There is no brain type for the given brainType: {0}", brainType));
			}
		}

    private static int CalculateChromosomeLengthForBrainType(BrainType brainType, NeuralNetworkSettings networkSettings, Brain.UniqueMusclesContext uniqueMusclesContext) {
      
      int numberOfInputs = Brain.GetNetworkInputCountForBrainType(brainType);
      int numberOfOutputs = Brain.GetNetworkOutputCountForBrainType(brainType, uniqueMusclesContext);
      int totalWeightCount = 0;
      int totalLayerCount = networkSettings.NumberOfIntermediateLayers + 2;
      for (int i = 0; i < totalLayerCount - 1; i++) {
        int layerInputNodesCount = 0;
        int layerOutputNodesCount = 0;
        if (i == 0) {
          layerInputNodesCount = numberOfInputs;
        } else {
          layerInputNodesCount = networkSettings.NodesPerIntermediateLayer[i - 1];
        }
        if (i == totalLayerCount - 2) {
          layerOutputNodesCount = numberOfOutputs;
        } else {
          layerOutputNodesCount = networkSettings.NodesPerIntermediateLayer[i];
        }
        totalWeightCount += (layerInputNodesCount * layerOutputNodesCount);
      }
      return totalWeightCount;
    }

		private static ObjectiveTracker AddObjectiveTracker(Objective objective, Creature creature) {

			switch (objective) {
			case Objective.Running:
				return creature.gameObject.AddComponent<RunningObjectiveTracker>();
			case Objective.Jumping:
				return creature.gameObject.AddComponent<JumpingObjectiveTracker>();
			case Objective.ObstacleJump:
				return creature.gameObject.AddComponent<ObstacleJumpObjectiveTracker>();
			case Objective.Climbing:
				return creature.gameObject.AddComponent<ClimbingObjectiveTracker>();
			case Objective.Flying:
				return creature.gameObject.AddComponent<FlyingObjectiveTracker>();
			}

			throw new System.ArgumentException(string.Format("There is no objective tracker for the given objective {0}", objective));
		}

		public int GetNumberOfCurrentBrainInputs() {
			
			var usesLegacyBrain = SimulationData.LastV2SimulatedGeneration > 0;
			if (!usesLegacyBrain) {
				return UniversalBrain.NUMBER_OF_INPUTS;
			}

			switch (Settings.Objective) {
				case Objective.Running: return RunningBrain.NUMBER_OF_INPUTS;
				case Objective.Jumping: return JumpingBrain.NUMBER_OF_INPUTS;
				case Objective.ObstacleJump: return ObstacleJumpingBrain.NUMBER_OF_INPUTS;
				case Objective.Climbing: return ClimbingBrain.NUMBER_OF_INPUTS;
			}

			return 0;
		}

		public LegacySimulationOptions GetLegacySimulationOptions() {
			if (SimulationData.LastV2SimulatedGeneration > 0) {
				return new LegacySimulationOptions() {
					LegacyRotationCalculation = true,
					LegacyClimbingDropCalculation = SimulationData.Settings.Objective == Objective.Climbing
				};
			}
			return new LegacySimulationOptions();
		}

		public void SaveToGallery() {
			CreatureRecording recording = this.BestCreatureRecording;
			if (recording != null) {
				CreatureRecordingSerializer.SaveCreatureRecordingFile(recording);
				if (CreatureWasSavedToGallery != null) CreatureWasSavedToGallery();
			}
		}

		public void SaveSimulation() {

			string filePathToUpdate = null;
			if (string.IsNullOrEmpty(lastSaveFilePath)) {
				// The first save of each separate simulation run (even when loaded from an old simulation file)
				// should result in a distinct save file.
				string availableFilename = SimulationSerializer.GetAvailableSimulationName(SimulationData);
				if (!string.IsNullOrEmpty(loadedFromSimulationFilePath)) {
					string dstFilePath = SimulationSerializer.PathToSimulationSave(availableFilename);
					// Copy the source file so we can append to it as a new file
					File.Copy(loadedFromSimulationFilePath, dstFilePath);
					filePathToUpdate = dstFilePath;
					this.lastSaveFilePath = dstFilePath;
				} else {
					string savedFilenameWithoutExtension = SimulationSerializer.SaveSimulationFile(availableFilename, SimulationData);
					this.lastSaveFilePath = SimulationSerializer.PathToSimulationSave(savedFilenameWithoutExtension);
				}
			} else {
				filePathToUpdate = lastSaveFilePath;
			}

			if (!string.IsNullOrEmpty(filePathToUpdate)) {
				this.lastSaveFilePath = SimulationSerializer.SaveSimulationDataUpdatesIntoExistingFile(SimulationData, filePathToUpdate);
			}
			
			BestCreaturesController.RemoveUnneededBestCreatures(SimulationData);
			this.LastSavedGeneration = currentGenerationNumber;
			if (SimulationWasSaved != null) SimulationWasSaved();
		}

		public void LoadBestCreatureOfGenerationIfNecessary(int generation) {
			string currentSaveFilePath = CurrentSaveFilePath;
			if (SimulationData.BestCreatures[generation - 1] == null &&
				 !string.IsNullOrEmpty(currentSaveFilePath)
			) {
				SimulationSerializer.LoadBestCreatureData(currentSaveFilePath, SimulationData, generation - 1);
			}
		}
	}
}
