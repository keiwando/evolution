using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

	public const string GRID_SIZE_KEY = "GRID_SIZE";
	public const string GRID_ENABLED_KEY = "GRID_ENABLED";

	public const string KEEP_BEST_CREATURE_KEY = "KEEP_BEST_CREATURE";

	private const string POPULATION_COUNT_KEY = "POPULATION_COUNT";
	private const string GENERATION_TIME_KEY = "GENERATION_TIME";
	private const string BATCH_SIZE_KEY = "BATCH_SIZE";
	private const string BATCH_SIMULATION_ENABLED_KEY = "BATCH_SIMULATION_ENABLED";

	private const string TASK_KEY = "EVOLUTION_TASK";

	private static string[] TASK_OPTIONS = new string[] {"RUNNING", "JUMPING", "OBSTACLE JUMP", "CLIMBING"};

	private const float DEFAULT_GRID_SIZE = 2.0f;

	private static int DEFAULT_POPULATION_COUNT = 10;

	[SerializeField] private AutoScroll contentContainer;

	// Grid stuff
	[SerializeField] private Grid grid;
	[SerializeField] private Slider gridSizeSlider;
	[SerializeField] private Text gridSizeLabel;
	[SerializeField] private Toggle gridToggle;

	// Keep best creatures
	[SerializeField] private Toggle keepBestCreaturesToggle;

	// Batch simulation
	[SerializeField] private InputField batchSizeInput;
	[SerializeField] private Toggle batchSizeToggle;

	// Population size
	[SerializeField] private InputField populationSizeInput;

	// Simulation Time
	[SerializeField] private InputField simulationTimeInput;

	// Evolution Task
	[SerializeField] private Dropdown taskDropdown;

	// Use this for initialization
	void Start () {
		Setup();
	}

	private void Setup() {

		SetupInputFields();
		SetupTaskDropDown();

		// Setup the grid stuff
		var gridSize = PlayerPrefs.GetFloat(GRID_SIZE_KEY, DEFAULT_GRID_SIZE);
		grid.Size = gridSize;
		gridSizeSlider.value = gridSize;
		UpdateGridSizeText(gridSize);

		var gridEnabled = PlayerPrefs.GetInt(GRID_ENABLED_KEY, 0) == 1;
		grid.gameObject.SetActive(gridEnabled);
		gridToggle.isOn = gridEnabled;

		var keepBestCreatures = PlayerPrefs.GetInt(KEEP_BEST_CREATURE_KEY, 0) == 1;
		keepBestCreaturesToggle.isOn = keepBestCreatures;

		var batchSimulationEnabled = PlayerPrefs.GetInt(BATCH_SIMULATION_ENABLED_KEY, 0) == 1;
		BatchSizeToggled(batchSimulationEnabled);
		batchSizeToggle.isOn = batchSimulationEnabled;

		//contentContainer.ScrollToTop();
	}

	public void Show() {
		contentContainer.gameObject.SetActive(true);
	}

	public void Hide() {
		contentContainer.gameObject.SetActive(false);
	}

	public void GridToggled(bool val) {

		PlayerPrefs.SetInt(GRID_ENABLED_KEY, val ? 1 : 0);

		grid.gameObject.SetActive(val);
	}

	public void GridSizeChanged(float value) {

		UpdateGridSizeText(value);
		grid.Size = value;
		grid.VisualRefresh();

		PlayerPrefs.SetFloat(GRID_SIZE_KEY, value);
	}

	private void UpdateGridSizeText(float value) {
		gridSizeLabel.text = value.ToString("0.0");
	}

	public void KeepBestCreaturesToggled(bool value) {
		PlayerPrefs.SetInt(KEEP_BEST_CREATURE_KEY, value ? 1 : 0);
	}

	private void SetupTaskDropDown() {

		var taskString = PlayerPrefs.GetString(TASK_KEY, "RUNNING");
		var index = new List<string>(TASK_OPTIONS).IndexOf(taskString);

		taskDropdown.value = index;
	}

	private void SetupInputFields() {

		populationSizeInput.onEndEdit.AddListener(delegate {
			PopulationSizeChanged();
		});

		simulationTimeInput.onEndEdit.AddListener(delegate {
			SimulationTimeChanged();
		});

		batchSizeInput.onEndEdit.AddListener(delegate {
			BatchSizeChanged();
		});

		SetupDefaultNumbers();
	}

	private void SetupDefaultNumbers() {

		var populationSize = PlayerPrefs.GetInt(POPULATION_COUNT_KEY, DEFAULT_POPULATION_COUNT);
		populationSizeInput.text = populationSize.ToString();

		simulationTimeInput.text = PlayerPrefs.GetInt(GENERATION_TIME_KEY, 10).ToString();

		batchSizeInput.text = ClampBatchSize(PlayerPrefs.GetInt(BATCH_SIZE_KEY, populationSize)).ToString();
	}

	public int GetPopulationSize() {

		var num = Mathf.Clamp(Int32.Parse(populationSizeInput.text), 2, 10000000);
		PlayerPrefs.SetInt(POPULATION_COUNT_KEY, num);

		return num;
	}

	public int GetSimulationTime() {

		var time = Mathf.Clamp(Int32.Parse(simulationTimeInput.text), 1, 100000);
		PlayerPrefs.SetInt(GENERATION_TIME_KEY, time);

		return time;
	}

	private void PopulationSizeChanged() {
		// Make sure the size is at least 2
		var num = Mathf.Clamp(Int32.Parse(populationSizeInput.text), 2, 10000000);
		populationSizeInput.text = num.ToString();
	}

	private void SimulationTimeChanged() {
		// Make sure the time is at least 1
		var time = Mathf.Clamp(Int32.Parse(simulationTimeInput.text), 1, 100000);
		simulationTimeInput.text = time.ToString();
	}

	private int BatchSizeChanged() {
		// Make sure the size is between 1 and the population size
		//var populationSize = Mathf.Clamp(Int32.Parse(populationSizeInput.text), 2, 10000000);

		var batchSize = ClampBatchSize(Int32.Parse(batchSizeInput.text)); //Mathf.Clamp(Int32.Parse(batchSizeInput.text), 1, populationSize);
		batchSizeInput.text = batchSize.ToString();

		return batchSize;
	}

	private int ClampBatchSize(int size) {

		var populationSize = Mathf.Clamp(Int32.Parse(populationSizeInput.text), 2, 10000000);

		return Mathf.Clamp(size, 1, populationSize);
	}

	private int GetBatchSize() {

		return BatchSizeChanged();
	}

	public void BatchSizeToggled(bool val) {

		PlayerPrefs.SetInt(BATCH_SIMULATION_ENABLED_KEY, val ? 1 : 0);

		batchSizeInput.gameObject.SetActive(val);
	}

	/// <summary>
	/// Returns the chosen task based on the value of the taskDropDown;
	/// </summary>
	public Evolution.Task GetTask() {

		var taskString = taskDropdown.captionText.text.ToUpper();
		PlayerPrefs.SetString(TASK_KEY, taskString);

		return Evolution.TaskFromString(taskString);
	}

	public EvolutionSettings GetEvolutionSettings() {

		var settings = new EvolutionSettings();

		settings.batchSize = GetBatchSize();
		settings.keepBestCreatures = keepBestCreaturesToggle.isOn;
		settings.populationSize = GetPopulationSize();
		settings.simulationTime = GetSimulationTime();
		settings.task = GetTask();

		return settings;
	}
}
