using UnityEngine;

namespace Keiwando.Evolution {

    public class DeactivateOnAndroid: MonoBehaviour {

        #if UNITY_ANDROID
        void Start() {
            gameObject.SetActive(false);
        }
        #endif
    }
}