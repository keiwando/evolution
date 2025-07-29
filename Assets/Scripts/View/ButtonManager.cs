using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Keiwando.Evolution.UI;
using Keiwando.UI;

public class ButtonManager : MonoBehaviour {

	[SerializeField]	
	private CreatureEditor editor;

	[SerializeField] private SelectableButton jointButton;
	[SerializeField] private SelectableButton boneButton;
	[SerializeField] private SelectableButton muscleButton;
	[SerializeField] private SelectableButton moveButton;
	[SerializeField] private SelectableButton selectButton;
	[SerializeField] private SelectableButton deleteButton;
	[SerializeField] private Button decorationsButton;
	[SerializeField] private DecorationSelectionView decorationSelectionView;
	[SerializeField] private SlidingContainer decorationsSelectionSlidingContainer;
	[SerializeField] private SlidingContainer buttonsSlidingContainer;
	private TMP_Text decorationsButtonLabel;

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

		decorationsButtonLabel = decorationsButton.GetComponentInChildren<TMP_Text>();
		decorationsButton.onClick.AddListener(delegate () {
			if (editor.SelectedTool != CreatureEditor.Tool.Decoration) {
				editor.SelectedTool = CreatureEditor.Tool.Decoration;
			} else {
				editor.SelectedTool = CreatureEditor.Tool.Move;
			}
			Refresh();
		});

		decorationsSelectionSlidingContainer.LastSlideDirection = SlidingContainer.Direction.Left;
		buttonsSlidingContainer.LastSlideDirection = SlidingContainer.Direction.Right;

		editor.onToolChanged += delegate (CreatureEditor.Tool tool) {
			Refresh();
		};
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

		if (editor.SelectedTool == CreatureEditor.Tool.Decoration) {
			decorationsButtonLabel.SetText("Back");
			if (buttonsSlidingContainer.LastSlideDirection == SlidingContainer.Direction.Right) {
				buttonsSlidingContainer.Slide(SlidingContainer.Direction.Left, 0.3f, false);
			}
			if (decorationsSelectionSlidingContainer.LastSlideDirection == SlidingContainer.Direction.Left) {
				decorationsSelectionSlidingContainer.Slide(SlidingContainer.Direction.Right, 0.3f, false);
			}
		} else {
			decorationsButtonLabel.SetText("Skin");
			if (buttonsSlidingContainer.LastSlideDirection == SlidingContainer.Direction.Left) {
				buttonsSlidingContainer.Slide(SlidingContainer.Direction.Right, 0.3f, false);
			}
			if (decorationsSelectionSlidingContainer.LastSlideDirection == SlidingContainer.Direction.Right) {
				decorationsSelectionSlidingContainer.Slide(SlidingContainer.Direction.Left, 0.3f, false);
			}
		}
	}


	public void SelectButton(SelectableButton button) {

		editor.SelectedTool = buttonMap[button];
	}
}

