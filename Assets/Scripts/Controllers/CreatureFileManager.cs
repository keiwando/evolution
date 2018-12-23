using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Keiwando.NativeFileSO;

public class CreatureFileManager : MonoBehaviour, FileSelectionViewControllerDelegate {

	[SerializeField]
	private CreatureBuilder creatureBuilder;

	[SerializeField]
	private FileSelectionViewController viewController;

	private int selectedIndex = 0;
	private List<string> creatureNames = new List<string>();

	void Start() {
		NativeFileSOMobile.shared.FilesWereOpened += delegate (OpenedFile[] files) {
			var evolutionFiles = files.Select(delegate (OpenedFile file) {
				var extension = file.Extension.ToLower();
				return extension.Equals(".creat");
			});
			// TODO: Import the files
		};
	}

	public void ShowUI() {
		RefreshCache();
		viewController.Show(this);
	}

	// MARK: - FileSelectionViewControllerDelegate

	public string GetTitle(FileSelectionViewController controller) {
		return "Creature Designs";
	}

	public string GetEmptyMessage(FileSelectionViewController controller) {
		return "You haven't saved any creature designs yet";
	}

	public int GetNumberOfItems(FileSelectionViewController controller) {
		return creatureNames.Count;
	}

	public string GetTitleForItemAtIndex(FileSelectionViewController controller,
										 int index) {
		return creatureNames[index];
	}
	public int GetIndexOfSelectedItem(FileSelectionViewController controller) {
		return selectedIndex;
	}

	public void DidSelectItem(FileSelectionViewController controller, int index) {
		selectedIndex = index;
	}

	public void DidEditTitleAtIndex(FileSelectionViewController controller, int index) {
		// TODO: Rename file & check if filename is available
		RefreshCache();
	}

	public void LoadButtonClicked(FileSelectionViewController controller) {

		var name = creatureNames[selectedIndex];
		viewController.Close();
		StartCoroutine(LoadOnNextFrame(name));
	}

	private IEnumerator LoadOnNextFrame(string name) {

		yield return new WaitForEndOfFrame();

		creatureBuilder.LoadCreature(name);
	}

	public void ImportButtonClicked(FileSelectionViewController controller) {

		SupportedFileType[] supportedFileTypes = {
			CustomEvolutionFileType.creat
		};

		NativeFileSO.shared.OpenFiles(supportedFileTypes,
		delegate (bool filesWereOpened, OpenedFile[] files) {
			if (filesWereOpened) {
			  	foreach (OpenedFile file in files) {

					CreatureSaver.Save(file.Name.Replace(file.Extension, ""), file.ToUTF8String());
				  	RefreshCache();
				  	viewController.Refresh();
			 	}
			}
	  	});
	}

	public void ExportButtonClicked(FileSelectionViewController controller) {

		var name = creatureNames[selectedIndex];
		string path = CreatureSaver.PrepareForExport(name);

		FileToSave file = new FileToSave(path, CustomEvolutionFileType.creat);

		NativeFileSO.shared.SaveFile(file);
	}

	public void DeleteButtonClicked(FileSelectionViewController controller) {
		var name = creatureNames[selectedIndex];
		CreatureSaver.DeleteCreatureSave(name);
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
		creatureNames = CreatureSaver.GetCreatureNames();
	}
}
