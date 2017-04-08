using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

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

	private void SetupDefaultNumbers() {
		generationNumberInput.text = "10";
		generationTimeInput.text = "10";
	}

	public int GetPopulationInput() {
		return Mathf.Clamp(Int32.Parse(generationNumberInput.text), 2, 10000000);
	}

	public int GetSimulationTime() {
		return Mathf.Clamp(Int32.Parse(generationTimeInput.text), 1, 100000);
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

		switch(taskDropDown.captionText.text.ToUpper()) {

			case "RUNNING": return Evolution.Task.RUNNING; break;
			case "JUMPING": return Evolution.Task.JUMPING; break;
			case "OBSTACLE JUMP": return Evolution.Task.OBSTACLE_JUMP; break;
			case "CLIMBING": return Evolution.Task.CLIMBING; break;

			default: return Evolution.Task.RUNNING;
		}
	}
}
