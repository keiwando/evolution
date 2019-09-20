using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface SelectableListItemViewDelegate {
	void DidSelect(SelectableListItemView itemView);
}

public class SelectableListItemView : MonoBehaviour {

	public GameObject selectedIndicator;
	public GameObject defaultIndicator;

	public Text label;

	public Button button;

	public SelectableListItemViewDelegate Delegate { get; set; }

	void Start() { 
		button.onClick.AddListener(ButtonWasPressed);
	}
	
	public void Refresh(string title, bool selected) {
		label.text = title;
		SetSelected(selected);
	}
	
	public void SetSelected(bool selected) {
		selectedIndicator.SetActive(selected);
		defaultIndicator.SetActive(!selected);
	}

	public void ButtonWasPressed() {
		if (Delegate != null) {
			Delegate.DidSelect(this);
		}
	}
}
