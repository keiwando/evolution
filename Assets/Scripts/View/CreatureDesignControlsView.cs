using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureDesignControlsView : MonoBehaviour {

	[SerializeField]
	private Text currentCreatureNameLabel;

	public void SetCurrentCreatureName(string name) {
		currentCreatureNameLabel.text = name;
	}

	public void SetUnnamed() {
		currentCreatureNameLabel.text = "Unnamed";
	}
}
