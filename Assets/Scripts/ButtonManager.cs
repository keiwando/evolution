using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

	public List<SelectableButton> buttons;

	public SelectableButton selectedButton;

	// Use this for initialization
	void Start () {
		selectedButton.selected = true;

		foreach (SelectableButton button in buttons) {
			button.manager = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void selectButton(SelectableButton button) {

		if (!button.Equals(selectedButton)) {
			
			button.selected = true;
			selectedButton.selected = false;
			selectedButton = button;	
		}
	}
}
