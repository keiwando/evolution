using System;
using UnityEngine;
using Keiwando;

namespace Keiwando.Evolution {

    [RequireComponent(typeof(Camera))]
    public class BoundedCamera: ZoomableCamera {

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
            var size = CameraUtils.GetOrthographicSize(camera);
            var halfWidth = size.x * 0.5f;
            var halfHeight = size.y * 0.5f;

            float minX = leftMovementPadding >= 0 ? cameraPos.x - halfWidth - leftMovementPadding : float.MinValue;
            float maxX = rightMovementPadding >= 0 ? cameraPos.x + halfWidth + rightMovementPadding : float.MaxValue;
            float minY = bottomMovementPadding >= 0 ? cameraPos.y - halfHeight - bottomMovementPadding : float.MinValue;
            float maxY = topMovementPadding >= 0 ? cameraPos.y + halfHeight + topMovementPadding : float.MaxValue;
            movementBounds.min = new Vector2(minX, minY);
            movementBounds.max = new Vector2(maxX, maxY);
            
            // Prioritize movement bounds over zoom out length
            var initialZoom = camera.orthographicSize;

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

            InputRegistry.shared.Register(InputType.Touch | InputType.Click, this, EventHandleMode.PassthroughEvent);
            // Drag with middle mouse button
            var dragRecognizer = GestureRecognizerCollection.shared.GetDragGestureRecognizer(2);
            dragRecognizer.OnGesture += OnDrag;
        }

        private void OnDrag(DragGestureRecognizer rec) {
            if (InputRegistry.shared.MayHandle(InputType.Touch | InputType.Click, this))
                    MoveCamera(CameraUtils.ScreenToWorldDistance(camera, -rec.DragDelta));
        }

        protected override void OnAfterZoom() {
            ClampPosition();
        }

        protected override void OnPinch(PinchGestureRecognizer gestureRecognizer) {
            MoveCamera(CameraUtils.ScreenToWorldDistance(camera, gestureRecognizer.PinchCenterDelta));
        }

        private void MoveCamera(Vector3 distance) {

            distance.z = 0;
            var position = camera.transform.position + distance;
            position = ClampPosition(position);

            var oldPos = camera.transform.position;

            camera.transform.position = position;
        }

        private void ClampPosition() {
            camera.transform.position = ClampPosition(camera.transform.position);
        }

        private Vector3 ClampPosition(Vector3 pos) {

            var halfHeight = camera.orthographicSize;
            var halfWidth = camera.aspect * halfHeight;

            var boundsMinY = movementBounds.min.y;

            // if (bottomBoundRelativeToZoom) {
            //     var zoom = camera.orthographicSize;
            //     var zoomOutLength = Math.Max(this.zoomOutLength, 0.0000001f);
            //     var initialZoom = minZoom + zoomOutLength;
            //     var zoomOutPercent = 1 - Math.Min(Math.Max((initialZoom - zoom) / zoomOutLength, 0), 1);
            //     boundsMinY += bottomMovementPadding * (1 - zoomOutPercent);
            // }

            var minX = movementBounds.min.x + halfWidth;
            var maxX = movementBounds.max.x - halfWidth;
            var minY = boundsMinY + halfHeight;
            var maxY = movementBounds.max.y - halfHeight;

            pos.x = Mathf.Clamp(pos.x, minX, maxX); 
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            return pos;
        }
    }
}