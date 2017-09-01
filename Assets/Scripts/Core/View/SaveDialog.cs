using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SaveDialog : MonoBehaviour {

	[SerializeField] private InputField InputField;
	[SerializeField] private CreatureBuilder CreatureBuilder;

	[SerializeField] private Text ErrorMessage;

	// Use this for initialization
	void Start () {
		ResetErrors();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnSaveClicked() {
		ErrorMessage.enabled = false;
		CreatureBuilder.SaveCreature(InputField.text);
	}

	public void OnCancelClicked() {
		ResetErrors();
		this.gameObject.SetActive(false);
	}

	public void ShowErrorMessage(string message) {
		ErrorMessage.text = message;
		ErrorMessage.enabled = true;
	}

	public void ResetErrors() {
		ErrorMessage.enabled = false;
	}

}
