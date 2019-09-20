using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Experiments {
    public class MassRotationExperiment : MonoBehaviour {
    
        private const float FORCE = 10000f;

        void Start() {
            var body = GetComponent<Rigidbody>();
            // body.AddRelativeTorque(new Vector3(0, FORCE));
            body.AddForce(new Vector3(transform.position.x - 5, FORCE));
        }
    }
}
