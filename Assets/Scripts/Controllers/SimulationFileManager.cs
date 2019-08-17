using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Keiwando;
using Keiwando.NativeFileSO;

public class SimulationFileManager : MonoBehaviour, FileSelectionViewControllerDelegate {

	[SerializeField]
	private CreatureEditor editor;

	[SerializeField]
	private FileSelectionViewController viewController;
	[SerializeField]
	private UIFade importIndicator;
	[SerializeField]
	private UIFade failedImportIndicator;

	private int selectedIndex = 0;
	private List<string> filenames;

	void Start() {
		NativeFileSOMobile.shared.FilesWereOpened += delegate (OpenedFile[] files) {
			TryImport(files);
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
		return !FileUtil.INVALID_FILENAME_CHARACTERS.Contains(c);
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

		var simulationData = SimulationSerializer.LoadSimulationData(filename);
		InputRegistry.shared.DeregisterAll();
		editor.StartSimulation(simulationData);
	}

	private void TryImport(OpenedFile[] files) {
		// Whether at least one file was successfully imported
		var successfulImport = false;
		// Whether at least one .evol file failed to be imported
		var failedImport = false;
		foreach (OpenedFile file in files) {

			var extension = file.Extension.ToLower();
			if (extension.Equals(".evol")) {
				var encoded = file.ToUTF8String();
				try {
					var simulationData = SimulationSerializer.ParseSimulationData(encoded, file.Name);
					// SimulationSerializer.SaveSimulation(simulationData);
				} catch {
					failedImport = true;
					Debug.LogError(string.Format("Failed to parse .evol file contents: {0}", encoded));
					continue;
				}
				SimulationSerializer.SaveSimulationFile(file.Name, encoded);
				successfulImport = true;
			}
		}
		RefreshCache();
		try {
			viewController.Refresh();
		} catch {}
		if (successfulImport) {
			importIndicator.FadeInOut();
		} 
		if (failedImport) {
			failedImportIndicator.FadeInOut(1.8f);
		} 
	}

	public void ImportButtonClicked(FileSelectionViewController controller) {

		SupportedFileType[] supportedFileTypes = {
			CustomEvolutionFileType.evol
		};

		NativeFileSO.shared.OpenFiles(supportedFileTypes,
		  delegate (bool filesWereOpened, OpenedFile[] files) { 
			if (filesWereOpened) {
				TryImport(files);
			}
		});
	}

	public void ExportButtonClicked(FileSelectionViewController controller) {

		var name = filenames[selectedIndex];
		string path = SimulationSerializer.PathToSimulationSave(name);

		FileToSave file = new FileToSave(path, CustomEvolutionFileType.evol);

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
		filenames = SimulationSerializer.GetEvolutionSaveFilenames()
			.Select(filename => SimulationSerializer.EXTENSION_PATTERN.Replace(filename, ""))
			.ToList();
	}
}