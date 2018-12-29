using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface RenameDialogDelegate {
	void DidConfirmRename(RenameDialog dialog, string newName);
	bool CanEnterCharacter(RenameDialog dialog, int index, char c);
	void DidChangeValue(RenameDialog dialog, string value);
	string GetOriginalName(RenameDialog dialog);
}

public class RenameDialog : MonoBehaviour {

	public static RenameDialog shared;

	public RenameDialogDelegate Delegate { get; set; }

	[SerializeField]
	private InputField inputField;
	[SerializeField]
	private Text errorMessage;

	void Awake() {
		if (shared == null || shared == this) {
			shared = this;
		} else {
			Destroy(this.gameObject);
		}
	}

	public void Show(RenameDialogDelegate Delegate) {
		gameObject.SetActive(true);
		this.Delegate = Delegate;
		ResetErrors();
		KeyInputManager.shared.Register();

		inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) {
			if (Delegate.CanEnterCharacter(this, charIndex, addedChar)) {
				return addedChar;
			} else {
				return '\0';
			}
		};

		inputField.onValueChanged.AddListener(delegate {
			Delegate.DidChangeValue(this, inputField.text);
		});

		inputField.text = Delegate.GetOriginalName(this);
	}

	public void Close() {
		Delegate = null;
		KeyInputManager.shared.Deregister();
		gameObject.SetActive(false);
	}

	public void OnRenameClicked() {
		errorMessage.enabled = false;

		Delegate.DidConfirmRename(this, inputField.text);
		Close();
	}

	public void OnCancelClicked() {
		ResetErrors();
		Close();
	}

	public void ShowErrorMessage(string message) {
		errorMessage.text = message;
		errorMessage.enabled = true;
	}

	public void ResetErrors() {
		errorMessage.enabled = false;
	}
}
