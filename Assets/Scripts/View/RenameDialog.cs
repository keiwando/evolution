using UnityEngine;
using UnityEngine.UI;
using Keiwando;

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

	[SerializeField]
	private Button confirmButton;
	private CanvasGroup confirmCanvasGroup;

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

		this.confirmCanvasGroup = confirmButton.GetComponentInChildren<CanvasGroup>();
		KeyInputManager.shared.Register();
		InputRegistry.shared.Register(InputType.All, delegate (InputType types) {});

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

		confirmButton.onClick.AddListener(delegate () {
			OnRenameClicked();
		});

		ResetErrors();
	}

	public void Close() {
		Delegate = null;
		KeyInputManager.shared.Deregister();
		InputRegistry.shared.Deregister();
		inputField.onValidateInput = null;
		inputField.onValueChanged.RemoveAllListeners();
		confirmButton.onClick.RemoveAllListeners();
		gameObject.SetActive(false);
	}

	private void OnRenameClicked() {
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
		confirmCanvasGroup.alpha = 0.2f;
		confirmButton.enabled = false;
	}

	public void ResetErrors() {
		errorMessage.enabled = false;
		confirmCanvasGroup.alpha = 1f;
		confirmButton.enabled = true;
	}
}
