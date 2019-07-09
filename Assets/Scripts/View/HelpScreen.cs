using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

	public class HelpScreen : MonoBehaviour {

		private const string LANGUAGE_ENGLISH = "LANGUAGE_ENGLISH";
		private const string LANGUAGE_GERMAN = "LANGUAGE_GERMAN";
		private const string LANGUAGE_RUSSIAN = "LANGUAGE_RUSSIAN";
		private const string LANGUAGE_PORTUGUESE = "LANGUAGE_PORTUGUESE";

		[SerializeField] private Dropdown uiDropdown;
		[SerializeField] private Keiwando.UI.Dropdown<string> dropdown;

		public ScrollRect scrollRect;

		public RectTransform englishTextRect;
		public RectTransform germanTextRect;
		public RectTransform russianTextRect;
		public RectTransform portugueseTextRect;

	
		private Dictionary<string, RectTransform> languageMap;

		void Start () {
			// this.gameObject.SetActive(false);
			// this.gameObject.SetActive(true);

			

			languageMap = new Dictionary<string, RectTransform>() {
				{ LANGUAGE_ENGLISH, englishTextRect },
				{ LANGUAGE_GERMAN, germanTextRect },
				{ LANGUAGE_RUSSIAN, russianTextRect },
				{ LANGUAGE_PORTUGUESE, portugueseTextRect }
			};

			var currentLanguage = Settings.HelpScreenLanguage;
			LanguageSelected(currentLanguage);
		}


		public void EnglishButtonClicked() {
			LanguageSelected(LANGUAGE_ENGLISH);
		}

		public void GermanButtonClicked() {
			LanguageSelected(LANGUAGE_GERMAN);
		}

		public void RussianButtonClicked() {
			LanguageSelected(LANGUAGE_RUSSIAN);
		}

		public void PortugueseButtonClicked() {
			LanguageSelected(LANGUAGE_PORTUGUESE);
		}

		private void LanguageSelected(string language) {

			Settings.HelpScreenLanguage = language;

			foreach (var rect in languageMap.Values) {
				rect.gameObject.SetActive (false);
			}
			foreach (var btn in buttonMap.Values) {
				btn.Deselected();
			}

			var languageRect = languageMap[language];
			languageRect.gameObject.SetActive (true);
			scrollRect.content = languageRect;

			var button = buttonMap[language];
			button.Selected();
		}


		public void BackButtonClicked() {
			this.gameObject.SetActive(false);
		}

		public void HelpButtonClicked() {
			this.gameObject.SetActive(true);
		}
	}

}