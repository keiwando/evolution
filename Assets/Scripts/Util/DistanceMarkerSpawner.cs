using UnityEngine;
using System.Collections.Generic;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

    public class DistanceMarkerSpawner: MonoBehaviour {

        private const int INITIAL_SPAWN_COUNT = 100;
        private const float STAT_ADJUSTMENT_FACTOR = 5f;

        public float MarkerDistance { get; set; } = 5f;

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
            // TODO: Add factor to distance calculation
            // Create markers
            for (int i = 1; i <= INITIAL_SPAWN_COUNT; i++) {
                pos += transform.right * MarkerDistance * STAT_ADJUSTMENT_FACTOR;
                var labelValue = i * MarkerDistance;
                AddMarker(template, pos, labelValue.ToString("0"));
            }

            // Create marker for best of previous gen
            var prevBestDistance = Context != null ? Context.GetDistanceOfBest(Context.GetCurrentGeneration() - 1) : float.NaN;
            if (!float.IsNaN(prevBestDistance)) {
                var actualDistance = prevBestDistance / STAT_ADJUSTMENT_FACTOR;
                var bestLabelPos = transform.position + (prevBestDistance * transform.right);
                var marker = AddMarker(bestMarkerTemplate, bestLabelPos, actualDistance.ToString("0"));
                marker.Text.color = new Color(0.784f, 0.149f, 0.133f);
                print(bestLabelPos);
            }

            template.gameObject.SetActive(false);
            bestMarkerTemplate.gameObject.SetActive(false);
        }

        private DistanceMarker AddMarker(DistanceMarker template, Vector3 pos, string label) {
            var newMarker = Instantiate(template, pos, Quaternion.identity, template.transform.parent);
            newMarker.Text.text = label;
            allMarkers.Add(newMarker);
            return newMarker;
        }
    }
}

