using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Keiwando.NativeFileSO;
using Keiwando.Evolution.UI;

namespace Keiwando.Evolution {

	public class CreatureFileManager : MonoBehaviour, 
								   FileSelectionViewControllerDelegate, 
								   SaveDialogDelegate {

		[SerializeField]
		private CreatureEditor editor;

		[SerializeField]
		private FileSelectionViewController viewController;
		[SerializeField]
		private SaveDialog saveDialog;
		[SerializeField]
		private UIFade importIndicator;
		[SerializeField]
		private UIFade failedImportIndicator;

		private int selectedIndex = 0;
		private List<string> creatureNames = new List<string>();

		void Start() {
			NativeFileSOMobile.shared.FilesWereOpened += delegate (OpenedFile[] files) {
				TryImport(files);
			};
		}

		public void ShowUI() {
			RefreshCache();
			var currentName = editor.GetCreatureName();
			this.selectedIndex = 0;
			for (int i = 0; i < creatureNames.Count; i++) {
				if (creatureNames[i] == currentName) {
					this.selectedIndex = i;
					break;
				}
			}
			viewController.Show(this);
		}

		public void PromptCreatureSave() {
			saveDialog.Show(this);
		}

		// MARK: - SaveDialogDelegate

		public void DidConfirmSave(SaveDialog dialog, string name) {

			if (string.IsNullOrEmpty(name)) {
				dialog.ShowErrorMessage("Please enter a name for your creature!");
				return;
			}

			try {
				editor.SaveCurrentDesign(name);
			} catch (IllegalFilenameException e) {
				dialog.ShowErrorMessage(string.Format("The creature name cannot contain: {0}", 
				new string(FileUtil.INVALID_FILENAME_CHARACTERS)));
				return;
			}

			dialog.Close();
		}

		public bool CanEnterCharacter(SaveDialog dialog, int index, char c) {
			return !FileUtil.INVALID_FILENAME_CHARACTERS.Contains(c);
		}

		public void DidChangeValue(SaveDialog dialog, string value) {
			if (CreatureSerializer.CreatureExists(value)) {
				dialog.ShowErrorMessage(string.Format("The existing save for {0} will be overwritten!", value));
			} else {
				dialog.ResetErrors();
			}
		}

		public string GetSuggestedName(SaveDialog dialog) {
			return editor.GetCreatureName();
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

		public bool IsCharacterValidForName(FileSelectionViewController controller, char c) {
			return !FileUtil.INVALID_FILENAME_CHARACTERS.Contains(c);
		}

		public bool IsNameAvailable(FileSelectionViewController controller, string newName) {
			return !CreatureSerializer.CreatureExists(newName);
		}

		public void DidEditTitleAtIndex(FileSelectionViewController controller, int index, string newName) {
			
			if (!IsNameAvailable(controller, newName)) return;

			var currentName = creatureNames[index];
			CreatureSerializer.RenameCreatureDesign(currentName, newName);
			RefreshCache();
		}

		public void LoadButtonClicked(FileSelectionViewController controller) {

			var name = creatureNames[selectedIndex];
			viewController.Close();
			StartCoroutine(LoadOnNextFrame(name));
		}

		private IEnumerator LoadOnNextFrame(string name) {

			yield return new WaitForEndOfFrame();

			var design = CreatureSerializer.LoadCreatureDesign(name);
			editor.LoadDesign(design);
		}

		private void CloseAndLoadOnNextFrame(string name) {
			viewController.Close();
			StartCoroutine(LoadOnNextFrame(name));
		}

		private void TryImport(OpenedFile[] files) {
			// Whether at least one file was successfully imported
			var successfulImport = false;
			// Whether at least one .creat file failed to be imported
			var failedImport = false;
			CreatureDesign lastImportedDesign = null;
			foreach (var file in files) {
				var extension = file.Extension.ToLower();
				if (extension.Equals(".creat")) {

					var nameFromFile = CreatureSerializer.EXTENSION_PATTERN.Replace(file.Name, "");
					var encoded = file.ToUTF8String();
					try {
						var design = CreatureSerializer.ParseCreatureDesign(encoded, nameFromFile);
						CreatureSerializer.SaveCreatureDesign(design, false);
						lastImportedDesign = design;
					} catch {
						failedImport = true;
						Debug.LogError(string.Format("Failed to parse .creat file contents: {0}", encoded));
						continue;
					}
					successfulImport = true;
				}
			}
			RefreshCache();
			try {
				viewController.Refresh();
			} catch {}
			if (successfulImport) {
				importIndicator.FadeInOut();
				CloseAndLoadOnNextFrame(lastImportedDesign.Name);
			}
			if (failedImport) {
				failedImportIndicator.FadeInOut(1.8f);
			}
		}

		public void ImportButtonClicked(FileSelectionViewController controller) {

			SupportedFileType[] supportedFileTypes = {
				CustomEvolutionFileType.creat
			};

			NativeFileSO.NativeFileSO.shared.OpenFiles(supportedFileTypes,
			delegate (bool filesWereOpened, OpenedFile[] files) {
				if (filesWereOpened) {
					TryImport(files);
				}
			});
		}

		public void ExportButtonClicked(FileSelectionViewController controller) {

			var name = creatureNames[selectedIndex];
			string path = CreatureSerializer.PathToCreatureDesign(name);

			FileToSave file = new FileToSave(path, CustomEvolutionFileType.creat);

			NativeFileSO.NativeFileSO.shared.SaveFile(file);
		}

		public void DeleteButtonClicked(FileSelectionViewController controller) {
			var name = creatureNames[selectedIndex];
			CreatureSerializer.DeleteCreatureSave(name);
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
			creatureNames = CreatureSerializer.GetCreatureNames();
		}
	}

}