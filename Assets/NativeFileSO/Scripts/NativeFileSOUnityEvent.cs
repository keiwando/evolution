using System;
using UnityEngine;

namespace Keiwando.NativeFileSO { 

	public class NativeFileSOUnityEvent : MonoBehaviour {

		public static event Action UnityReceivedControl;

		private static NativeFileSOUnityEvent instance;

		void Awake() {
			if (instance == null) {
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			} else {
				Destroy(this.gameObject);
			}
		}

		private void Start() {
			SendEvent();
		}

		private void OnApplicationFocus(bool focus) {

			if (focus) {
				SendEvent();
			}
		}

		private void OnApplicationPause(bool pause) {
			if (!pause) {
				SendEvent();
			}
		}

		private void SendEvent() {
			if (UnityReceivedControl != null) {
				UnityReceivedControl();
			}
		}
	}
}
