using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

	public class Evolution : MonoBehaviour {

		private struct Solution {
			public IChromosomeEncodable<float[]> Encodable;
			public CreatureStats Stats;
		}

		#region Events

		public event Action NewGenerationDidBegin;
		public event Action NewBatchDidBegin;
		public event Action SimulationWasSaved;
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

		// Cached values
		private SimulationSettings cachedSettings;

		public bool IsSimulatingInBatches { get { return cachedSettings.SimulateInBatches; } }

		/// <summary>
		/// The number of creatures that are currently being simulated at once. Cached at the beginning of
		/// each generation.
		/// </summary>
		/// <value></value>
		public int CurrentBatchSize { get { return cachedSettings.BatchSize; } }

		#endregion
		#region Global Simulation Data


		public SimulationData SimulationData { get; private set; }
		
		/// <summary>
		/// The creature body template, from which the entire generation is instantiated. 
		/// Has no brain by default.
		/// </summary>
		private Creature creature;

		/// <summary>
		/// The obstacle gameobject of the "Obstacle Jump" task.
		/// </summary>
		public GameObject Obstacle { get; set; }

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

		private Coroutine simulationRoutine;
		
		void Start () {
			
			Physics.autoSimulation = false;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			this.AutoSaver = new AutoSaver();

			// Find the configuration
			var configContainer = FindObjectOfType<SimulationConfigContainer>();
			if (configContainer == null) {
				Debug.LogError("No simulation config was found");
				return;
			}

			var data = configContainer.SimulationData;
			StartSimulation(data);
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
		private void StartSimulation(SimulationData data) {

			this.SimulationData = data;
			this.cachedSettings = Settings;
			this.LastSavedGeneration = data.BestCreatures.Count;

			// Debug.Log("Task " + EvolutionTaskUtil.StringRepresentation(data.Settings.Task));

			// Instantiate the creature template
			var creatureBuilder = new CreatureBuilder(data.CreatureDesign);
			this.creature = creatureBuilder.Build();
			this.creature.RemoveMuscleColliders();
			this.creature.Alive = false;
			
			this.currentGenerationNumber = data.BestCreatures.Count + 1;

			this.creature.gameObject.SetActive(false);
			if (this.InitializationDidEnd != null) InitializationDidEnd();

			this.simulationRoutine = StartCoroutine(Simulate());
		}

		private IEnumerator Simulate() {

			while (true) {
				yield return SimulateGeneration();
			}
		}

		private IEnumerator SimulateGeneration() {

			var solutions = new Solution[Settings.PopulationSize];
			var solutionIndex = 0;
			// Prepare batch simulation
			int actualBatchSize = Settings.SimulateInBatches ? Settings.BatchSize : Settings.PopulationSize;
			int numberOfBatches = (int)Math.Ceiling((double)this.Settings.PopulationSize / actualBatchSize);
			int firstChromosomeIndex = 0;
			// Cache values that can be changed during the simulation
			this.cachedSettings = this.Settings;

			if (NewGenerationDidBegin != null) NewGenerationDidBegin();

			for (int i = 0; i < numberOfBatches; i++) {
				
				this.currentBatchNumber = i + 1;
				int remainingCreatures = Settings.PopulationSize - (i * actualBatchSize);
				int currentBatchSize = Math.Min(actualBatchSize, remainingCreatures);

				var sceneLoadConfig = new SceneController.SimulationSceneLoadConfig(
					this.SimulationData.CreatureDesign,
					currentBatchSize,
					this.SimulationData.SceneDescription,
					SceneController.SimulationSceneType.Simulation,
					GetLegacySimulationOptions()
				);

				var context = new SceneController.SimulationSceneLoadContext();
				var sceneContext = new SimulationSceneContext(this.SimulationData);

				yield return SceneController.LoadSimulationScene(sceneLoadConfig, context, sceneContext);
				
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

				yield return SimulateBatch();

				// Evaluate creatures and destroy the scene after extracting all 
				// required performance statistics
				for (int j = 0; j < batch.Length; j++) {
					var creature = batch[j];
					solutions[solutionIndex++] = new Solution() { 
						Encodable = creature.brain.Network,
						Stats = creature.GetStatistics()
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

			yield return new WaitForSeconds(cachedSettings.SimulationTime);
		}

		private void EvaluateSolutions(Solution[] solutions) {

			SortGenerationByFitness(solutions);

			// Save the best solution
			var best = solutions[0];
			SimulationData.BestCreatures.Add(new ChromosomeData(best.Encodable.ToChromosome(), best.Stats));

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
			Array.Sort(generation, delegate(Solution lhs, Solution rhs) { return rhs.Stats.fitness.CompareTo(lhs.Stats.fitness); } );
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
					// ApplyBrain(creatures[i], chromosomes[0]);
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
				brain = AddBrainComponent(Settings.Task, creature.gameObject);
			}
			brain.Init(NetworkSettings, creature.muscles.ToArray(), chromosome);
			
			brain.SimulationTime = cachedSettings.SimulationTime;
			brain.Creature = creature;
			creature.brain = brain;
		}

		private static Brain AddBrainComponent(EvolutionTask task, GameObject gameObject) {
			switch (task) {
			case EvolutionTask.Running:
				return gameObject.AddComponent<RunningBrain>();
			case EvolutionTask.Jumping:
				return gameObject.AddComponent<JumpingBrain>();
			case EvolutionTask.ObstacleJump:
				return gameObject.AddComponent<ObstacleJumpingBrain>();
			case EvolutionTask.Climbing:
				return gameObject.AddComponent<ClimbingBrain>();
			default:
				throw new System.ArgumentException(string.Format("There is no brain type for the given task: {0}", task));
			}
		}

		public LegacySimulationOptions GetLegacySimulationOptions() {
			if (SimulationData.LastV2SimulatedGeneration > 0) {
				return new LegacySimulationOptions() {
					LegacyRotationCalculation = true,
					LegacyClimbingDropCalculation = SimulationData.Settings.Task == EvolutionTask.Climbing
				};
			}
			return new LegacySimulationOptions();
		}

		public string SaveSimulation() {
			var savefileName = SimulationSerializer.SaveSimulation(SimulationData);
			this.LastSavedGeneration = currentGenerationNumber;
			if (SimulationWasSaved != null) SimulationWasSaved();
			return savefileName;
		}
	}
}
