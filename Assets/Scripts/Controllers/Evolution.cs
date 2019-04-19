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

	public SimulationSettings Settings {
		set { settings = value; }
		get { return settings; }
	}
	private SimulationSettings settings;

	private NeuralNetworkSettings brainSettings;

	// Cached values

	private bool simulateInBatchesCached;
	public bool ShouldSimulateInBatches { get { return simulateInBatchesCached; } }

	// If generation should be simulated in batches the batch size needs to be cached
	// at the beginning of each generation simulation to prevent problems with the adjustable
	// settings.
	private int batchSizeCached;

	/// <summary>
	/// The number of creatures that are currently being simulated at once.
	/// </summary>
	/// <value></value>
	public int CurrentBatchSize { get { return batchSizeCached; } }

	#endregion
	#region Global Simulation Data


	private SimulationData simulationData;
	
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
	public Creature[] currentGeneration;

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

	private BestCreaturesController BestCreaturesController;
	// TODO: Get rid of this reference
	private ViewController viewController;
	private AutoSaver autoSaver;

	#endregion
	
	void Start () {

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		this.autoSaver = new AutoSaver();
		this.

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
	/// Continues
	/// </summary>
	private void StartSimulation(SimulationData data) {

		this.brainSettings = data.NetworkSettings;
		this.settings = data.Settings;
		this.simulationData = data;

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
		
		// Batch simulation
		this.currentBatchNumber = 1;


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

		string[] result = new string[settings.populationSize];

		var lazyChromosomes = new List<LazyChromosomeData>();
		foreach (var creature in currentGeneration) {
			lazyChromosomes.Add(new LazyChromosomeData(creature, creature.GetStatistics()));
		}
		var selection = new Selection<LazyChromosomeData>(Selection<LazyChromosomeData>.Mode.FitnessProportional, lazyChromosomes);
		

		chromosomeCache.Clear();

		int start = 0;
		if (settings.keepBestCreatures) {

			// keep the two best creatures
			var best = selection.SelectBest(2);
			result[0] = best[0].Chromosome;
			result[1] = best[1].Chromosome;
			// result[0] = GetChromosomeWithCaching(0);
			// result[1] = GetChromosomeWithCaching(1);
			start = 2;
		}

		for(int i = start; i < settings.populationSize; i += 2) {

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
				result[i+1] = newChromosomes[1];
			}
		}

		return result;

	}

	/// <summary>
	/// Optimized
	/// Takes two chromosome strings and returns an array of two new chromosome strings that are a combination of the parent strings.
	/// </summary>
	private string[] CombineChromosomes(string chrom1, string chrom2) {

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

		bool shouldMutate = UnityEngine.Random.Range(0,100.0f) < settings.mutationRate;

		if (!shouldMutate) return chromosome;

		// pick a mutation index.
		int index = UnityEngine.Random.Range(4,chromosome.Length - 1);
		// determine a mutation length
		int length = Mathf.Min(Mathf.Max(0, chromosome.Length - index - 3), UnityEngine.Random.Range(2,15));

		for(int i = 0; i < length; i++) {

			char character = chromosome[i];
			chromosome[i] = character == '0' ? '1' : '0';
		}

		return chromosome;
	}

	private void ResetCreatures() {

		for (int i = 0; i < currentGeneration.Length; i++) {

			var currentCreature = currentGeneration[i];
			currentCreature.Reset();
			//ApplyBrain(currentCreature, currentChromosomes[i]);
			UpdateExistingBrain(currentCreature, currentChromosomes[i]);
		}
	}

	/** Creates a generation of creatures with the current set of Chromosomes. */
	private Creature[] CreateGeneration() {
		
		this.creature.gameObject.SetActive(true);

		Creature[] creatures = new Creature[settings.populationSize];
		Creature creature;

		for(int i = 0; i < settings.populationSize; i++) {
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

	/** Creates an array of creatures. */
	private Creature[] CreateCreatures() {

		Creature[] creatures = new Creature[settings.populationSize];

		for(int i = 0; i < settings.populationSize; i++) {
			creatures[i] = CreateCreature();
		}

		return creatures;
	}

	public Creature CreateCreature() {

		var dropPos = dropHeight;
		dropPos.y += safeHeightOffset;

		Creature creat = (Creature) ((GameObject) Instantiate(creature.gameObject, dropPos, Quaternion.identity)).GetComponent<Creature>();
		creat.RefreshLineRenderers();
		creat.Obstacle = obstacle;

		return creat;
	}

	/** Applies brains to an array of creatures. */
	private void ApplyBrains(Creature[] creatures, bool random){

		for (int i = 0; i < creatures.Length; i++) {

			Creature creature = creatures[i];
			string chromosome = random ? "" : currentChromosomes[i];
			ApplyBrain(creature, chromosome);
		}
	}

	public void ApplyBrain(Creature creature, string chromosome) {

		Brain brain = AddBrainComponent(settings.task, creature.gameObject);
		brain.muscles = creature.muscles.ToArray();
		brain.SimulationTime = settings.simulationTime;
		brain.networkSettings = brainSettings;

		brain.SetupNeuralNet(chromosome);	

		brain.creature = creature;
		creature.brain = brain;
	}

	public void UpdateExistingBrain(Creature creature, string chromosome) {

		var brain = creature.GetComponent<Brain>();
		brain.SimulationTime = settings.simulationTime;
		brain.networkSettings = brainSettings;

		brain.SetupNeuralNet(chromosome);
	}

	private Brain AddBrainComponent(EvolutionTask task, GameObject gameObject) {
		switch (task) {
		case EvolutionTask.RUNNING:
			return gameObject.AddComponent<RunningBrain>();
		case EvolutionTask.JUMPING:
			return gameObject.AddComponent<JumpingBrain>();
		case EvolutionTask.OBSTACLE_JUMP:
			return gameObject.AddComponent<ObstacleJumpingBrain>();
		case EvolutionTask.CLIMBING:
			return gameObject.AddComponent<ClimbingBrain>();
		default:
			throw new System.ArgumentException(string.Format("There is no brain type for the given task: {0}", task));
		}
	}

	private void CalculateDropHeight() {

		float DistanceFromGround = creature.DistanceFromGround();
		float padding = 0.5f;
		dropHeight = creature.transform.position;
		dropHeight.y -= DistanceFromGround - padding;
	}

	public void UpdateCreaturesWithObstacle(GameObject obstacle) {

		if (currentGeneration == null) return;

		this.obstacle = obstacle;
		foreach (var creature in currentGeneration) {
			creature.Obstacle = obstacle;
		}
	}

	/// <summary>
	/// Saves the simulation.
	/// </summary>
	/// <returns>The filename of the savefile.</returns>
	public string SaveSimulation() {

		if (currentGenerationNumber == 1) return null;

		var creatureName = CreatureSaver.GetCurrentCreatureName();
		var creatureSaveData = CreatureSaver.GetCurrentCreatureData();
		var bestChromosomes = BCController.GetBestChromosomes();
		var currentChromosomes = new List<string>(this.currentChromosomes);

		return SimulationSerializer.WriteSaveFile(creatureName, settings, brainSettings, currentGenerationNumber, creatureSaveData, bestChromosomes, currentChromosomes);
	}

	public void SetAutoSaveEnabled(bool value) {

		if (autoSaver != null) {
			autoSaver.Enabled = value;	
		}
	}
}
