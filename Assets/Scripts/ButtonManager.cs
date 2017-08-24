using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

	// MARK: PlayerPref keys

	private const string POPULATION_COUNT_KEY = "POPULATION_COUNT";
	private const string GENERATION_TIME_KEY = "GENERATION_TIME";
	private const string TASK_KEY = "EVOLUTION_TASK";

	private static string[] TASK_OPTIONS = new string[] {"RUNNING", "JUMPING", "OBSTACLE JUMP", "CLIMBING"};

	private static int DEFAULT_POPULATION_COUNT = 10;
	private static int IOS_DEFAULT_POPULATION_COUNT = 5;

	private float CAMERA_MAX_X;
	private float CAMERA_MIN_X;
	private float CAMERA_MAX_Y;
	private float CAMERA_MIN_Y;

	private const float CAMERA_DX = 12f;
	private const float CAMERA_DY = 7f;

	[SerializeField] private Camera buildingCamera;

	[SerializeField] private InputField generationNumberInput;
	[SerializeField] private InputField generationTimeInput;

	public SelectableButton jointButton;
	public SelectableButton boneButton;
	public SelectableButton muscleButton;

	public SelectableButton moveButton;
	public SelectableButton deleteButton;

	public SelectableButton selectedButton;

	public Button creatureDeleteButton;
	public DeleteConfirmationDialog deleteConfirmation;

	public CreatureBuilder creatureBuilder;

	public Dropdown dropDown;

	public Dropdown taskDropDown;

	private Dictionary<SelectableButton, CreatureBuilder.BuildSelection> buttonMap;



	// Use this for initialization
	void Start () {

		buttonMap = new Dictionary<SelectableButton, CreatureBuilder.BuildSelection>();

		buttonMap.Add(jointButton, CreatureBuilder.BuildSelection.Joint);
		buttonMap.Add(boneButton, CreatureBuilder.BuildSelection.Bone);
		buttonMap.Add(muscleButton, CreatureBuilder.BuildSelection.Muscle);
		buttonMap.Add(deleteButton, CreatureBuilder.BuildSelection.Delete);
		buttonMap.Add(moveButton, CreatureBuilder.BuildSelection.Move);

		selectedButton.Selected = true;

		foreach (SelectableButton button in buttonMap.Keys) {
			button.manager = this;
		}

		var cameraPos = buildingCamera.transform.position;

		CAMERA_MIN_X = cameraPos.x - CAMERA_DX;
		CAMERA_MAX_X = cameraPos.x + CAMERA_DX;
		CAMERA_MIN_Y = cameraPos.y - CAMERA_DY;
		CAMERA_MAX_Y = cameraPos.y + CAMERA_DY;

		//SetupInputFields();
		//SetupTaskDropDown();
		//SetupDropDown();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Refresh() {
		SetupDropDown();
	}

	/// <summary>
	/// Sets up the dropdown list with the names of the creatures that can be loaded.
	/// </summary>
	public void SetupDropDown() {
		dropDown.ClearOptions();

		//dropDown.AddOptions(CreatureSaver.GetCreatureNames());
		dropDown.AddOptions(CreateDropDownOptions());
	}

	/// <summary>
	/// Sets the Dropdown to the given value (name).
	/// </summary>
	public void SetDropDownToValue(string name) {

		var index = CreateDropDownOptions().IndexOf(name);
		dropDown.value = index;
	}

	public static List<string> CreateDropDownOptions() {
		var options = new List<string>();

		options.Add("Creature");
		options.AddRange(CreatureSaver.GetCreatureNames());

		return options;
	}

	public void CreatureDropdownValueChanged(Int32 index) {

		var options = CreateDropDownOptions();
		var customCreatureSelected = options[index].ToUpper() != "CREATURE";

		creatureDeleteButton.gameObject.SetActive(customCreatureSelected);
	}

	/*private void SetupTaskDropDown() {

		var taskString = PlayerPrefs.GetString(TASK_KEY, "RUNNING");
		var index = new List<string>(TASK_OPTIONS).IndexOf(taskString);

		taskDropDown.value = index;
	}

	private void SetupInputFields() {

		generationNumberInput.onEndEdit.AddListener(delegate {
			PopulationSizeChanged();
		});

		generationTimeInput.onEndEdit.AddListener(delegate {
			SimulationTimeChanged();
		});

		SetupDefaultNumbers();
	}

	private void SetupDefaultNumbers() {

		generationNumberInput.text = PlayerPrefs.GetInt(POPULATION_COUNT_KEY, DEFAULT_POPULATION_COUNT).ToString();

		generationTimeInput.text = PlayerPrefs.GetInt(GENERATION_TIME_KEY, 10).ToString();
		//generationTimeInput.text = "10";
	}

	public int GetPopulationInput() {

		var num = Mathf.Clamp(Int32.Parse(generationNumberInput.text), 2, 10000000);
		PlayerPrefs.SetInt(POPULATION_COUNT_KEY, num);

		return num;
	}

	public int GetSimulationTime() {

		var time = Mathf.Clamp(Int32.Parse(generationTimeInput.text), 1, 100000);
		PlayerPrefs.SetInt(GENERATION_TIME_KEY, time);

		return time;
	}

	public void PopulationSizeChanged() {
		// Make sure the size is at least 2
		var num = Mathf.Clamp(Int32.Parse(generationNumberInput.text), 2, 10000000);
		generationNumberInput.text = num.ToString();
	}

	public void SimulationTimeChanged() {
		// Make sure the time is at least 1
		var time = Mathf.Clamp(Int32.Parse(generationTimeInput.text), 1, 100000);
		generationTimeInput.text = time.ToString();
	}*/

	public void ShowCreatureDeleteButton() {
		creatureDeleteButton.gameObject.SetActive(true);
	}

	public void HideCreatureDeleteButton() {
		creatureDeleteButton.gameObject.SetActive(false);
	}

	public void DeleteCurrentCreatureSave() {

		var selectedCreatureIndex = dropDown.value;
		var options = CreateDropDownOptions();

		var currentCreatureName = options[selectedCreatureIndex];

		if (currentCreatureName.ToUpper() != "CREATURE") {
			deleteConfirmation.ConfirmDeletionFor(currentCreatureName, delegate(string name) {
			
				CreatureSaver.DeleteCreatureSave(currentCreatureName);

				creatureBuilder.DeleteCreature();
				SetupDropDown();
				dropDown.value = 0;
			});
		}
	}

	public void selectButton(SelectableButton button) {

		if (!button.Equals(selectedButton)) {
			
			button.Selected = true;
			selectedButton.Selected = false;
			selectedButton = button;

			creatureBuilder.SelectedPart = buttonMap[button];
		}
	}

	public void selectButton(CreatureBuilder.BuildSelection part) {
		
		foreach ( SelectableButton button in buttonMap.Keys) {
			
			if (buttonMap[button].Equals(part)) {
				selectButton(button);
				break;
			}
		}
	}

	/*
	/// <summary>
	/// Returns the chosen task based on the value of the taskDropDown;
	/// </summary>
	public Evolution.Task GetTask() {

		var taskString = taskDropDown.captionText.text.ToUpper();
		PlayerPrefs.SetString(TASK_KEY, taskString);

		return Evolution.TaskFromString(taskString);
	}*/

	public void MoveCamera(Vector3 distance) {

		distance.z = 0;
		var position = buildingCamera.transform.position + distance;

		position.x = Mathf.Clamp(position.x, CAMERA_MIN_X, CAMERA_MAX_X);
		position.y = Mathf.Clamp(position.y, CAMERA_MIN_Y, CAMERA_MAX_Y);

		buildingCamera.gameObject.transform.position = position;
	}

	//
}
