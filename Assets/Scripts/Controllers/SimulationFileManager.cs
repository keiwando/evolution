using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Keiwando.NativeFileSO;

public class SimulationFileManager : MonoBehaviour, FileSelectionViewControllerDelegate {

	[SerializeField]
	private CreatureBuilder creatureBuilder;
	[SerializeField]
	private Evolution evolution;

	[SerializeField]
	private FileSelectionViewController viewController;

	private int selectedIndex = 0;
	private List<string> filenames;

	void Start() {
		NativeFileSOMobile.shared.FilesWereOpened += delegate (OpenedFile[] files) {
			foreach (var file in files) { 
				var extension = file.Extension.ToLower();
				if (extension.Equals(".evol")) {
					// TODO: Validate file contents
					SimulationSerializer.SaveSimulationFile(file.Name, file.ToUTF8String());
				}
			}
			viewController.Refresh();
		};
	}

	public void ShowUI() {
		RefreshCache();
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
		return filenames.Count;
	}

	public string GetTitleForItemAtIndex(FileSelectionViewController controller,
										 int index) {
		return filenames[index];
	}
	public int GetIndexOfSelectedItem(FileSelectionViewController controller) {
		return selectedIndex;
	}

	public void DidSelectItem(FileSelectionViewController controller, int index) {
		selectedIndex = index;
	}

	public bool IsCharacterValidForName(FileSelectionViewController controller, char c) {
		return !SimulationSerializer.INVALID_NAME_CHARACTERS.Contains(c);
	}

	public bool IsNameAvailable(FileSelectionViewController controller, string newName) {
		return !SimulationSerializer.SimulationSaveExists(newName);
	}

	public void DidEditTitleAtIndex(FileSelectionViewController controller, int index, string newName) {
		
		if (!IsNameAvailable(controller, newName)) return;

		var currentName = filenames[index];
		SimulationSerializer.RenameSimulationSave(currentName, newName);
		RefreshCache();
	}

	public void LoadButtonClicked(FileSelectionViewController controller) {

		var filename = filenames[selectedIndex];
		StartCoroutine(LoadOnNextFrame(filename));
	}

	private IEnumerator LoadOnNextFrame(string filename) {

		yield return new WaitForEndOfFrame();

		SimulationSerializer.LoadSimulationFromSaveFile(filename, creatureBuilder, evolution);
	}

	public void ImportButtonClicked(FileSelectionViewController controller) {

		SupportedFileType[] supportedFileTypes = {
			SupportedFileType.PlainText,
			CustomEvolutionFileType.evol
		};

		NativeFileSO.shared.OpenFiles(supportedFileTypes,
		  delegate (bool filesWereOpened, OpenedFile[] files) { 
			if (filesWereOpened) {
				foreach (OpenedFile file in files) {
					SimulationSerializer.SaveSimulationFile(file.Name, file.ToUTF8String());	
					RefreshCache();
					viewController.Refresh();
				}
			}
		});
	}

	public void ExportButtonClicked(FileSelectionViewController controller) {

		var name = filenames[selectedIndex];
		string path = SimulationSerializer.PathToSimulationSave(name);

		FileToSave file = new FileToSave(path, SupportedFileType.PlainText);

		NativeFileSO.shared.SaveFile(file);
	}

	public void DeleteButtonClicked(FileSelectionViewController controller) {
		var filename = filenames[selectedIndex];
		SimulationSerializer.DeleteSaveFile(filename);
		selectedIndex = 0;
		RefreshCache();
	}

	public void DidClose(FileSelectionViewController controller) {
		Reset();
	}

	private void Reset() {
		selectedIndex = 0;
		RefreshCache();
	}

	private void RefreshCache() {
		filenames = SimulationSerializer.GetEvolutionSaveFilenames();
	}
}