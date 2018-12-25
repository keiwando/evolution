using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class DeleteConfirmationDialog : MonoBehaviour {

	public delegate void Delete(string name);

	private Delete deleteHandler;

	[SerializeField] private Text filenameLabel;

	public void ConfirmDeletionFor(string filename, Delete deleteHandler) {

		filenameLabel.text = filename;
		this.deleteHandler = deleteHandler;

		gameObject.SetActive(true);
	}

	public void ConfirmedDeletion() {

		//SimulationSerializer.DeleteSaveFile(filenameLabel.text);
		deleteHandler(filenameLabel.text);

		gameObject.SetActive(false);
	}

	public void Cancel() {
		filenameLabel.text = "";

		gameObject.SetActive(false);
	}
}
