using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour {

	private const float deselectedAlpha = 0.27f;

	private Image image;
	private Color selectedColor;
	private Color deselectedColor;

	void Start () {

		image = GetComponent<Image>();
		deselectedColor = selectedColor = image.color;
		deselectedColor.a = deselectedAlpha;
	}

	public void Selected() {
		image.color = selectedColor;
	}

	public void Deselected() {
		image.color = deselectedColor;
	}
}
