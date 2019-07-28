using System;
using UnityEngine;
using Keiwando;

namespace Keiwando.Evolution {

    [RequireComponent(typeof(Camera))]
    public class BoundedCamera: ZoomableCamera {

        [SerializeField]
        public bool isControlledByInput = true;

        /// <summary>
        /// The camera to be controlled
        /// </summary>
        new private Camera camera;

        [SerializeField]
        private float bottomMovementPadding = 0f;
        [SerializeField]
        private float topMovementPadding = 0f;
        [SerializeField]
        private float leftMovementPadding = 0f;
        [SerializeField]
        private float rightMovementPadding = 0f;

        /// <summary>
        /// The bounds in world coordinates, inside of which the camera can be moved around.
        /// </summary>
        /// <remarks>
        /// The visible bounds of the camera cannot be moved outside of these movementBounds.
        /// </remarks>
        private Rect movementBounds = new Rect();


        void Start() {
            
            this.camera = GetComponent<Camera>();
            base.Start(camera);

            var cameraPos = camera.transform.position;
            var size = GetOrthographicSize();
            var halfWidth = size.x * 0.5f;
            var halfHeight = size.y * 0.5f;

            float minX = leftMovementPadding >= 0 ? cameraPos.x - halfWidth - leftMovementPadding : float.MinValue;
            float maxX = rightMovementPadding >= 0 ? cameraPos.x + halfWidth + rightMovementPadding : float.MaxValue;
            float minY = bottomMovementPadding >= 0 ? cameraPos.y - halfHeight - bottomMovementPadding : float.MinValue;
            float maxY = topMovementPadding >= 0 ? cameraPos.y + halfHeight + topMovementPadding : float.MaxValue;
            movementBounds.min = new Vector2(minX, minY);
            movementBounds.max = new Vector2(maxX, maxY);

            var initialZoom = camera.orthographicSize;

            // Prioritize movement bounds over zoom out length

            if (camera.aspect > (movementBounds.width / movementBounds.height)) {
                // Check for violation of horizontal movement bounds
                if (minX != float.MinValue && maxX != float.MaxValue) {
                    var maxOrthoSize = 0.5f * size.y * movementBounds.width / size.x;
                    zoomOutLength = Math.Min(zoomOutLength, maxOrthoSize - initialZoom);
                }
            } else {
                // Check for violation of vertical movement bounds
                if (minY != float.MinValue && maxY != float.MaxValue) {
                    var maxOrthoSize = 0.5f * movementBounds.height;
                    zoomOutLength = Math.Min(zoomOutLength, maxOrthoSize - initialZoom);
                }
            }
        }
    }
}