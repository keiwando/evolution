using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

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

	public Dropdown taskDropDown;

	private Dictionary<SelectableButton, CreatureBuilder.BuildSelection> buttonMap;


	void Start() {

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
	}

	public void ShowCreatureDeleteButton() {
		creatureDeleteButton.gameObject.SetActive(true);
	}

	public void HideCreatureDeleteButton() {
		creatureDeleteButton.gameObject.SetActive(false);
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

	public void MoveCamera(Vector3 distance) {

		distance.z = 0;
		var position = buildingCamera.transform.position + distance;

		position.x = Mathf.Clamp(position.x, CAMERA_MIN_X, CAMERA_MAX_X);
		position.y = Mathf.Clamp(position.y, CAMERA_MIN_Y, CAMERA_MAX_Y);

		buildingCamera.gameObject.transform.position = position;
	}
}
