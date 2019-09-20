using UnityEngine;

namespace Keiwando.Experiments {
    public class MassExperimentProjectile : MonoBehaviour {
    
        private const float FORCE = 1000f;

        void Start() {
            var body = GetComponent<Rigidbody>();
            body.AddForce(new Vector3(0, FORCE));
        }
    }
}
