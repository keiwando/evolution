using UnityEngine;

namespace Keiwando {

    public class ScrollGestureRecognizer: MonoBehaviour, IGestureRecognizer<ScrollGestureRecognizer> {

        public event GestureCallback<ScrollGestureRecognizer> OnGesture;

        public GestureRecognizerState State { get; private set; } = GestureRecognizerState.Ended;

        /// <summary>
        /// The current mouse scroll delta
        /// </summary>
        public Vector2 ScrollDelta { get; private set; } = Vector2.zero;

        void Update() {
            
            var oldState = State;
            ScrollDelta = Input.mouseScrollDelta;

            if (ScrollDelta != Vector2.zero) {
                if (State == GestureRecognizerState.Ended) {
                    State = GestureRecognizerState.Began;
                } else {
                    State = GestureRecognizerState.Changed;
                }
            } else {
                State = GestureRecognizerState.Ended;
            }

            if (State == GestureRecognizerState.Changed || oldState != State) {
                if (OnGesture != null) OnGesture(this);
            }
        }
    }
}