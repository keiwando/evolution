using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface SaveDialogDelegate {
	void DidConfirmSave(SaveDialog dialog, string name);
	bool CanEnterCharacter(SaveDialog dialog, int index, char c);
	void DidChangeValue(SaveDialog dialog, string value);
}

public class SaveDialog : MonoBehaviour {

	public static SaveDialog shared;

	public SaveDialogDelegate Delegate { get; set; }

	[SerializeField] private InputField inputField;

	[SerializeField] private Text errorMessage;

	void Awake() {
		if (shared == null || shared == this) {
			shared = this;
		} else {
			Destroy(this.gameObject);
		}
	}

	public void Show() {
		gameObject.SetActive(true);
		ResetErrors();

		inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) {
			if (Delegate == null || Delegate.CanEnterCharacter(this, charIndex, addedChar)) {
				return addedChar;
			} else {
				return '\0';
			}
		};

		inputField.onValueChanged.AddListener(delegate {
			if (Delegate != null) {
				Delegate.DidChangeValue(this, inputField.text);
			}
		});
	}

	public void Close() {
		Delegate = null;
		gameObject.SetActive(false);
	}

	public void OnSaveClicked() {
		errorMessage.enabled = false;

		if (Delegate != null) {
			Delegate.DidConfirmSave(this, inputField.text);
		}
	}

	public void OnCancelClicked() {
		ResetErrors();
		this.gameObject.SetActive(false);
	}

	public void ShowErrorMessage(string message) {
		errorMessage.text = message;
		errorMessage.enabled = true;
	}

	public void ResetErrors() {
		errorMessage.enabled = false;
	}

}
