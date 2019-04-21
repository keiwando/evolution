using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

public class Evolution : MonoBehaviour {

	#region Events

	public event Action NewGenerationDidBegin;
	public event Action NewBatchDidBegin;
	public event Action SimulationWasSaved;

	#endregion
	#region Settings

	public SimulationSettings Settings { get; set; }

	public NeuralNetworkSettings NetworkSettings { get; set; }

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

	/// <summary>
	/// The height from the ground from which the creatures should be dropped on spawn.
	/// </summary>
	private Vector3 dropHeight;

	/// <summary>
	/// An offset added to the drop height in order to prevent creatures from being
	/// spawned into the ground.
	/// </summary>
	private float safeHeightOffset;

	public int CurrentGenerationNumber { get { return currentGenerationNumber; } }
	/// <summary>
	/// The number of the currently simulating generation. Starts at 1.
	/// </summary>
	private int currentGenerationNumber = 1;

	#endregion
	#region Per Generation Data

	/// <summary>
	/// The chromosome strings of the current generation.
	/// </summary>
	private string[] currentChromosomes;

	/// <summary>
	/// The current generation of Creatures.
	/// </summary>
	private Creature[] currentGeneration;

	/// <summary>
	/// The currently simulating batch of creatures (a subset of currentGeneration).
	/// </summary>
	public Creature[] CurrentCreatureBatch {
		get { return currentCreatureBatch; }
	}
	private Creature[] currentCreatureBatch;

	/// <summary>
	/// The number of the currently simulating batch. Between 1 and Ceil(populationSize / batchSizeCached)
	/// </summary>
	public int CurrentBatchNumber { 
		get { return currentBatchNumber; } 
	}
	private int currentBatchNumber;

	#endregion
	#region Utils & Controllers

	public AutoSaver AutoSaver { get; private set; }

	#endregion
	
	void Start () {

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		this.AutoSaver = new AutoSaver();

		// Find the configuration
		var configContainer = FindObjectOfType<SimulationConfigContainer>();
		if (configContainer == null) {
			Debug.Log("No simulation config was found");
			return;
		}

		var data = configContainer.SimulationData;
		StartSimulation(data);
	}
	
	/// <summary>
	/// Performs cleanup necessary to completely stop the simulation.
	/// </summary>
	public void Finish() {
		KillGeneration();
	}

	/// <summary>
	/// Continues the simulation from the state given by data.
	/// </summary>
	private void StartSimulation(SimulationData data) {

		this.NetworkSettings = data.NetworkSettings;
		this.Settings = data.Settings;
		this.SimulationData = data;

		// Instantiate the creature template
		var creatureBuilder = new CreatureBuilder(data.CreatureDesign);
		this.creature = creatureBuilder.Build();
		this.creature.RemoveMuscleColliders();
		this.creature.Alive = false;

		// Update safe drop offset
		// Ensures that the creature isn't spawned into the ground
		var lowestY = creature.GetLowestPoint().y;
		this.safeHeightOffset = lowestY < 0 ? -lowestY + 1f : 0f;
		// Calculate the drop height
		float distanceFromGround = creature.DistanceFromGround();
		float padding = 0.5f;
		this.dropHeight = creature.transform.position;
		this.dropHeight.y -= distanceFromGround - padding;
		
		this.currentGenerationNumber = data.BestCreatures.Count + 1;
		this.currentChromosomes = data.CurrentChromosomes;
		this.currentGeneration = CreateGeneration();
		// TODO: Apply brains

		// Batch simulation
		this.currentBatchNumber = 1;
		// Cache values that can be changed during the simulation
		this.cachedSettings = this.Settings;
		
		this.creature.gameObject.SetActive(false);

		SimulateGeneration();
	}

	private void SimulateGeneration() {


	}

	/// <summary>
	/// Continues the evolution from the save state. 
	/// 
	/// This function call has to replace calls to StartEvolution (and therefore also SetupEvolution) when a simulation would be started
	/// from the beginning.
	/// 
	/// </summary>
	/// <param name="generationNum">The generation number that the simulation should continue at from.</param>
	/// <param name="timePerGen">The time for each generation simulation.</param>
	/// <param name="bestChromosomes">The list of best chromosomes of the already simluated generations.</param>
	/// <param name="currentChromosomes">A list of chromosomes of creatures of the last (current) generation.</param>
	//public void ContinueEvolution(int generationNum, int timePerGen, List<ChromosomeInfo> bestChromosomes, List<string> currentChromosomes) {
	public void ContinueEvolution(int generationNum, SimulationSettings SimulationSettings, NeuralNetworkSettings networkSettings, List<ChromosomeStats> bestChromosomes, List<string> currentChromosomes) {

		this.settings = SimulationSettings;
		this.brainSettings = networkSettings;

		viewController = GameObject.Find("ViewController").GetComponent<ViewController>();
		Assert.IsNotNull(viewController);

		this.currentGenerationNumber = generationNum;
		this.currentChromosomes = currentChromosomes.ToArray();

		creature.RemoveMuscleColliders();
		creature.Alive = false;

		viewController.UpdateGeneration(generationNum);

		autoSaver = new AutoSaver();

		// Setup Evolution call
		CalculateDropHeight();

		BCController = GameObject.Find("Best Creature Controller").GetComponent<BestCreaturesController>();
		

		BCController.SetBestChromosomes(bestChromosomes);
		BCController.ShowBCThumbScreen();
		BCController.RunBestCreatures(generationNum - 1);

		currentGeneration = CreateGeneration();

		// Batch simulation
		currentBatchNumber = 1;
		simulateInBatchesCached = settings.simulateInBatches;
		batchSizeCached = settings.simulateInBatches ? settings.batchSize : settings.populationSize;
		var currentBatchSize = Mathf.Min(batchSizeCached, settings.populationSize - ((currentBatchNumber - 1) * batchSizeCached));
		currentCreatureBatch = new Creature[currentBatchSize]; 

		Array.Copy(currentGeneration, 0, currentCreatureBatch, 0, currentBatchSize);

		creature.gameObject.SetActive(false);

		SimulateGeneration();

		var cameraFollow = Camera.main.GetComponent<CameraFollowScript>();
		cameraFollow.toFollow = currentGeneration[0];
		cameraFollow.currentlyWatchingIndex = 0;

		RefreshVisibleCreatures();
	}

	/** Starts the Evolution for the current */
	public void StartEvolution() {

		viewController = GameObject.Find("ViewController").GetComponent<ViewController>();
		Assert.IsNotNull(viewController);

		creature.RemoveMuscleColliders();
		creature.Alive = false;
		running = true;
		SetupEvolution();
	}

	private void SetupEvolution() {

		CalculateDropHeight();

		BCController = GameObject.Find("Best Creature Controller").GetComponent<BestCreaturesController>();
		BCController.dropHeight = dropHeight;
		BCController.Creature = creature;

		// The first generation will have random brains.
		currentGeneration = CreateCreatures();
		ApplyBrains(currentGeneration, true);


		// Batch simulation
		currentBatchNumber = 1;
		simulateInBatchesCached = settings.simulateInBatches;
		batchSizeCached = settings.simulateInBatches ? settings.batchSize : settings.populationSize;
		var currentBatchSize = Mathf.Min(batchSizeCached, settings.populationSize - ((currentBatchNumber - 1) * batchSizeCached));
		currentCreatureBatch = new Creature[currentBatchSize];
		Array.Copy(currentGeneration, 0, currentCreatureBatch, 0, currentBatchSize);

		SimulateGeneration();

		creature.gameObject.SetActive(false);

		var cameraFollow = Camera.main.GetComponent<CameraFollowScript>();
		cameraFollow.toFollow = currentGeneration[0];
		cameraFollow.currentlyWatchingIndex = 0;

		RefreshVisibleCreatures();

		viewController.UpdateGeneration(currentGenerationNumber);

		autoSaver = new AutoSaver();
	}
		
	/// <summary>
	/// Simulates the task for every creature in the current batch
	/// </summary>
	private void SimulateGeneration() {

		foreach (Creature creature in currentGeneration) {
			creature.Alive = false;
			creature.gameObject.SetActive(false);
		}

		foreach (Creature creature in currentCreatureBatch) {
			creature.Alive = true;
			creature.gameObject.SetActive(true);
		}

		StartCoroutine(StopSimulationAfterTime(settings.simulationTime));
	}

	IEnumerator StopSimulationAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		// Check if all of the batches of the current generation were simulated.
		var creaturesLeft = settings.populationSize - (currentBatchNumber * batchSizeCached);
		if (simulateInBatchesCached && creaturesLeft > 0 ) {
			// Simulate the next batch first

			var currentBatchSize = Mathf.Min(batchSizeCached, creaturesLeft);
			currentCreatureBatch = new Creature[currentBatchSize];
			Array.Copy(currentGeneration, currentBatchNumber * batchSizeCached, currentCreatureBatch, 0, currentBatchSize);
			currentBatchNumber += 1;

			viewController.UpdateGeneration(currentGenerationNumber);

			var cameraFollow = Camera.main.GetComponent<CameraFollowScript>();
			cameraFollow.toFollow = currentCreatureBatch[0];
			cameraFollow.currentlyWatchingIndex = 0;

			RefreshVisibleCreatures();

			SimulateGeneration();

		} else {
			EndSimulation();	
		}
	}

	/// <summary>
	/// Ends the simulation of the current creature batch.
	/// </summary>
	private void EndSimulation() {

		if(!running) return;

		foreach( Creature creature in currentGeneration ) {
			creature.Alive = false;
		}

		EvaluateCreatures(currentGeneration);
		SortGenerationByFitness();

		// save the best creature
		var best = currentGeneration[0];

		BCController.AddBestCreature(currentGenerationNumber, best.brain.ToChromosomeString(), best.GetStatistics());

		var saved = autoSaver.Update(currentGenerationNumber, this);

		if (saved) {
			viewController.ShowSavedLabel();
		}

		currentChromosomes = CreateNewChromosomesFromGeneration();
		currentGenerationNumber++;

		ResetCreatures();


		var cameraFollow = Camera.main.GetComponent<CameraFollowScript>();
		cameraFollow.toFollow = currentGeneration[0];
		cameraFollow.currentlyWatchingIndex = 0;

		// Batch simulation
		currentBatchNumber = 1;
		simulateInBatchesCached = settings.simulateInBatches;
		batchSizeCached = settings.simulateInBatches ? settings.batchSize : settings.populationSize;
		var currentBatchSize = Mathf.Min(batchSizeCached, settings.populationSize);
		currentCreatureBatch = new Creature[currentBatchSize]; 
		Array.Copy(currentGeneration, 0, currentCreatureBatch, 0, currentBatchSize);

		// Update the view
		viewController.UpdateGeneration(currentGenerationNumber);

		RefreshVisibleCreatures();

		SimulateGeneration();

	}

	/** Determines a fitness score for every creature in the array. */
	private void EvaluateCreatures(Creature[] creatures) {

		foreach (Creature creature in creatures) {
			creature.brain.EvaluateFitness();
		}
	}

	private void SortGenerationByFitness() {
		Array.Sort(currentGeneration, delegate(Creature a, Creature b) { return b.brain.fitness.CompareTo(a.brain.fitness); } );
	}

	private string[] CreateNewChromosomesFromGeneration() {

		string[] result = new string[Settings.PopulationSize];

		var lazyChromosomes = new List<LazyChromosomeData>();
		foreach (var creature in currentGeneration) {
			lazyChromosomes.Add(new LazyChromosomeData(creature, creature.GetStatistics()));
		}
		var selection = new Selection<LazyChromosomeData>(Selection<LazyChromosomeData>.Mode.FitnessProportional, lazyChromosomes);

		int start = 0;
		if (Settings.KeepBestCreatures) {

			// keep the two best creatures
			var best = selection.SelectBest(2);
			result[0] = best[0].Chromosome;
			result[1] = best[1].Chromosome;
			// result[0] = GetChromosomeWithCaching(0);
			// result[1] = GetChromosomeWithCaching(1);
			start = 2;
		}

		for(int i = start; i < Settings.PopulationSize; i += 2) {

			// randomly pick two creatures and let them "mate"
			// int index1 = PickRandomWeightedIndex();
			// int index2 = PickRandomWeightedIndex();

			string chrom1 = selection.Select().Chromosome;
			string chrom2 = selection.Select().Chromosome;
			// string chrom1 = GetChromosomeWithCaching(index1);
			// string chrom2 = GetChromosomeWithCaching(index2);

			string[] newChromosomes = CombineChromosomes(chrom1, chrom2);

			Assert.AreEqual(chrom1.Length, chrom2.Length);
			Assert.AreEqual(chrom1.Length, newChromosomes[0].Length);

			result[i] = newChromosomes[0];
			if (i + 1 < result.Length) {
				result[i + 1] = newChromosomes[1];
			}
		}

		return result;

	}

	/// <summary>
	/// Optimized
	/// Takes two chromosome strings and returns an array of two new chromosome strings that are a combination of the parent strings.
	/// </summary>
	private string[] CombineChromosomes(string chrom1, string chrom2) {

		// TODO: Move into Recombination class

		int splitIndex = UnityEngine.Random.Range(1, chrom2.Length);
		string[] result = new string[2];

		var chrom1Builder = new StringBuilder(chrom1);
		var chrom2Builder = new StringBuilder(chrom2);

		var chrom2End = chrom2.Substring(splitIndex);
		var chrom1End = chrom1.Substring(splitIndex);

		chrom1Builder.Remove(splitIndex, chrom1Builder.Length - splitIndex);
		chrom2Builder.Remove(splitIndex, chrom2Builder.Length - splitIndex);

		chrom1Builder.Append(chrom2End);
		chrom2Builder.Append(chrom1End);

		Assert.AreEqual(chrom1Builder.Length, chrom1.Length);
		Assert.AreEqual(chrom2Builder.Length, chrom2.Length);

		result[0] = Mutate(chrom1Builder).ToString();
		result[1] = Mutate(chrom2Builder).ToString();

		return result;
	}

	private StringBuilder Mutate(StringBuilder chromosome) {

		bool shouldMutate = UnityEngine.Random.Range(0,100.0f) < Settings.MutationRate;

		if (!shouldMutate) return chromosome;

		return Mutation.Mutate<MutableString, char>(new MutableString(chromosome), Mutation.Mode.Inversion).Builder;
	}

	private void ResetCreatures() {

		for (int i = 0; i < currentGeneration.Length; i++) {

			var currentCreature = currentGeneration[i];
			currentCreature.Reset();
			ApplyBrain(currentCreature, currentChromosomes[i]);
		}
	}

	private Creature[] CreateGeneration(string[] chromosomes) {
		
		this.creature.gameObject.SetActive(true);

		Creature[] creatures = new Creature[chromosomes.Length];
		Creature creature;

		for(int i = 0; i < chromosomes.Length; i++) {
			creature = CreateCreature();
			ApplyBrain(creature, currentChromosomes[i]);
			creatures[i] = creature;

			creature.name = "Creature " + (i+1);
		}

		this.creature.gameObject.SetActive(false);
		return creatures;
	}

	private void KillGeneration() {
		
		foreach(Creature creature in currentGeneration) {
			Destroy(creature.gameObject);
		}
	}

	private Creature[] CreateCreatures(int count) {

		Creature[] creatures = new Creature[count];

		for(int i = 0; i < count; i++) {
			creatures[i] = CreateCreature();
		}

		return creatures;
	}

	public Creature CreateCreature() {

		var dropPos = dropHeight;
		dropPos.y += safeHeightOffset;

		Creature creat = (Creature) (Instantiate(creature.gameObject, dropPos, Quaternion.identity)).GetComponent<Creature>();
		creat.RefreshLineRenderers();
		creat.Obstacle = this.Obstacle;

		return creat;
	}

	private void ApplyBrains(Creature[] creatures, bool random){

		for (int i = 0; i < creatures.Length; i++) {

			Creature creature = creatures[i];
			string chromosome = random ? "" : currentChromosomes[i];
			ApplyBrain(creature, chromosome);
		}
	}

	public void ApplyBrain(Creature creature, string chromosome) {
		
		Brain brain = creature.GetComponent<Brain>();
		
		if (brain == null) {
			brain = AddBrainComponent(Settings.Task, creature.gameObject);
			brain.muscles = creature.muscles.ToArray();
		}
		
		brain.SimulationTime = cachedSettings.SimulationTime;
		brain.networkSettings = NetworkSettings;
		brain.creature = creature;
		creature.brain = brain;

		brain.SetupNeuralNet(chromosome);
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

	public void UpdateCreaturesWithObstacle(GameObject obstacle) {

		if (currentGeneration == null) return;

		this.Obstacle = obstacle;
		foreach (var creature in currentGeneration) {
			creature.Obstacle = obstacle;
		}
	}

	/// <summary>
	/// Saves the simulation.
	/// </summary>
	/// <returns>The filename of the savefile.</returns>
	// public string SaveSimulation() {

	// 	if (currentGenerationNumber == 1) return null;

	// 	var creatureName = CreatureSaver.GetCurrentCreatureName();
	// 	var creatureSaveData = CreatureSaver.GetCurrentCreatureData();
	// 	var bestChromosomes = BCController.GetBestChromosomes();
	// 	var currentChromosomes = new List<string>(this.currentChromosomes);

	// 	return SimulationSerializer.WriteSaveFile(creatureName, settings, brainSettings, currentGenerationNumber, creatureSaveData, bestChromosomes, currentChromosomes);
	// }
}
