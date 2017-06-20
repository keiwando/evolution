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

	private const float CAMERA_MAX_X = 2;
	private const float CAMERA_MIN_X = -CAMERA_MAX_X;
	private const float CAMERA_MAX_Y = 1;
	private const float CAMERA_MIN_Y = -CAMERA_MAX_Y;

	[Serializable] private Camera buildingCamera;

	[SerializeField] private InputField generationNumberInput;
	[SerializeField] private InputField generationTimeInput;

	public SelectableButton jointButton;
	public SelectableButton boneButton;
	public SelectableButton muscleButton;

	public SelectableButton deleteButton;

	public SelectableButton selectedButton;

	public CreatureBuilder creatureBuilder;

	public Dropdown dropDown;

	public Dropdown taskDropDown;

	private Dictionary<SelectableButton, CreatureBuilder.BodyPart> buttonMap;



	// Use this for initialization
	void Start () {

		buttonMap = new Dictionary<SelectableButton, CreatureBuilder.BodyPart>();

		buttonMap.Add(jointButton, CreatureBuilder.BodyPart.Joint);
		buttonMap.Add(boneButton, CreatureBuilder.BodyPart.Bone);
		buttonMap.Add(muscleButton, CreatureBuilder.BodyPart.Muscle);
		buttonMap.Add(deleteButton, CreatureBuilder.BodyPart.None);

		selectedButton.Selected = true;

		foreach (SelectableButton button in buttonMap.Keys) {
			button.manager = this;
		}

		SetupDefaultNumbers();
		SetupTaskDropDown();
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

	private void SetupTaskDropDown() {

		var taskString = PlayerPrefs.GetString(TASK_KEY, "RUNNING");
		var index = new List<string>(TASK_OPTIONS).IndexOf(taskString);

		taskDropDown.value = index;
	}

	private void SetupDefaultNumbers() {

		/*if (Application.platform == RuntimePlatform.IPhonePlayer) 
			generationNumberInput.text = IOS_DEFAULT_POPULATION_COUNT.ToString();
		else 
			generationNumberInput.text = DEFAULT_POPULATION_COUNT.ToString();
			*/

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


	public void selectButton(SelectableButton button) {

		if (!button.Equals(selectedButton)) {
			
			button.Selected = true;
			selectedButton.Selected = false;
			selectedButton = button;

			creatureBuilder.SelectedPart = buttonMap[button];
		}
	}

	public void selectButton(CreatureBuilder.BodyPart part) {
		
		foreach ( SelectableButton button in buttonMap.Keys) {
			
			if (buttonMap[button].Equals(part)) {
				selectButton(button);
				break;
			}
		}
	}

	/// <summary>
	/// Returns the chosen task based on the value of the taskDropDown;
	/// </summary>
	public Evolution.Task GetTask() {

		var taskString = taskDropDown.captionText.text.ToUpper();
		PlayerPrefs.SetString(TASK_KEY, taskString);

		return Evolution.TaskFromString(taskString);

		/*switch(taskDropDown.captionText.text.ToUpper()) {

			case "RUNNING": return Evolution.Task.RUNNING; break;
			case "JUMPING": return Evolution.Task.JUMPING; break;
			case "OBSTACLE JUMP": return Evolution.Task.OBSTACLE_JUMP; break;
			case "CLIMBING": return Evolution.Task.CLIMBING; break;

			default: return Evolution.Task.RUNNING;
		}*/
	}

	public void MoveCamera(Vector3 distance) {

		var position = buildingCamera.transform.position + distance;

		position.x = Mathf.Clamp(position.x, CAMERA_MIN_X, CAMERA_MAX_X);
		position.y = Mathf.Clamp(position.y, CAMERA_MIN_Y, CAMERA_MAX_Y);

		camera.transform.position = position;
	}

	//
}
