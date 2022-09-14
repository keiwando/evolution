using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    [RequireComponent(typeof(Camera))]
    public class TrackedCamera: ZoomableCamera {

        public RenderTexture RenderTexture;

        new private Camera camera;

        /// <summary>
        /// The control points which define the camera track.
        /// Specifying no control points allows the camera to move freely.
        /// Specifying one control point fixes the camera in place
        /// Specifying two or more control points moves the camera along that path.
        /// </summary>
        public CameraControlPoint[] ControlPoints { 
            set {
                var pointMap = new Dictionary<float, CameraControlPoint>();
                for (int i = 0; i < value.Length; i++) {
                    pointMap[value[i].x] = value[i];
                }
                var uniquePoints = pointMap.Values.ToArray();

                // if (uniquePoints.Length == 0) {
                //     this.controlPoints = new [] { 
                //         new CameraControlPoint(0, 0, 0.5f), new CameraControlPoint(1, 0, 0.5f)
                //     };
                // } else if (uniquePoints.Length == 1) {
                //     this.controlPoints = new [] {
                //         uniquePoints[0], new CameraControlPoint(uniquePoints[0].x + 1, uniquePoints[0].y, uniquePoints[0].pivot)
                //     };
                // } else {
                this.controlPoints = uniquePoints;
                Array.Sort(this.controlPoints, delegate (CameraControlPoint lhs, CameraControlPoint rhs) {
                    return lhs.x.CompareTo(rhs.x);
                });
                // }
            }
        }
        private CameraControlPoint[] controlPoints;

        /// <summary>
        /// The transform to be tracked by this camera.
        /// </summary>
        public Creature Target;

        /// <summary>
        /// The index of the control points segment that the target was previously in.
        /// E.g. an index of 0 means that the target was last between 
        /// ControlPoints[0] and ControlsPoints[1]. 
        /// We cache this value to improve the efficiency of finding the current control
        /// segment, given the fact that most of the time the segment doesn't change
        /// between frames.
        /// </summary>
        private int lastControlSegmentIndex = 0;

        void Start() {
            this.camera = GetComponent<Camera>();
            base.Start(camera);
        }

        void Update() {

            if (Target == null) return;

            var controlPoint = GetInterpolatedControlPoint(Target.GetXPosition());
            this.zoomAnchor = new Vector2(0.5f, controlPoint.pivot);
            camera.transform.position = CalculateCameraPosition(camera, controlPoint);
        }

        protected override void OnAfterZoom() {
            Update();
        }

        private static Vector3 CalculateCameraPosition(Camera camera, CameraControlPoint controlPoint) {

            Vector2 orthoSize = CameraUtils.GetOrthographicSize(camera);
            float y = controlPoint.y - (controlPoint.pivot - 0.5f) * orthoSize.y;
            return new Vector3(controlPoint.x, y, camera.transform.position.z);
        }

        private CameraControlPoint GetInterpolatedControlPoint(float x) {

            if (controlPoints.Length == 0) {
                return new CameraControlPoint(x, Target.GetYPosition(), 0.31f);
            } else if (controlPoints.Length == 1) {
                return controlPoints[0];
            }

            // Find the current control point segment
            var segmentIndex = lastControlSegmentIndex;
            if (controlPoints[segmentIndex].x > x || x > controlPoints[segmentIndex + 1].x) {
                segmentIndex = BinarySearchCurrentSegment(x);
            }

            CameraControlPoint left;
            CameraControlPoint right;
            if (segmentIndex < 0) {
                // Extrapolate from first two control points
                left = controlPoints[0];
                right = controlPoints[1];
            } else if (segmentIndex > 0) {
                // Extrapolate from last two control points
                left = controlPoints[controlPoints.Length - 2];
                right = controlPoints[controlPoints.Length - 1];
            } else {
                // Interpolate in current segment
                left = controlPoints[segmentIndex];
                right = controlPoints[segmentIndex + 1];
            }

            float t = (x - left.x) / (right.x - left.x);
            float y = left.y + t * (right.y - left.y);
            float pivot = Math.Min(Math.Max(left.pivot + t * (right.pivot - left.pivot), 0), 1);

            return new CameraControlPoint(x, y, pivot);
        }

        private int BinarySearchCurrentSegment(float x) {

            if (x < controlPoints[0].x) return -1;
            if (x > controlPoints[controlPoints.Length - 1].x) return controlPoints.Length;

            int leftBound = 0;
            // There are at least two elements in the array, so this is okay
            int rightBound = controlPoints.Length - 2;

            while (leftBound <= rightBound) {
                int mid = (int)Math.Floor(0.5 * (leftBound + rightBound));
                var leftControlPoint = controlPoints[mid];
                var rightControlPoint = controlPoints[mid + 1];
                if (leftControlPoint.x <= x && x <= rightControlPoint.x) {
                    return mid;
                } else if (x < leftControlPoint.x) {
                    rightBound = mid - 1;
                } else {
                    leftBound = mid + 1;
                }
            }

            throw new Exception("Failed to determine current segment. Array might not be sorted correctly");
        }
    }
}