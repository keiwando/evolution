using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidHelpScreen : MonoBehaviour {

	[SerializeField] private GameObject stupidHelpCanvas;

	private const string FIRST_TIME_KEY = "FIRST_TIME";

	// Use this for initialization
	void Start () {

		if (Application.platform == RuntimePlatform.Android && PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1) {

			Show();

			PlayerPrefs.SetInt(FIRST_TIME_KEY, 0);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void Show() {

		stupidHelpCanvas.gameObject.SetActive(true);
	}

	public void Hide() {

		stupidHelpCanvas.gameObject.SetActive(false);
	}
}
