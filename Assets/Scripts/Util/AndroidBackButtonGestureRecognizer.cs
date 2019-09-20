#pragma warning disable CS0067
using UnityEngine;

namespace Keiwando {

    public class AndroidBackButtonGestureRecognizer: MonoBehaviour, 
        IGestureRecognizer<AndroidBackButtonGestureRecognizer> {

        public event GestureCallback<AndroidBackButtonGestureRecognizer> OnGesture;

        public GestureRecognizerState State { get; private set; } = GestureRecognizerState.Ended;

        #if UNITY_ANDROID
        
        void Update() {

            if (Input.GetKeyDown(KeyCode.Escape)) {
                State = GestureRecognizerState.Began;
                if (OnGesture != null) OnGesture(this);
            } else {
                State = GestureRecognizerState.Ended;
            }
        }

        #endif
    }
}