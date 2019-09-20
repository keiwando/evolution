using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public class CreditsViewController: MonoBehaviour {

        void Start() {

            var androidBackButton = GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer();
            androidBackButton.OnGesture += delegate (AndroidBackButtonGestureRecognizer recognizer) {
                if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
                    Close();
            };
        }

        public void Show() {
            this.gameObject.SetActive(true);
            InputRegistry.shared.Register(InputType.AndroidBack, this);
        }

        public void Close() {
            InputRegistry.shared.Deregister(this);
            this.gameObject.SetActive(false);
        }
    }
}