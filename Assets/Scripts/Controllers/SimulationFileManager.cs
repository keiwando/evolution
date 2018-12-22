using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class SimulationFileManager : MonoBehaviour, FileSelectionViewControllerDelegate {

	[SerializeField]
	private CreatureBuilder creatureBuilder;
	[SerializeField]
	private Evolution evolution;

	[SerializeField]
	private FileSelectionViewController viewController;

	private static readonly Regex RENAME_REGEX = new Regex(".txt|.evol");

	private int selectedIndex = 0;

	public void ShowUI() {
		viewController.Show(this);
	}

	// MARK: - FileSelectionViewControllerDelegate

	public string GetTitle(FileSelectionViewController controller) {
		return "Choose a saved simulation";
	}

	public string GetEmptyMessage(FileSelectionViewController controller) {
		return "You haven't saved any simulations yet";
	}

	public int GetNumberOfItems(FileSelectionViewController controller) {
		return EvolutionSaver.GetEvolutionSaveFilenames().Count;
	}

	public string GetTitleForItemAtIndex(FileSelectionViewController controller,
										 int index) {
		return RENAME_REGEX.Replace(EvolutionSaver.GetEvolutionSaveFilenames()[index], "");
	}
	public int GetIndexOfSelectedItem(FileSelectionViewController controller) {
		return selectedIndex;
	}

	public void ItemSelected(FileSelectionViewController controller, int index) {
		selectedIndex = index;
	}

	public void DidEditTitleAtIndex(FileSelectionViewController controller, int index) { 
		
	}

	public void LoadButtonClicked(FileSelectionViewController controller) {

		var filename = EvolutionSaver.GetEvolutionSaveFilenames()[selectedIndex];
		StartCoroutine(LoadOnNextFrame(filename));
	}

	private IEnumerator LoadOnNextFrame(string filename) {

		yield return new WaitForEndOfFrame();

		EvolutionSaver.LoadSimulationFromSaveFile(filename, creatureBuilder, evolution);
	}

	public void ImportButtonClicked(FileSelectionViewController controller) { 
	
	}

	public void ExportButtonClicked(FileSelectionViewController controller) { 
	
	}

	public void DeleteButtonClicked(FileSelectionViewController controller) {
		var filename = EvolutionSaver.GetEvolutionSaveFilenames()[selectedIndex];
		EvolutionSaver.DeleteSaveFile(filename);
		selectedIndex = 0;
	}

	public void DidClose(FileSelectionViewController controller) {
		Reset();
	}

	private void Reset() {
		selectedIndex = 0;
	}
}