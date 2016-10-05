using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

	private List<SelectableButton> buttons;

	public SelectableButton jointButton;
	public SelectableButton boneButton;
	public SelectableButton muscleButton;

	public SelectableButton selectedButton;

	public CreatureBuilder creatureBuilder;

	// Use this for initialization
	void Start () {

		buttons = new List<SelectableButton>();
		buttons.Add(jointButton);
		buttons.Add(boneButton);
		buttons.Add(muscleButton);

		selectedButton.Selected = true;

		foreach (SelectableButton button in buttons) {
			button.manager = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void selectButton(SelectableButton button) {

		if (!button.Equals(selectedButton)) {
			
			button.Selected = true;
			selectedButton.Selected = false;
			selectedButton = button;

			if (button.Equals(jointButton)) {
				creatureBuilder.SelectedPart = CreatureBuilder.BodyPart.Joint;
			} else if (button.Equals(boneButton)) {
				creatureBuilder.SelectedPart = CreatureBuilder.BodyPart.Bone;
			} else if (button.Equals(muscleButton)) {
				creatureBuilder.SelectedPart = CreatureBuilder.BodyPart.Muscle;
			}
		}
	}

	public void selectButton(CreatureBuilder.BodyPart part) {

		switch(part) {
			
		case CreatureBuilder.BodyPart.Bone: selectButton(boneButton); break;
		case CreatureBuilder.BodyPart.Joint: selectButton(jointButton); break;
		case CreatureBuilder.BodyPart.Muscle: selectButton(muscleButton); break;
		}
	}
}
