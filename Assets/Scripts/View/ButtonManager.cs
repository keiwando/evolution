using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

	[SerializeField]	
	private CreatureEditor editor;

	[SerializeField] private SelectableButton jointButton;
	[SerializeField] private SelectableButton boneButton;
	[SerializeField] private SelectableButton muscleButton;
	[SerializeField] private SelectableButton moveButton;
	[SerializeField] private SelectableButton selectButton;
	[SerializeField] private SelectableButton deleteButton;

	private Dictionary<SelectableButton, CreatureEditor.Tool> buttonMap;
	private SelectableButton selectedButton;

	void Start() {

		buttonMap = new Dictionary<SelectableButton, CreatureEditor.Tool>();

		buttonMap.Add(jointButton, CreatureEditor.Tool.Joint);
		buttonMap.Add(boneButton, CreatureEditor.Tool.Bone);
		buttonMap.Add(muscleButton, CreatureEditor.Tool.Muscle);
		buttonMap.Add(deleteButton, CreatureEditor.Tool.Delete);
		buttonMap.Add(moveButton, CreatureEditor.Tool.Move);
		buttonMap.Add(selectButton, CreatureEditor.Tool.Select);

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

			editor.ActiveTool = buttonMap[button];
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
