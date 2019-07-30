using System.Collections.Generic;
using UnityEngine;

namespace Keiwando {

    public class GestureRecognizerCollection: MonoBehaviour {

        public static GestureRecognizerCollection shared { get; private set; }

        private ScrollGestureRecognizer scrollGestureRecognizer;
        private Dictionary<int, DragGestureRecognizer> dragGestureRecognizers = new Dictionary<int, DragGestureRecognizer>();
        private PinchGestureRecognizer pinchGestureRecognizer;
        private AndroidBackButtonGestureRecognizer androidBackButtonGestureRecognizer;

        public ScrollGestureRecognizer GetScrollGestureRecognizer() {
            if (scrollGestureRecognizer == null)
                scrollGestureRecognizer = gameObject.AddComponent<ScrollGestureRecognizer>();
            return scrollGestureRecognizer;
        }

        public DragGestureRecognizer GetDragGestureRecognizer(int mouseButton = 0) {
            if (!dragGestureRecognizers.ContainsKey(mouseButton)) {
                var recognizer = gameObject.AddComponent<DragGestureRecognizer>();
                recognizer.MouseButton = mouseButton;
                dragGestureRecognizers[mouseButton] = recognizer;
            }
            return dragGestureRecognizers[mouseButton];
        }

        public PinchGestureRecognizer GetPinchGestureRecognizer() {
            if (pinchGestureRecognizer == null)
                pinchGestureRecognizer = gameObject.AddComponent<PinchGestureRecognizer>();
            return pinchGestureRecognizer;
        }

        public AndroidBackButtonGestureRecognizer GetAndroidBackButtonGestureRecognizer() {
            if (androidBackButtonGestureRecognizer == null)
                androidBackButtonGestureRecognizer = gameObject.AddComponent<AndroidBackButtonGestureRecognizer>();
            return androidBackButtonGestureRecognizer;
        }

        void Awake() {
            if (shared == null) {
                shared = this;
                DontDestroyOnLoad(this.gameObject);
            } else {
                Destroy(this.gameObject);
            }
        }
    }
}