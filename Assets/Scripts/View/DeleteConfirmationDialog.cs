using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace Keiwando.Evolution.UI {

	public class DeleteConfirmationDialog : MonoBehaviour {

		public delegate void Delete(string name);

		private Delete deleteHandler;

		[SerializeField] private Text filenameLabel;

		public void ConfirmDeletionFor(string filename, Delete deleteHandler) {

			filenameLabel.text = filename;
			this.deleteHandler = deleteHandler;

			InputRegistry.shared.Register(InputType.AndroidBack, this, EventHandleMode.ConsumeEvent);
			GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;

			gameObject.SetActive(true);
		}

		public void ConfirmedDeletion() {

			//SimulationSerializer.DeleteSaveFile(filenameLabel.text);
			deleteHandler(filenameLabel.text);

			DeregisterAndroidBack();
			gameObject.SetActive(false);
		}

		public void Cancel() {
			filenameLabel.text = "";

			DeregisterAndroidBack();
			gameObject.SetActive(false);
		}

		private void DeregisterAndroidBack() {
			InputRegistry.shared.Deregister(this);
			GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
		}

		private void OnAndroidBack(AndroidBackButtonGestureRecognizer recognizer) {
			if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
			Cancel();
		}
	}
}