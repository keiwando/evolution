using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpScreen : MonoBehaviour {

	private const string LANGUAGE_ENGLISH = "LANGUAGE_ENGLISH";
	private const string LANGUAGE_GERMAN = "LANGUAGE_GERMAN";
	private const string LANGUAGE_RUSSIAN = "LANGUAGE_RUSSIAN";
	private const string LANGUAGE_PORTUGUESE = "LANGUAGE_PORTUGUESE";

	private Dictionary<string, RectTransform> languageMap;
	private Dictionary<string, LanguageButton> buttonMap;

	public ScrollRect scrollRect;

	public RectTransform englishTextRect;
	public RectTransform germanTextRect;
	public RectTransform russianTextRect;
	public RectTransform portugueseTextRect;

	public LanguageButton englishButton;
	public LanguageButton germanButton;
	public LanguageButton russianButton;
	public LanguageButton portugueseButton;

	void Start () {
		this.gameObject.SetActive(false);
		this.gameObject.SetActive(true);

		languageMap = new Dictionary<string, RectTransform>() {
			{ LANGUAGE_ENGLISH, englishTextRect },
			{ LANGUAGE_GERMAN, germanTextRect },
			{ LANGUAGE_RUSSIAN, russianTextRect },
			{ LANGUAGE_PORTUGUESE, portugueseTextRect }
		};

		buttonMap = new Dictionary<string, LanguageButton>() {
			{ LANGUAGE_ENGLISH, englishButton },
			{ LANGUAGE_GERMAN, germanButton },
			{ LANGUAGE_RUSSIAN, russianButton },
			{ LANGUAGE_PORTUGUESE, portugueseButton }
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
