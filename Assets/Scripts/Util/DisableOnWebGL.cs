using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DisableOnWebGL : MonoBehaviour {

	void Start () {

		#if UNITY_WEBGL
		Disable();
		#endif
	}

	void Disable() {

		var button = GetComponent<Button>();
		if (button != null) {

			var canvasGroup = gameObject.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 0.4f;
			button.enabled = false;

		} else {
			this.gameObject.SetActive(false);
		}
	}
}
