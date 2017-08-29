using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Evolution : MonoBehaviour {

	public enum Task {
		RUNNING = 0,
		JUMPING = 1,
		OBSTACLE_JUMP = 2,
		CLIMBING = 3
	}

	// TODO: Rewrite this whole logic
	public static Task TaskForNumber(int n) {
		switch(n) {
		case 0: return Task.RUNNING; 
		case 1: return Task.JUMPING; 
		case 2: return Task.OBSTACLE_JUMP; 
		case 3: return Task.CLIMBING;
		}

		return Task.RUNNING;
	}

	public static string TaskToString(Task task) {
		switch(task) {
		case Task.RUNNING: return "Running";
		case Task.JUMPING: return "Jumping"; 
		case Task.OBSTACLE_JUMP: return "Obstacle Jump";
		case Task.CLIMBING: return "Climbing";
		}

		return "Running";
	}

	public static Task TaskFromString(string task) {

		switch(task.ToUpper()) {

		case "RUNNING": return Task.RUNNING; 
		case "JUMPING": return Task.JUMPING; 
		case "OBSTACLE JUMP": return Task.OBSTACLE_JUMP;
		case "CLIMBING": return Task.CLIMBING; 

		default: return Task.RUNNING;
		}
	}

	public EvolutionSettings Settings {
		set { settings = value; }
		get { return settings; }
	}
	private EvolutionSettings settings;

	public NeuralNetworkSettings BrainSettings {
		set { brainSettings = value; }
		get { return brainSettings; }
	}
	private NeuralNetworkSettings brainSettings;

	//public static Task task;

	private static Dictionary<Task, System.Type> brainMap;

	public GameObject obstacle;

	/** The creature to be evolved. Has no brain by default. */
	public Creature creature;


	public float TimeScale {
		get { return  Time.timeScale; }
		set {
			Time.timeScale = value;
		}
	}

	//private float MUTATION_RATE = 0.5f;
	private float MUTATION_RATE {
		get { return settings.mutationRate / 100f; }
	}

	private int currentGenerationNumber = 1;

	private int[] randomPickingWeights;

	/// <summary>
	/// The current generation of Creatures.
	/// </summary>
	public Creature[] currentGeneration;

	/// <summary>
	/// The chromosome strings of the current generation.
	/// </summary>
	private string[] currentChromosomes;

	// Batch simulation

	private bool simulateInBatchesCached;
	public bool ShouldSimulateInBatches { get { return simulateInBatchesCached; } }

	// If generation should be simulated in batches the batch size needs to be cached
	// at the beginning of each generation simulation to prevent problems with the adjustable
	// settings.
	private int batchSizeCached;
	public int CurrentBatchSize { get { return batchSizeCached; } }

	/// <summary>
	/// The numbe of the currently simulating batch. Between 1 and Ceil(populationSize / batchSizeCached)
	/// </summary>
	private int currentlySimulatingBatch;
	public int CurrentBatchNumber { get { return currentlySimulatingBatch; } }

	/// <summary>
	/// The currently simulating batch of creatures (a subset of currentGeneration).
	/// </summary>
	private Creature[] currentCreatureBatch;



	private BestCreaturesController BCController;

	/// <summary>
	/// The height from the ground from which the creatures should be dropped on spawn.
	/// </summary>
	private Vector3 dropHeight;

	/// <summary>
	/// Whether the simulation is currently running or not. (Paused is still true)
	/// </summary>
	private bool running;

	// UI
	private ViewController viewController;

	// Auto-Save
	private AutoSaver autoSaver;

	// DEBUG: TODO: REMOVE!!
	public Creature currentBest;
	public Creature[] currentBestArr;

	// Use this for initialization
	void Start () {

		brainMap = new Dictionary<Task, System.Type>();
		brainMap.Add(Task.RUNNING, typeof(RunningBrain));
		brainMap.Add(Task.OBSTACLE_JUMP, typeof(ObstacleJumpingBrain));
		brainMap.Add(Task.JUMPING, typeof(JumpingBrain));
		brainMap.Add(Task.CLIMBING, typeof(ClimbingBrain));

	}
	
	// Update is called once per frame
	void Update () {

		HandleKeyboardInput();
	}

	private void HandleKeyboardInput() {

		if (!running) { return; }

		if (Input.anyKeyDown) {

			if (Input.GetKeyDown(KeyCode.LeftArrow)) {

				FocusOnPreviousCreature();
			
			} else if (Input.GetKeyDown(KeyCode.RightArrow)) {

				FocusOnNextCreature();

			} else if (Input.GetKeyDown(KeyCode.Escape)) {
				GoBackToCreatureBuilding();
			}

			if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Backspace)) {
				GoBackToCreatureBuilding();
			}

		}
	}

	public void GoBackToCreatureBuilding() {
		// Go back to the Creature building view.
		SceneManager.LoadScene("CreatureBuildingScene");
		KillGeneration();
		Destroy(creature.gameObject);
		Destroy(this.gameObject);
		running = false;
	}

	public void FocusOnNextCreature() {

		CameraFollowScript cam = Camera.main.GetComponent<CameraFollowScript>();
		int index = (cam.currentlyWatchingIndex + 1 ) % currentCreatureBatch.Length;
		cam.currentlyWatchingIndex = index;
		//cam.toFollow = currentGeneration[index];
		cam.toFollow = currentCreatureBatch[index];

		RefreshVisibleCreatures();
	}

	public void FocusOnPreviousCreature() {

		CameraFollowScript cam = Camera.main.GetComponent<CameraFollowScript>();
		int index = cam.currentlyWatchingIndex;
		//cam.currentlyWatchingIndex = index - 1 < 0 ? currentGeneration.Length - 1 : index - 1;
		cam.currentlyWatchingIndex = index - 1 < 0 ? currentCreatureBatch.Length - 1 : index - 1;
		//cam.toFollow = currentGeneration[index];
		cam.toFollow = currentCreatureBatch[cam.currentlyWatchingIndex];

		//print("Index: " + index);

		RefreshVisibleCreatures();
	}

	public void RefreshVisibleCreatures() {

		// Determine if all or only one creature should be visible
		if (settings.showOneAtATime) {

			foreach (var creature in currentCreatureBatch) {
				creature.SetOnInvisibleLayer();
			}

			CameraFollowScript cam = Camera.main.GetComponent<CameraFollowScript>();
			currentCreatureBatch[cam.currentlyWatchingIndex].SetOnVisibleLayer();
		
		} else {

			foreach (var creature in currentCreatureBatch) {
				creature.SetOnVisibleLayer();
			}

			// TODO: REMOVE DEBUG
			print("DEBUG 4");
			foreach (var creature in currentBestArr) {
				creature.SetOnVisibleLayer();
			}

			// END DEBUG
		}
	}



	/*private void TestCopy() {
		
		Muscle lastMuscle = null;
		foreach(Creature creatures in currentGeneration) {
			if (lastMuscle != null) {
				print("Muscles Equal: " + lastMuscle.Equals(creature.muscles[0]));
			}
			lastMuscle = creature.muscles[0];
		}
	}*/

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
	public void ContinueEvolution(int generationNum, EvolutionSettings evolutionSettings, NeuralNetworkSettings networkSettings, List<ChromosomeStats> bestChromosomes, List<string> currentChromosomes) {

		this.settings = evolutionSettings;
		this.brainSettings = networkSettings;

		viewController = GameObject.Find("ViewController").GetComponent<ViewController>();
		Assert.IsNotNull(viewController);

		this.currentGenerationNumber = generationNum;
		this.currentChromosomes = currentChromosomes.ToArray();

		creature.Alive = false;
		running = true;

		viewController.UpdateGeneration(generationNum);

		autoSaver = new AutoSaver();

		// Setup Evolution call
		CalculateDropHeight();

		BCController = GameObject.Find("Best Creature Controller").GetComponent<BestCreaturesController>();
		BCController.dropHeight = dropHeight;
		BCController.Creature = creature;

		BCController.SetBestChromosomes(bestChromosomes);
		BCController.ShowBCThumbScreen();
		BCController.RunBestCreatures(generationNum - 1);

		currentGeneration = CreateGeneration();

		// Batch simulation
		currentlySimulatingBatch = 1;
		simulateInBatchesCached = settings.simulateInBatches;
		batchSizeCached = settings.simulateInBatches ? settings.batchSize : settings.populationSize;
		var currentBatchSize = Mathf.Min(batchSizeCached, settings.populationSize - ((currentlySimulatingBatch - 1) * batchSizeCached));
		currentCreatureBatch = new Creature[currentBatchSize]; 

		Array.Copy(currentGeneration, 0, currentCreatureBatch, 0, currentBatchSize);
		//Array.Copy(currentBestArr, 0, currentCreatureBatch, 0, currentBatchSize);
		//currentGeneration = currentBestArr;


		creature.gameObject.SetActive(false);

		SimulateGeneration();

		var cameraFollow = Camera.main.GetComponent<CameraFollowScript>();
		cameraFollow.toFollow = currentGeneration[0];
		cameraFollow.currentlyWatchingIndex = 0;

		RefreshVisibleCreatures();

		print("DEBUG 5");
	}

	/** Starts the Evolution for the current */
	public void StartEvolution() {

		viewController = GameObject.Find("ViewController").GetComponent<ViewController>();
		Assert.IsNotNull(viewController);

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
		currentlySimulatingBatch = 1;
		simulateInBatchesCached = settings.simulateInBatches;
		batchSizeCached = settings.simulateInBatches ? settings.batchSize : settings.populationSize;
		var currentBatchSize = Mathf.Min(batchSizeCached, settings.populationSize - ((currentlySimulatingBatch - 1) * batchSizeCached));
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
		//TestCopy();
	}
		
	/// <summary>
	/// Simulates the task for every creature in the current batch
	/// </summary>
	private void SimulateGeneration() {

		foreach (Creature creature in currentGeneration) {
			print("alive false: " + creature.name);
			creature.Alive = false;
			creature.gameObject.SetActive(false);
		}

		foreach (Creature creature in currentCreatureBatch) {
			print("alive 2 true: " + creature.name);
			creature.Alive = true;
			creature.gameObject.SetActive(true);
		}

		// TODO: REMOVE DEBUG
		/*
		foreach (Creature creature in currentBestArr) {
			print("alive 3 false: " + creature.name);
			creature.Alive = false;
			creature.gameObject.SetActive(false);
		}

		foreach (Creature creature in currentBestArr) {
			print("alive 4 true: " + creature.name);
			creature.Alive = true;
			creature.gameObject.SetActive(true);
		}*/

		// END DEBUG

		StartCoroutine(StopSimulationAfterTime(settings.simulationTime));
	}

	IEnumerator StopSimulationAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		// Check if all of the batches of the current generation were simulated.
		var creaturesLeft = settings.populationSize - (currentlySimulatingBatch * batchSizeCached);
		if (simulateInBatchesCached && creaturesLeft > 0 ) {
			// Simulate the next batch first

			var currentBatchSize = Mathf.Min(batchSizeCached, creaturesLeft);
			currentCreatureBatch = new Creature[currentBatchSize];
			Array.Copy(currentGeneration, currentlySimulatingBatch * batchSizeCached, currentCreatureBatch, 0, currentBatchSize);
			currentlySimulatingBatch += 1;

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

		KillGeneration();
		currentGeneration = CreateGeneration();

		var cameraFollow = Camera.main.GetComponent<CameraFollowScript>();
		cameraFollow.toFollow = currentGeneration[0];
		cameraFollow.currentlyWatchingIndex = 0;

		// Batch simulation
		currentlySimulatingBatch = 1;
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
		SetupRandomPickingWeights();

		for(int i = 0; i < settings.populationSize; i += 2) {

			// randomly pick two creatures and let them "mate"
			int index1 = PickRandomWeightedIndex();
			int index2 = PickRandomWeightedIndex();

			string chrom1 = currentGeneration[index1].brain.ToChromosomeString();
			string chrom2 = currentGeneration[index2].brain.ToChromosomeString();

			string[] newChromosomes = CombineChromosomes(chrom1, chrom2);

			Assert.AreEqual(chrom1.Length, chrom2.Length);
			Assert.AreEqual(chrom1.Length, newChromosomes[0].Length);

			result[i] = newChromosomes[0];
			if (i + 1 < result.Length) {
				result[i+1] = newChromosomes[1];
			}
		}

		if (settings.keepBestCreatures) {
			
			// keep the two best creatures
			result[0] = currentGeneration[0].brain.ToChromosomeString();
			result[1] = currentGeneration[1].brain.ToChromosomeString();
		}

		return result;

	}

	/// <summary>
	/// Takes two chromosome strings and returns an array of two new chromosome strings that are a combination of the parent strings.
	/// </summary>
	private string[] CombineChromosomes(string chrom1, string chrom2) {

		int splitIndex = UnityEngine.Random.Range(1, chrom2.Length);
		string[] result = new string[2];

		result[0] = chrom1.Substring(0,splitIndex) + chrom2.Substring(splitIndex);
		result[1] = chrom2.Substring(0,splitIndex) + chrom1.Substring(splitIndex);

		Assert.AreEqual(result[0].Length, chrom1.Length);
		Assert.AreEqual(result[1].Length, chrom2.Length);

		result[0] = Mutate(result[0]);
		result[1] = Mutate(result[1]);

		return result;
	}


	private string Mutate(string chromosome) {

		bool shouldMutate = UnityEngine.Random.Range(0,100.0f) < (MUTATION_RATE * 100);

		if (!shouldMutate) return chromosome;

		// pick a mutation index.
		int index = UnityEngine.Random.Range(4,chromosome.Length - 1);
		// determine a mutation length
		int length = Mathf.Min(Mathf.Max(0, chromosome.Length - index - 3), UnityEngine.Random.Range(2,15));

		string toMutate = chromosome.Substring(index, length);
		string mutatedPart = "";

		for(int i = 0; i < toMutate.Length; i++) {

			char character = toMutate[i];
			mutatedPart += character == '0' ? '1' : '0';
		}

		string mutated = chromosome.Substring(0,index) + mutatedPart + chromosome.Substring(index + length);

		Assert.AreEqual(chromosome.Length, mutated.Length);
		return mutated;
	}

	/// <summary>
	/// Picks an index between 0 and POPULATION_SIZE. The first indices are more likely to be picked. The weights decrease towards to.
	/// </summary>
	/// <returns>The randomly weighted index.</returns>
	private int PickRandomWeightedIndex() {

		int number = UnityEngine.Random.Range(0, randomPickingWeights[0] - 1);
		// find the index in the random pickingweights
		for(int i = settings.populationSize - 1; i >= 0; i--) {
			if( randomPickingWeights[i] >= number ) {
				return i;
			}
		} 

		return 0;
	}

	/** Initialized the weights array for randomly picking the  */
	private void SetupRandomPickingWeights() {

		int[] weights = new int[settings.populationSize];
		// fill the weights array
		int value = 1;
		for (int i = 0; i < settings.populationSize; i++) {
			weights[weights.Length - 1 - i] = value;
			value += i;
		}

		randomPickingWeights = weights;
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


		// TODO: REMOVE DEBUG
		/*
		print("Debug 1");
		//currentBest = CreateCreature();
		//ApplyBrain(currentBest, currentChromosomes[0]);
		//currentBest.name = "Current Best Test";
		//currentBest.Alive = true;

		var testSet = new List<Creature>();

		for(int i = 0; i < settings.populationSize; i++) {
			creature = CreateCreature();
			ApplyBrain(creature, currentChromosomes[i]);
			testSet.Add(creature);
			//creatures[i] = creature;

			creature.name = "Test Creature " + (i+1);

			//creature.Alive = true;
		}

		currentBestArr = testSet.ToArray();
		// DEBUG END
		*/


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

	public Creature CreateCreature(){

		Creature creat = (Creature) ((GameObject) Instantiate(creature.gameObject, dropHeight, Quaternion.identity)).GetComponent<Creature>();
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

		if (brainMap == null) throw new System.Exception("The brain map is not initialized");

		Brain brain = (Brain) creature.gameObject.AddComponent(brainMap[settings.task]);
		brain.muscles = creature.muscles.ToArray();
		brain.SimulationTime = settings.simulationTime;
		brain.networkSettings = brainSettings;

		brain.SetupNeuralNet(chromosome);	

		brain.creature = creature;
		creature.brain = brain;
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

		//return EvolutionSaver.WriteSaveFile(creatureName, settings.task, settings.simulationTime, currentGenerationNumber, creatureSaveData, bestChromosomes, currentChromosomes); 
		return EvolutionSaver.WriteSaveFile(creatureName, settings, brainSettings, currentGenerationNumber, creatureSaveData, bestChromosomes, currentChromosomes);
	}

	public void SetAutoSaveEnabled(bool value) {

		if (autoSaver != null) {
			autoSaver.Enabled = value;	
		}
	}

	/** Creates a generation of creatures with the current set of Chromosomes. */
	public Creature[] CreateTestCreatureSet(string chromosome) {

		this.creature.gameObject.SetActive(true);

		Creature[] creatures = new Creature[settings.populationSize];

		for(int i = 0; i < settings.populationSize; i++) {
			
			var creature = CreateCreature();
			ApplyBrain(creature, chromosome);
			creatures[i] = creature;

			creature.name = "Best Creature " + (i+1);
		}

		this.creature.gameObject.SetActive(false);
		return creatures;
	}
}
