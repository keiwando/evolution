using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void BackButtonClicked() {
		this.gameObject.SetActive(false);
	}

	public void HelpButtonClicked() {
		this.gameObject.SetActive(true);
	}
}
