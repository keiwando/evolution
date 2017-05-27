using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationLoadDialog : MonoBehaviour {

	[SerializeField] private CreatureBuilder creatureBuilder;
	[SerializeField] private Evolution evolution;

	[SerializeField] private Dropdown dropdown;

	[SerializeField] private GameObject bugFixEmpty;

	private const string NO_SAVE_FILES = "You haven't saved any simulations yet";

	private bool saveFilesExist = false;

	// Use this for initialization
	void Start () {
		/*this.gameObject.SetActive(true);
		this.gameObject.SetActive(false);
		this.gameObject.SetActive(true);*/
	}
	
	// Update is called once per frame
	void Update () {

		/*if (bugFixEmpty.activeSelf) {

			if (saveFilesExist) {
				//dropdown.Show();
			} else {
				dropdown.Hide();
			}
		}*/
	}

	public void PromptDialog() {
		//this.gameObject.SetActive(true);
		bugFixEmpty.SetActive(true);
		SetupDropDown();
	}

	public void OnCancelClicked() {
		//this.gameObject.SetActive(false);
		bugFixEmpty.SetActive(false);
	}

	public void OnLoadClicked() {

		var filename = dropdown.options[dropdown.value].text;

		if (filename == NO_SAVE_FILES) return;

		filename += ".txt";

		EvolutionSaver.LoadSimulationFromSaveFile(filename, creatureBuilder, evolution);
	}

	private void SetupDropDown() {

		var filenames = EvolutionSaver.GetEvolutionSaveFilenames();

		var saveFiles = new List<string>();

		if (filenames.Count == 0) {
			saveFiles.Add(NO_SAVE_FILES);
		} else {
			saveFilesExist = true;
		}

		foreach (var name in filenames) {
			saveFiles.Add(name.Replace(".txt",""));
		}

		dropdown.ClearOptions();
		dropdown.AddOptions(saveFiles);
	}

}
