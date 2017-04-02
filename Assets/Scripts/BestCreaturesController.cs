using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestCreaturesController : MonoBehaviour {

	public ViewController viewController;

	/// <summary>
	/// The camera that follows the creatures in the main evolution "scene".
	/// </summary>
	public CameraFollowScript MainCamera;
	/// <summary>
	/// The camera used to follow the best creature of the selected generation.
	/// </summary>
	public CameraFollowScript BCCamera;

	public Canvas EvolutionCanvas;
	public Canvas BCCanvas;

	public GameObject BCThumbScreen;
	public InputField BCGenerationInput;

	public Transform floorHeight;
	public Vector3 dropHeight;

	/// <summary>
	/// The list of best creature brains (as chromosome strings). The index + 1 = generation Number.
	/// </summary>
	private List<string> BestCreatures;
	private Dictionary<string, float> BestFitness;

	private Creature creature;
	public Creature Creature {
		set { creature = value; }
	}

	private Creature currentBest;
	/// <summary>
	/// The generation of the currently showing 
	/// </summary>
	private int currentGeneration;

	// Use this for initialization
	void Start () {
		
		BestCreatures = new List<string>();
		BestFitness = new Dictionary<string, float>();

		BCThumbScreen.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Shows the best creatures "scene".
	/// </summary>
	public void ShowBestCreatures() {

		EvolutionCanvas.gameObject.SetActive(false);
		BCCanvas.gameObject.SetActive(true);

		MainCamera.SwitchToMiniViewport();
		BCCamera.SwitchToFullscreen();
	}

	/// <summary>
	/// Shows the main evolution / simulation scene.
	/// </summary>
	public void ShowEvolution() {

		EvolutionCanvas.gameObject.SetActive(true);
		BCCanvas.gameObject.SetActive(false);

		MainCamera.SwitchToFullscreen();
		BCCamera.SwitchToMiniViewport();
	}

	public void AddBestCreature(int generation, string chromosome, float fitness) {
		if (generation <= 0) throw new UnityException();

		BCThumbScreen.gameObject.SetActive(true);

		BestCreatures.Add(chromosome);
		BestFitness.Add(chromosome, fitness);

		if (currentBest == null) {
			ShowBestCreature(1);
		}
	}

	/// <summary>
	/// This function is called when the user finished selecting a new generation to show. 
	/// </summary>
	/// <param name="value">Value.</param>
	public void GenerationSelected(string value) {
		
		int generation;  
		if (!int.TryParse(value, out generation)) {
			throw new System.FormatException("The generation number value could not be parsed to an int");
		}

		GenerationSelected(generation);
	}

	private void GenerationSelected(int generation) {

		if (generation <= 0) {
			viewController.UpdateBCGeneration(currentGeneration);
			return;
		}
		
		// check to see if the selected generation was already simulated. If not, show a message.
		if (BestCreatures.Count < generation) {

			viewController.UpdateBCGeneration(currentGeneration);
			viewController.ShowErrorMessage(string.Format("Generation {0} has not been simulated yet.\n\nCurrently Simulated up to Generation {1}", generation, BestCreatures.Count));
			return;
		} 

		viewController.HideErrorMessage();

		ShowBestCreature(generation);
	}

	private void ShowBestCreature(int generation) {
		
		var chromosome = BestCreatures[generation - 1];
		SpawnCreature(chromosome);

		currentGeneration = generation;
		viewController.UpdateBCGeneration(generation);
	}

	private void SpawnCreature(string chromosome) {
		this.creature.gameObject.SetActive(true);

		if (currentBest != null) {
			Destroy(currentBest.gameObject);
		}

		var creat = CreateCreature();
		Evolution.ApplyBrain(creat, chromosome);
		creat.FloorHeight = floorHeight.position.y;
		creat.Alive = true;

		BCCamera.toFollow = creat;

		currentBest = creat;

		this.creature.gameObject.SetActive(false);
	}

	private Creature CreateCreature(){

		Creature creat = (Creature) ((GameObject) Instantiate(creature.gameObject, dropHeight + floorHeight.position, Quaternion.identity)).GetComponent<Creature>();
		creat.RefreshLineRenderers();
		return creat;
	}

	public void GoToNextGeneration() {
		
		GenerationSelected(currentGeneration + 1);
	}

	public void GoToPreviousGeneration() {
		
		GenerationSelected(currentGeneration - 1);
	}


}
