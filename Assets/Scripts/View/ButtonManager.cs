using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

	[SerializeField]	
	private CreatureBuilder creatureBuilder;

	[SerializeField] private SelectableButton jointButton;
	[SerializeField] private SelectableButton boneButton;
	[SerializeField] private SelectableButton muscleButton;
	[SerializeField] private SelectableButton moveButton;
	[SerializeField] private SelectableButton deleteButton;

	private Dictionary<SelectableButton, CreatureBuilder.BuildSelection> buttonMap;
	private SelectableButton selectedButton;

	void Start() {

		buttonMap = new Dictionary<SelectableButton, CreatureBuilder.BuildSelection>();

		buttonMap.Add(jointButton, CreatureBuilder.BuildSelection.Joint);
		buttonMap.Add(boneButton, CreatureBuilder.BuildSelection.Bone);
		buttonMap.Add(muscleButton, CreatureBuilder.BuildSelection.Muscle);
		buttonMap.Add(deleteButton, CreatureBuilder.BuildSelection.Delete);
		buttonMap.Add(moveButton, CreatureBuilder.BuildSelection.Move);

		foreach (SelectableButton button in buttonMap.Keys) {
			button.manager = this;
		}
	}

	public void SelectButton(SelectableButton button) {

		if (!button.Equals(selectedButton)) {
			
			button.Selected = true;
			if (selectedButton != null) {
				selectedButton.Selected = false;
			}
			selectedButton = button;

			creatureBuilder.SelectedPart = buttonMap[button];
		}
	}

	public void SelectButton(CreatureBuilder.BuildSelection part) {
		
		foreach ( SelectableButton button in buttonMap.Keys) {
			
			if (buttonMap[button].Equals(part)) {
				SelectButton(button);
				break;
			}
		}
	}
}
