using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

	public class HelpViewController : MonoBehaviour {

		private static readonly string[] languages = new [] {
			"en", "ru", "pt", "de"
		};

		[SerializeField] private Dropdown languageSelectionUIDropdown;
		[SerializeField] private Keiwando.UI.Dropdown<string> languageSelectionDropdown;

		[SerializeField] private HelpPagesView helpPagesViewTemplate;
		private HelpPagesView[] helpPagesViews;

		private HelpPages helpPages;

		void Start () {

			var currentLanguage = Settings.HelpScreenLanguage;
			LanguageSelected(currentLanguage);
		}


		private void LanguageSelected(string language) {

			Settings.HelpScreenLanguage = language;

		}


		public void BackButtonClicked() {
			this.gameObject.SetActive(false);
		}

		public void HelpButtonClicked() {
			this.gameObject.SetActive(true);
		}
	}

}