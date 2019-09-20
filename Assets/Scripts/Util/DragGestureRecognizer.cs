using UnityEngine;

namespace Keiwando {

    public class DragGestureRecognizer: MonoBehaviour, IGestureRecognizer<DragGestureRecognizer> {

        public event GestureCallback<DragGestureRecognizer> OnGesture;

        public GestureRecognizerState State { get; private set; } = GestureRecognizerState.Ended;

        /// <summary>
        /// The current screen position of the drag (touch or mouse).
        /// </summary>
        public Vector3 DragPosition { get; private set; } = Vector3.zero;

        /// <summary>
        /// The drag distance between the current and the previous frame in screen coordinates.
        /// </summary>
        public Vector3 DragDelta { get; private set; } = Vector3.zero;

        /// <summary>
        /// The mouse button used to check for a drag gesture.
        /// </summary>
        public int MouseButton { get; set; } = 0;

        /// <summary>
        /// The screen coordinate at the start of the drag gesture.
        /// </summary>
        private Vector3 startPosition = Vector3.zero;

        void Update() {
            
            #if UNITY_IOS || UNITY_ANDROID
            if (MouseButton == 0) {
                UpdateForTouches();
            }
            #else
            UpdateForMouse();
            #endif
        }

        private void UpdateForMouse() {

            if (!Input.GetMouseButton(MouseButton)) {
                var previousState = State;
                State = GestureRecognizerState.Ended;
                if (previousState != GestureRecognizerState.Ended) {
                    if (OnGesture != null) OnGesture(this);
                }
                return;
            }

            var mousePosition = Input.mousePosition;
            
            // Gesture Began
            if (State == GestureRecognizerState.Ended) {

                State = GestureRecognizerState.Began;
                startPosition = mousePosition;
                DragDelta = Vector3.zero;
                DragPosition = mousePosition;
                if (OnGesture != null) OnGesture(this);
                return;
            }

            DragDelta = mousePosition - DragPosition;
            DragPosition = mousePosition;

            if (DragDelta != Vector3.zero) {
                State = GestureRecognizerState.Changed;
                if (OnGesture != null) OnGesture(this);
            }
        }

        private void UpdateForTouches() {

            if (Input.touchCount == 0) {
                var previousState = State;
                State = GestureRecognizerState.Ended;
                if (previousState != GestureRecognizerState.Ended) {
                    if (OnGesture != null) OnGesture(this);
                }
                return;
            }

            var touchPos2D = Input.GetTouch(0).position;
            var touchPosition = new Vector3(touchPos2D.x, touchPos2D.y, 0);

            // Gesture Began
            if (State == GestureRecognizerState.Ended) {

                State = GestureRecognizerState.Began;
                startPosition = touchPosition;
                DragDelta = Vector3.zero;
                DragPosition = touchPosition;
                if (OnGesture != null) OnGesture(this);
                return;
            }

            DragDelta = touchPosition - DragPosition;
            DragPosition = touchPosition;

            if (DragDelta != Vector3.zero) {
                State = GestureRecognizerState.Changed;
                if (OnGesture != null) OnGesture(this);
            }
        }
    }
}