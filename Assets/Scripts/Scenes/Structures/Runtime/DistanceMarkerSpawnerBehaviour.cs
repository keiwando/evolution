using UnityEngine;
using System.Collections.Generic;

namespace Keiwando.Evolution.Scenes {

    public class DistanceMarkerSpawnerBehaviour: MonoBehaviour {

        private const int INITIAL_SPAWN_COUNT = 100;
        private const float STAT_ADJUSTMENT_FACTOR = 5f;

        public float MarkerDistance { get; set; } = 5f;
        public float DistanceAngleFactor { get; set; } = 1f;
        public float BestMarkerRotation { get; set; } = 0f;

        public ISceneContext Context { get; set; }

        [SerializeField]
        private DistanceMarker template;
        [SerializeField]
        private DistanceMarker bestMarkerTemplate;

        private List<DistanceMarker> allMarkers = new List<DistanceMarker>();

        void Start() {
            Spawn();
        }

        public void Spawn() {

            template.gameObject.SetActive(true);
            template.GetComponent<MeshRenderer>().sortingOrder = 30;
            
            var pos = transform.position;
            // Push the markers into the background
            pos.z = 3;
            // Create markers
            for (int i = 1; i <= INITIAL_SPAWN_COUNT; i++) {
                pos += transform.right * MarkerDistance * STAT_ADJUSTMENT_FACTOR;
                var labelValue = (int)(i * MarkerDistance * DistanceAngleFactor);
                AddMarker(template, pos, labelValue.ToString());
            }

            // Create marker for best of previous gen
            var prevBestDistance = Context != null ? Context.GetDistanceOfBest() : float.NaN;
            if (!float.IsNaN(prevBestDistance)) {
                var actualDistance = prevBestDistance / (STAT_ADJUSTMENT_FACTOR * DistanceAngleFactor);
                var bestLabelPos = transform.position + (prevBestDistance * transform.right);
                var rotationEulerAngle = BestMarkerRotation;
                var marker = AddMarker(bestMarkerTemplate, bestLabelPos, actualDistance.ToString("0"), rotationEulerAngle);
                marker.Text = "---  ";
                marker.TextColor = new Color(0.23f, 0.23f, 0.23f, 0.36f);
            }

            template.gameObject.SetActive(false);
            bestMarkerTemplate.gameObject.SetActive(false);
        }

        private DistanceMarker AddMarker(DistanceMarker template, Vector3 pos, string label, float rotation = 0) {

            var newMarker = Instantiate(template, pos, Quaternion.Euler(0, 0, rotation), template.transform.parent);
            newMarker.Text = label;
            newMarker.gameObject.layer = this.gameObject.layer;
            var markerTransform = newMarker.transform;
            for (int i = 0; i < markerTransform.childCount; i++) {
                var child = markerTransform.GetChild(i);
                child.gameObject.layer = newMarker.gameObject.layer;
            }
            allMarkers.Add(newMarker);
            return newMarker;
        }
    }
}