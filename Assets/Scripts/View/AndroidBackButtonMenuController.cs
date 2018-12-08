using UnityEngine;
using System.Collections;

public class AndroidBackButtonMenuController : MonoBehaviour {

	[SerializeField]
	private SaveDialog saveDialog;
	[SerializeField]
	private SimulationLoadDialog loadDialog;
	[SerializeField]
	private SettingsMenu settingsMenu;

#if PLATFORM_ANDROID
	void Update() {

		if (Input.GetKeyDown(KeyCode.Escape)) {

			if (settingsMenu.IsShowing) {
				if (settingsMenu.ScrollPos == AutoScroll.ScrollPos.Top) {
					settingsMenu.Hide();
				} else {
					settingsMenu.GoToTop();
				}
			} else if (saveDialog.gameObject.activeSelf) {
				saveDialog.OnCancelClicked();
			} else if (loadDialog.IsShowing) {
				loadDialog.OnCancelClicked();
			} else {
				Application.Quit();
			}
		}
	}
#endif
}
