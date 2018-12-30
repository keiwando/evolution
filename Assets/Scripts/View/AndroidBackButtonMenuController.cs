using UnityEngine;
using System.Collections;

public class AndroidBackButtonMenuController : MonoBehaviour {

	[SerializeField]
	private SaveDialog saveDialog;
	[SerializeField]
	private SettingsMenu settingsMenu;
	[SerializeField]
	private RenameDialog renameDialog;
	[SerializeField]
	private FileSelectionViewController fileSelectionView;
	[SerializeField]
	private HelpScreen helpScreen;

#if PLATFORM_ANDROID
	void Update() {

		if (Input.GetKeyDown(KeyCode.Escape)) {
			
			if (helpScreen.gameObject.activeSelf) {
				helpScreen.BackButtonClicked();
			} else if (settingsMenu.IsShowing) {
				if (settingsMenu.ScrollPos == AutoScroll.ScrollPos.Top) {
					settingsMenu.Hide();
				} else {
					settingsMenu.GoToTop();
				}
			} else if (saveDialog.gameObject.activeSelf) {
				saveDialog.OnCancelClicked();
			} else if (renameDialog.gameObject.activeSelf) {
				renameDialog.Close();
			} else if (fileSelectionView.gameObject.activeSelf) {
				fileSelectionView.Close();
			} else {
				Application.Quit();
			}
		}
	}
#endif
}
