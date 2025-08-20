using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.Evolution.UI;

public class OnboardingScreen : MonoBehaviour {

	[SerializeField] 
	private GameObject canvas;
	[SerializeField]
	private HelpViewController helpScreen;
	[SerializeField]
	private Button skipButton;
	[SerializeField]
	private Button nextButton;
	[SerializeField]
	private Button doneButton;
	[SerializeField]
	private Button helpButton;
	[SerializeField]
	private GameObject evolveButton;

	[SerializeField]
	private Text description;
	[SerializeField]
	private GameObject joints;
	[SerializeField]
	private GameObject bones;
	[SerializeField]
	private GameObject muscles;
	[SerializeField]
	private GameObject wing;

	private int currentScreenIndex = 0;
	private const int totalScreenCount = 6;

	private string[] descriptions = {
		"1. Place joints",
		"2. Connect the joints with bones",
		"3. Connect the bones with muscles",
		"4. Select bones and turn them into wings (Optional)",
		"Click on \"Evolve\" and \nwatch your creature improve\nat the selected task using\nbasic machine learning.",
		"Click on the Question mark\nfor more details\nabout how everything works"
	};

	void Start () {

		if (Settings.ShowOnboarding) {
			Show();
			Settings.ShowOnboarding = false;
		}
	}

	private void Show() {

		canvas.gameObject.SetActive(true);
		currentScreenIndex = 0;
		Refresh();
	}

	public void Hide() {
		canvas.gameObject.SetActive(false);
	}

	private void Refresh() {
		joints.SetActive(currentScreenIndex <= 2);
		bones.SetActive(1 <= currentScreenIndex && currentScreenIndex <= 2);
		muscles.SetActive(currentScreenIndex == 2);
		wing.SetActive(currentScreenIndex == 3);
		description.text = descriptions[currentScreenIndex];
		nextButton.gameObject.SetActive(currentScreenIndex < totalScreenCount - 1);
		helpButton.gameObject.SetActive(currentScreenIndex == 5);
		skipButton.gameObject.SetActive(currentScreenIndex < totalScreenCount - 1);
		doneButton.gameObject.SetActive(currentScreenIndex == totalScreenCount - 1);
		evolveButton.SetActive(currentScreenIndex == totalScreenCount - 2);
	}

	public void GoToNextScreen() {
		currentScreenIndex = System.Math.Min(currentScreenIndex + 1, totalScreenCount - 1);
		Refresh();
	}

	public void GoToHelp() {
		Hide();
		helpScreen.HelpButtonClicked();
	}
}
