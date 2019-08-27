using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

	public class SimulationLoadDialog : MonoBehaviour {

		public bool IsShowing { 
			get { return bugFixEmpty.activeSelf; }
		}

		[SerializeField] 
		private CreatureEditor editor;

		[SerializeField] 
		private Dropdown dropdown;

		[SerializeField] 
		private GameObject bugFixEmpty;

		[SerializeField] 
		private DeleteConfirmationDialog deleteConfirmation;

		private const string NO_SAVE_FILES = "You haven't saved any simulations yet";


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

			//SimulationSerializer.LoadSimulationFromSaveFile(filename, creatureBuilder, evolution);
			StartCoroutine(LoadOnNextFrame(filename));
		}

		private IEnumerator LoadOnNextFrame(string filename) {

			yield return new WaitForEndOfFrame();

			var simulationData = SimulationSerializer.LoadSimulationData(filename);
			editor.StartSimulation(simulationData);
		}

		private void SetupDropDown() {

			var filenames = SimulationSerializer.GetEvolutionSaveFilenames();

			var saveFiles = new List<string>();

			if (filenames.Count == 0) {
				saveFiles.Add(NO_SAVE_FILES);
			} else {
				//saveFilesExists = true;
			}

			foreach (var name in filenames) {
				saveFiles.Add(name.Replace(".txt", ""));
			}

			dropdown.ClearOptions();
			dropdown.AddOptions(saveFiles);

			if (filenames.Count > 0) {
				dropdown.Show();
			}
		}

		public void PromptSavefileDelete() {

			var filename = dropdown.options[dropdown.value].text;

			if (filename == NO_SAVE_FILES) return;

			filename += ".txt";

			//deleteConfirmation.ConfirmDeletionFor(filename);
			deleteConfirmation.ConfirmDeletionFor(filename, delegate(string name) {

				SimulationSerializer.DeleteSaveFile(filename);

				SetupDropDown();
				dropdown.value = 0;
			});
		}

	}
}
