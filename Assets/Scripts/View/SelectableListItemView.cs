using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableListItemView : MonoBehaviour {

	public GameObject selectedIndicator;
	public GameObject defaultIndicator;

	public Text label;
	
	
	public void Refresh(string title, bool selected) {
		label.text = title;
		SetSelected(selected);
	}
	
	public void SetSelected(bool selected) {
		selectedIndicator.SetActive(selected);
		defaultIndicator.SetActive(!selected);
	}
}
