using UnityEngine;
using System.Collections.Generic;

public class DistanceMarkerSpawner: MonoBehaviour {

    private const int INITIAL_SPAWN_COUNT = 100;
    private const float MARKER_DISTANCE = 5f;
    private const float STAT_ADJUSTMENT_FACTOR = 5f;

    [SerializeField]
    private Transform spawnPosition;

    [SerializeField]
    private DistanceMarker template;

    private List<DistanceMarker> allMarkers = new List<DistanceMarker>();

    void Start() {
        Spawn();
    }

    public void Spawn() {

        template.gameObject.SetActive(true);
        template.GetComponent<MeshRenderer>().sortingOrder = 30;
        var pos = template.transform.position;
        // Push the markers into the background
        pos.z = 3;
        var spawnX = spawnPosition.transform.position.x;

        // Create markers
        for (float i = 1; i <= INITIAL_SPAWN_COUNT; i++) {
            var dX = i * MARKER_DISTANCE;
            pos.x = (dX * STAT_ADJUSTMENT_FACTOR) + spawnX;
            var newMarker = Instantiate(template, pos, Quaternion.identity, template.transform.parent);
            newMarker.Text.text = dX.ToString("0");
            allMarkers.Add(newMarker);
        }

        template.gameObject.SetActive(false);
    }
}