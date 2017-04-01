using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class Evolution : MonoBehaviour {

	public enum Task {
		RUNNING,
		JUMPING_OVER_BALL,
		STANDING_UP
	}

	public Task task;

	private Dictionary<Task, System.Type> brainMap;

	/** The creature to be evolved. Has no brain by default. */
	public Creature creature;


	public float TimeScale {
		get { return  Time.timeScale; }
		set {
			Time.timeScale = value;
		}
	}

	public int SimulationTime {
		set { SIMULATION_TIME = value; }
		get { return SIMULATION_TIME; }
	}
	private int SIMULATION_TIME = 10;	// in seconds

	public int PopulationSize {
		set { 
			if (value % 2 == 0) {
				POPULATION_SIZE = value;
			} else {
				POPULATION_SIZE = value + 1;	
			}
		}
		get { return POPULATION_SIZE; }
	}
	private int POPULATION_SIZE = 10;

	private float MUTATION_RATE = 0.5f;

	private int currentGenerationNumber = 1;

	private int[] randomPickingWeights;

	/** The current generation of Creatures. */
	private Creature[] currentGeneration;
	/** The chromosome strings of the current generation. */
	private string[] currentChromosomes;

	private BestCreaturesController BCController;

	/** The height at which the  */
	private Vector3 dropHeight;

	private bool running;

	// UI
	private ViewController viewController;

	// Use this for initialization
	void Start () {

		brainMap = new Dictionary<Task, System.Type>();
		brainMap.Add(Task.RUNNING, typeof(RunningBrain));

	}
	
	// Update is called once per frame
	void Update () {

		HandleKeyboardInput();
	}

	private void HandleKeyboardInput() {

		if (!running) { return; }

		if (Input.anyKeyDown) {

			if (Input.GetKeyDown(KeyCode.LeftArrow)) {

				CameraFollowScript cam = Camera.main.GetComponent<CameraFollowScript>();
				int index = cam.currentlyWatchingIndex;
				cam.currentlyWatchingIndex = index - 1 < 0 ? currentGeneration.Length - 1 : index - 1;
				cam.toFollow = currentGeneration[index];
			
			} else if (Input.GetKeyDown(KeyCode.RightArrow)) {

				CameraFollowScript cam = Camera.main.GetComponent<CameraFollowScript>();
				int index = (cam.currentlyWatchingIndex + 1 ) % POPULATION_SIZE ;
				cam.currentlyWatchingIndex = index;
				cam.toFollow = currentGeneration[index];

			} else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) {
				// Go back to the Creature building view.
				SceneManager.LoadScene("CreatureBuildingScene");
				KillGeneration();
				Destroy(creature.gameObject);
				Destroy(this.gameObject);
				running = false;
			}
		}
	}

	private void SetupEvolution() {

		CalculateDropHeight();

		BCController = GameObject.Find("Best Creature Controller").GetComponent<BestCreaturesController>();
		BCController.dropHeight = dropHeight;

		string[] currentChromosomes = new string[POPULATION_SIZE];
		// The first generation will have random brains.
		currentGeneration = CreateCreatures();
		ApplyBrains(currentGeneration, true);
		SimulateGeneration();

		creature.gameObject.SetActive(false);

		Camera.main.GetComponent<CameraFollowScript>().toFollow = currentGeneration[0];

		//TestCopy();
	}

	private void TestCopy() {
		Muscle lastMuscle = null;
		foreach(Creature creatures in currentGeneration) {
			if (lastMuscle != null) {
				print("Muscles Equal: " + lastMuscle.Equals(creature.muscles[0]));
			}
			lastMuscle = creature.muscles[0];
		}
	}

	/** Starts the Evolution for the current */
	public void StartEvolution() {

		viewController = GameObject.Find("ViewController").GetComponent<ViewController>();
		Assert.IsNotNull(viewController);

		creature.Alive = false;
		running = true;
		SetupEvolution();
	}

	/** Simulates the task for every creature in the current generation. */
	private void SimulateGeneration() {

		foreach( Creature creature in currentGeneration ) {
			creature.Alive = true;
		}

		StartCoroutine(StopSimulationAfterTime(SIMULATION_TIME));
	}

	IEnumerator StopSimulationAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		EndSimulation();
	}

	private void EndSimulation() {

		if(!running) return;

		foreach( Creature creature in currentGeneration ) {
			creature.Alive = false;
		}

		EvaluateCreatures(currentGeneration);
		SortGenerationByFitness();

		print("Highest Fitness: " + currentGeneration[0].brain.fitness);
		// save the best creature
		//bestCreatures[currentGenerationNumber] = currentGeneration[0];
		BCController.AddBestCreature(currentGenerationNumber, currentGeneration[0].brain.ToChromosomeString());

		currentChromosomes = CreateNewChromosomesFromGeneration();
		currentGenerationNumber++;

		KillGeneration();
		currentGeneration = CreateGeneration();
		Camera.main.GetComponent<CameraFollowScript>().toFollow = currentGeneration[0];

		// Update the view
		viewController.UpdateGeneration(currentGenerationNumber);


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

		string[] result = new string[POPULATION_SIZE];
		SetupRandomPickingWeights();

		// keep the two best creatures
		result[0] = currentGeneration[0].brain.ToChromosomeString();
		result[1] = currentGeneration[1].brain.ToChromosomeString();

		for(int i = 2; i < POPULATION_SIZE; i += 2) {

			// randomly pick two creatures and let them "mate"
			int index1 = PickRandomWeightedIndex();
			int index2 = PickRandomWeightedIndex();

			string chrom1 = currentGeneration[index1].brain.ToChromosomeString();
			string chrom2 = currentGeneration[index2].brain.ToChromosomeString();

			string[] newChromosomes = CombineChromosomes(chrom1, chrom2);

			Assert.AreEqual(chrom1.Length, chrom2.Length);
			Assert.AreEqual(chrom1.Length, newChromosomes[0].Length);

			result[i] = newChromosomes[0];
			result[i+1] = newChromosomes[1];
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
		int index = UnityEngine.Random.Range(0,chromosome.Length - 1);
		// determine a mutation length
		int length = Mathf.Min(chromosome.Length - index - 3, UnityEngine.Random.Range(2,15));

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
	/// Picks an index between 0 and POPULATION_SIZE. The first indices as more likely to be picked. The weights decrease towards to.
	/// </summary>
	/// <returns>The randomly weighted index.</returns>
	private int PickRandomWeightedIndex() {

		int number = UnityEngine.Random.Range(0, randomPickingWeights[0] - 1);
		// find the index in the random pickingweights
		for(int i = POPULATION_SIZE - 1; i >= 0; i--) {
			if( randomPickingWeights[i] >= number ) {
				return i;
			}
		} 

		return 0;
	}

	/** Initialized the weights array for randomly picking the  */
	private void SetupRandomPickingWeights() {

		int[] weights = new int[POPULATION_SIZE];
		// fill the weights array
		int value = 1;
		for (int i = 0; i < POPULATION_SIZE; i++) {
			weights[weights.Length - 1 - i] = value;
			value += i;
		}

		randomPickingWeights = weights;
	}

	/** Creates a generation of creatures with the current set of Chromosomes. */
	private Creature[] CreateGeneration() {
		
		this.creature.gameObject.SetActive(true);

		Creature[] creatures = new Creature[POPULATION_SIZE];
		Creature creature;

		for(int i = 0; i < POPULATION_SIZE; i++) {
			creature = CreateCreature();
			ApplyBrain(creature, currentChromosomes[i]);
			creatures[i] = creature;
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

		Creature[] creatures = new Creature[POPULATION_SIZE];

		for(int i = 0; i < POPULATION_SIZE; i++) {
			creatures[i] = CreateCreature();
		}

		return creatures;
	}

	private Creature CreateCreature(){

		Creature creat = (Creature) ((GameObject) Instantiate(creature.gameObject, dropHeight, Quaternion.identity)).GetComponent<Creature>();
		creat.RefreshLineRenderers();
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

	private void ApplyBrain(Creature creature, string chromosome) {

		Brain brain = (Brain) creature.gameObject.AddComponent(brainMap[task]);
		brain.muscles = creature.muscles.ToArray();

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
}
