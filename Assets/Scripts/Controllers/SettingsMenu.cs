using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

	private static string[] TASK_OPTIONS = new string[] {"RUNNING", "JUMPING", "OBSTACLE JUMP", "CLIMBING"};

	private const float DEFAULT_GRID_SIZE = 2.0f;

	public bool IsShowing { 
		get { return contentContainer.gameObject.activeSelf; }
	}

	public AutoScroll.ScrollPos ScrollPos { 
		get { return contentContainer.CurrentScrollPos; }
	}

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

	// Mutation rate
	[SerializeField] private InputField mutationRateInput;

	// Use this for initialization
	void Start () {
		Setup();
	}

	private void Setup() {

		SetupInputFieldCallbacks();
		SetupTaskDropDown();

		// Setup the grid stuff
		var gridSize = Settings.GridSize;
		grid.Size = gridSize;
		gridSizeSlider.value = gridSize;
		UpdateGridSizeText(gridSize);

		var gridEnabled = Settings.GridEnabled;
		grid.gameObject.SetActive(gridEnabled);
		gridToggle.isOn = gridEnabled;

		// Evolution settings
		var settings = LoadSimulationSettings();

		keepBestCreaturesToggle.isOn = settings.KeepBestCreatures;

		BatchSizeToggled(settings.SimulateInBatches);
		batchSizeToggle.isOn = settings.SimulateInBatches;

		batchSizeInput.text = settings.BatchSize.ToString();
		populationSizeInput.text = settings.PopulationSize.ToString();
		simulationTimeInput.text = settings.SimulationTime.ToString();
		mutationRateInput.text = settings.MutationRate.ToString();
	}

	private void SetupInputFieldCallbacks() {

		populationSizeInput.onEndEdit.AddListener(delegate {
			PopulationSizeChanged();
		});

		simulationTimeInput.onEndEdit.AddListener(delegate {
			SimulationTimeChanged();
		});

		batchSizeInput.onEndEdit.AddListener(delegate {
			BatchSizeChanged();
		});

		mutationRateInput.onEndEdit.AddListener(delegate {
			MutationRateChanged();
		});
	}

	private void SetupTaskDropDown() {

		var settings = LoadSimulationSettings();
		var taskString = settings.Task.StringRepresentation().ToUpper();

		var index = new List<string>(TASK_OPTIONS).IndexOf(taskString);

		taskDropdown.value = index;
	}

	public void TaskChanged() {

		var settings = LoadSimulationSettings();
		settings.Task = EvolutionTaskUtil.TaskFromString(taskDropdown.captionText.text);
		SaveSimulationSettings(settings);
	}

	public void Show() {
		contentContainer.gameObject.SetActive(true);
	}

	public void Hide() {
		contentContainer.gameObject.SetActive(false);
	}

	public void GoToTop() {
		contentContainer.ScrollToTop();
	}

	public void GridToggled(bool val) {

		Settings.GridEnabled = val;
		grid.gameObject.SetActive(val);
	}

	public void GridSizeChanged(float value) {

		UpdateGridSizeText(value);
		grid.Size = value;
		grid.VisualRefresh();

		Settings.GridSize = value;
	}

	private void UpdateGridSizeText(float value) {
		gridSizeLabel.text = value.ToString("0.0");
	}

	public void KeepBestCreaturesToggled(bool value) {

		var settings = LoadSimulationSettings();
		settings.KeepBestCreatures = value;
		SaveSimulationSettings(settings);
	}

	private void PopulationSizeChanged() {
		// Make sure the size is at least 2
		var num = Mathf.Clamp(Int32.Parse(populationSizeInput.text), 2, 10000000);
		populationSizeInput.text = num.ToString();

		var settings = LoadSimulationSettings();
		settings.PopulationSize = num;
		SaveSimulationSettings(settings);
	}

	private void SimulationTimeChanged() {
		// Make sure the time is at least 1
		var time = Mathf.Clamp(Int32.Parse(simulationTimeInput.text), 1, 100000);
		simulationTimeInput.text = time.ToString();

		var settings = LoadSimulationSettings();
		settings.SimulationTime = time;
		SaveSimulationSettings(settings);
	}

	private void MutationRateChanged() {
		// Clamp between 1 and 100 %
		var rate = Mathf.Clamp(int.Parse(mutationRateInput.text), 1, 100);
		mutationRateInput.text = rate.ToString();

		var settings = LoadSimulationSettings();
		settings.MutationRate = rate;
		SaveSimulationSettings(settings);
	}

	private void BatchSizeChanged() {
		// Make sure the size is between 1 and the population size
		var batchSize = ClampBatchSize(Int32.Parse(batchSizeInput.text));
		batchSizeInput.text = batchSize.ToString();

		var settings = LoadSimulationSettings();
		settings.BatchSize = batchSize;
		SaveSimulationSettings(settings);
	}

	private int ClampBatchSize(int size) {

		var settings = LoadSimulationSettings();
		var populationSize = settings.PopulationSize;

		return Mathf.Clamp(size, 1, populationSize);
	}

	public void BatchSizeToggled(bool val) {

		batchSizeInput.gameObject.SetActive(val);

		var settings = LoadSimulationSettings();
		settings.SimulateInBatches = val;
		SaveSimulationSettings(settings);
	}

	/// <summary>
	/// Returns the chosen task based on the value of the taskDropDown;
	/// </summary>
	public EvolutionTask GetTask() {

		var taskString = taskDropdown.captionText.text.ToUpper();
		var task = EvolutionTaskUtil.TaskFromString(taskString);
		
		var settings = LoadSimulationSettings();
		settings.Task = task;
		SaveSimulationSettings(settings);

		return task;
	}

	public SimulationSettings GetSimulationSettings() {
		return EditorStateManager.Load().SimulationSettings;
	}

	private void SaveSimulationSettings(SimulationSettings settings) {
		Settings.SimulationSettings = settings.Encode();
	}

	/// <summary>
	/// Loads the currently stored Evolution settings.
	/// </summary>
	/// <returns>The evolution settings.</returns>
	private SimulationSettings LoadSimulationSettings() {
		
		return SimulationSettings.Decode(Settings.SimulationSettings);
	}

	public NeuralNetworkSettings GetNeuralNetworkSettings() {
		return NeuralNetworkSettingsManager.GetNetworkSettings();
	}
}
