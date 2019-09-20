using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GroundRaycastTest {

    public class GroundRaycastTest: MonoBehaviour {

        [SerializeField] private GameObject raycastOrigin;

        void Start() {

            var ground = GameObject.FindGameObjectWithTag("StaticForeground");
            ground.GetComponent<BoxCollider>().enabled = false;
            ground.GetComponent<BoxCollider>().enabled = true;

            var groundLayerMask = LayerMask.NameToLayer("StaticForeground");
            Debug.Log("GroundMask " + groundLayerMask);

            RaycastHit hit;
            // if (Physics.Raycast(raycastOrigin.transform.position, Vector3.down, out hit, groundDistanceLayerMask)) {
            if (Physics.Raycast(raycastOrigin.transform.position, Vector3.down, out hit, Mathf.Infinity, 1 << groundLayerMask)) {
			
                if (hit.collider.gameObject.CompareTag("Ground")) {
                    Debug.Log("Ground Distance " + hit.distance);
                    return;
                }
            }

            Debug.Log("No hit");
        }
    }
}