using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution.UI;

public class StupidHelpScreen : MonoBehaviour {

	[SerializeField] 
	private GameObject stupidHelpCanvas;
	[SerializeField]
	private HelpViewController helpScreen;

	void Start () {

		// Apparently only Android users are for some reason incapable of finding
		// the help page on their own.
		if (Application.platform == RuntimePlatform.Android && !Settings.HelpIndicatorShown) {

			Show();
			Settings.HelpIndicatorShown = true;
		}
	}

	private void Show() {

		stupidHelpCanvas.gameObject.SetActive(true);
	}

	public void Hide() {

		stupidHelpCanvas.gameObject.SetActive(false);
	}

	public void GoToHelp() {

		Hide();
		helpScreen.HelpButtonClicked();
	}
}
