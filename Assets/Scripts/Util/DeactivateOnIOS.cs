using UnityEngine;

namespace Keiwando.Evolution {

    public class DeactivateOnIOS: MonoBehaviour {

        #if UNITY_IOS
        void Start() {
            gameObject.SetActive(false);
        }
        #endif
    }
}