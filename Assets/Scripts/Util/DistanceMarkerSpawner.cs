using UnityEngine;
using System.Collections.Generic;

namespace Keiwando.Evolution {

    public class DistanceMarkerSpawner: MonoBehaviour {

        private const int INITIAL_SPAWN_COUNT = 100;
        private const float STAT_ADJUSTMENT_FACTOR = 5f;

        public float MarkerDistance { get; set; } = 5f;

        [SerializeField]
        private DistanceMarker template;

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
            for (float i = 1; i <= INITIAL_SPAWN_COUNT; i++) {
                pos += transform.right * MarkerDistance * STAT_ADJUSTMENT_FACTOR;
                var labelValue = i * MarkerDistance;
                var newMarker = Instantiate(template, pos, Quaternion.identity, template.transform.parent);
                newMarker.Text.text = labelValue.ToString("0");
                allMarkers.Add(newMarker);
            }

            template.gameObject.SetActive(false);
        }
    }
}

