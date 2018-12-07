using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidHelpScreen : MonoBehaviour {

	[SerializeField] 
	private GameObject stupidHelpCanvas;
	[SerializeField]
	private HelpScreen helpScreen;

	private const string FIRST_TIME_KEY = "FIRST_TIME";


	void Start () {

		// Apparently only Android users are for some reason incapable of finding
		// the help page on their own.
		if (Application.platform == RuntimePlatform.Android && PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1) {

			Show();

			PlayerPrefs.SetInt(FIRST_TIME_KEY, 0);
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
