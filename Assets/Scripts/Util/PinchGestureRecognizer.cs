using UnityEngine;

namespace Keiwando {

    public class PinchGestureRecognizer: MonoBehaviour, IGestureRecognizer<PinchGestureRecognizer> {

        public event GestureCallback<PinchGestureRecognizer> OnGesture;

        public GestureRecognizerState State { get; private set; } = GestureRecognizerState.Ended;

        /// <summary>
        /// The center of the pinch gesture in screen coordinates.
        /// </summary>
        public Vector3 PinchCenter { get; private set; } = Vector3.zero;

        /// <summary>
        /// The pinch center distance between the current and the previous frame in screen coordinates.
        /// </summary>
        public Vector3 PinchCenterDelta { get; private set; } = Vector3.zero;

        /// <summary>
        /// The current pinch distance relative to the distance at the start of the gesture.
        /// </summary>
        /// <value></value>
        public float Scale { get; private set; } = 1f;

        /// <summary>
        /// The current pinch distance relative to the distance on the last frame.
        /// </summary>
        public float ScaleDelta { get; private set; } = 1f;

        
        /// <summary>
        /// The pinch distance at the start of the gesture.
        /// </summary>
        private float startPinchDistance;

        #if UNITY_IOS || UNITY_ANDROID

        void Update() {
            
            // Gesture Ended
            if (Input.touchCount != 2) {
                var previousState = State;
                State = GestureRecognizerState.Ended;
                if (previousState != GestureRecognizerState.Ended) {
                    if (OnGesture != null) OnGesture(this);
                }
                return;
            }

            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);
            
            var oldPinchCenter = PinchCenter;
            PinchCenter = CalculatePinchCenter(touch1.position, touch2.position);
            PinchCenterDelta = PinchCenter - oldPinchCenter;

            var touchDistance = (touch1.position - touch2.position).magnitude * 0.5f;
            if (touchDistance == 0f) touchDistance = 0.00001f;

            // Gesture Began
            if (State == GestureRecognizerState.Ended) {

                State = GestureRecognizerState.Began;
                startPinchDistance = touchDistance;
                Scale = 1f;
                ScaleDelta = 1f;
                PinchCenterDelta = Vector3.zero;
                if (OnGesture != null) OnGesture(this);
                return;
            }


            float newScale = touchDistance / startPinchDistance;
            ScaleDelta = newScale / Scale;
            Scale = newScale;

            if (ScaleDelta != 1f || oldPinchCenter != PinchCenter) {
                State = GestureRecognizerState.Changed;
                if (OnGesture != null) OnGesture(this);
            }
        }

        #endif

        private Vector3 CalculatePinchCenter(Vector2 touch1, Vector2 touch2) {

            var center2D = 0.5f * (touch1 + touch2);
            return new Vector3(center2D.x, center2D.y);
        }
    }
}