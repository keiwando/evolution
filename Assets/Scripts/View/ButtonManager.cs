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

	private Dictionary<SelectableButton, CreatureEditor.Tool> buttonMap = 
		new Dictionary<SelectableButton, CreatureEditor.Tool>();

	void Start() {

		buttonMap.Add(jointButton, CreatureEditor.Tool.Joint);
		buttonMap.Add(boneButton, CreatureEditor.Tool.Bone);
		buttonMap.Add(muscleButton, CreatureEditor.Tool.Muscle);
		buttonMap.Add(deleteButton, CreatureEditor.Tool.Delete);
		buttonMap.Add(moveButton, CreatureEditor.Tool.Move);
		buttonMap.Add(selectButton, CreatureEditor.Tool.Select);

		foreach (SelectableButton button in buttonMap.Keys) {
			button.manager = this;
		}

		Refresh();
	}

	public void Refresh() {

		foreach (KeyValuePair<SelectableButton, CreatureEditor.Tool> entry in buttonMap) {
			if (entry.Value == editor.SelectedTool) {
				entry.Key.Selected = true;
			} else {
				entry.Key.Selected = false;
			}
		}
	}

	public void SelectButton(SelectableButton button) {

		editor.SelectedTool = buttonMap[button];
		Refresh();
	}
}

