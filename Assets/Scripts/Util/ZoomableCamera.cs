using UnityEngine;

namespace Keiwando.Evolution {

    public abstract class ZoomableCamera: MonoBehaviour {

        /// <summary>
        /// The amount by which the camera can be zoomed in.
        /// </summary>
        [SerializeField]
        protected float zoomInLength = 10;

        /// <summary>
        /// The amount by which the camera can be zoomed out
        /// </summary>
        [SerializeField]
        protected float zoomOutLength = 10;

        /// <summary>
        /// The difference between the lowest and highest zoom level (orthographicSize).
        /// </summary>
        private float zoomLength {
            get { return zoomInLength + zoomOutLength; }
        }

        /// <summary>
        /// The minimum allowed value of the camera's orthographic size
        /// </summary>
        private float minZoom;

        // Gestures

        private float lastPinchDist = 0f;

        internal virtual void Start(Camera camera) {
            var initialZoom = camera.orthographicSize;
            minZoom = initialZoom - zoomInLength;

            InputRegistry.shared.Register(InputType.Scroll, delegate (InputType type) {
                UpdateInputs();
            });
        }

        
    }
}