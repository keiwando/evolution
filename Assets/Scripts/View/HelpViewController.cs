using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;
using TMPro;

namespace Keiwando.Evolution.UI {

	public class HelpViewController : MonoBehaviour {

		private struct Language {
			public string id;
			public string label;
			public int fontIndex;
			public int flagIndex;
		}

		[SerializeField] TMP_FontAsset[] fonts;
		[SerializeField] Sprite[] flags;

		private readonly Language[] languages = new [] {
			new Language () {
				id = "en",
				label = "English",
				fontIndex = 0,
				flagIndex = 0
			},
			new Language () {
				id = "ru",
				label = "Russian",
				fontIndex = 1,
			 	flagIndex = 1
			},
			new Language () {
				id = "pt",
				label = "Portuguese",
				fontIndex = 2,
			 	flagIndex = 2
			},
			new Language () {
				id = "de",
				label = "German",
				fontIndex = 2,
			 	flagIndex = 3
			}
		};

		[SerializeField] private Dropdown languageSelectionUIDropdown;
		[SerializeField] private Keiwando.UI.Dropdown<string> languageSelectionDropdown;

		[SerializeField] private HelpPagesView helpPagesViewTemplate;
		private Dictionary<string, HelpPagesView> helpPagesViews = new Dictionary<string, HelpPagesView>();

		private HelpPages helpPages;

		void Start () {

			var dropdownData = new List<Dropdown<string>.Data>();
			foreach (var language in languages) {
				dropdownData.Add(new Dropdown<string>.Data() {
					Value = language.id,
					Label = language.label,
					Sprite = flags[language.flagIndex]
				});
			}
			var languageDropdown = new Dropdown<string>(languageSelectionUIDropdown, dropdownData);
			languageDropdown.onValueChanged += delegate (string id) {
				LanguageSelected(id);	
			};
			this.languageSelectionDropdown = languageDropdown;

			helpPagesViewTemplate.gameObject.SetActive(false);
			
			// Create all help page views
			foreach (var language in languages) {
				var view = Instantiate(helpPagesViewTemplate, helpPagesViewTemplate.transform.parent);

				var pages = LoadHelpPagesForLanguage(language, fonts[language.fontIndex]);
				view.gameObject.SetActive(false);
				view.Setup(pages);
				helpPagesViews[language.id] = view;
			}
			
			var currentLanguage = Settings.Language;
			LanguageSelected(currentLanguage);

			for (int i = 0; i < languages.Length; i++) {
				if (languages[i].id == currentLanguage) {
					languageSelectionUIDropdown.value = i;
					break;
				}
			}
		}

		private void LanguageSelected(string language) {

			helpPagesViews[Settings.Language].gameObject.SetActive(false);
			Settings.Language = language;
			helpPagesViews[language].gameObject.SetActive(true);
		}

		public void BackButtonClicked() {
			InputRegistry.shared.Deregister(this);
			GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBackButton;
			this.gameObject.SetActive(false);
		}

		public void HelpButtonClicked() {
			this.gameObject.SetActive(true);
			InputRegistry.shared.Register(InputType.All, this);
			GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBackButton;
		}

		private HelpPages LoadHelpPagesForLanguage(Language lang, TMPro.TMP_FontAsset font) {
			
			var fileContents = Resources.Load<TextAsset>(string.Format("Help/{0}", lang.id)).text;
			var pages = HelpPageParser.Parse(fileContents);

			return new HelpPages () {
				Font = font,
				Pages = pages
			};
		}

		private void OnAndroidBackButton(AndroidBackButtonGestureRecognizer recognizer) {
			if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
				BackButtonClicked();
		}
	}

}