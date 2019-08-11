using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPerformanceExperiment : MonoBehaviour {

    private const int RAYCAST_COUNT = 160;

    private float totalDistance = 0f;

    void FixedUpdate() {

        for (int i = 0; i < RAYCAST_COUNT; i++) {
            totalDistance += RandomRaycast();
        }
    }

    private float RandomRaycast() {

        var distance = 0f;
        RaycastHit hit;
        if (Physics.Raycast(
            transform.position,
            new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)),
            out hit,
            float.MaxValue,
            1 << 9)
        ) {
            distance = hit.distance;
        }
        return distance;
    }
    
}
