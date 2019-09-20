using UnityEngine;

namespace Keiwando.Evolution.UI {

    public class EnableIfHomeIndicator: MonoBehaviour {

        void Start() {

            #if UNITY_IOS
            this.gameObject.SetActive(Screen.currentResolution.height != Screen.safeArea.height);            
            # else 
            this.gameObject.SetActive(false);
            #endif
        }
    }
}