using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoScroll : MonoBehaviour {

	public enum ScrollPos { 
		Top, Bottom
	}

	public float topY;
	public float bottomY;

	public bool scrollByScreenHeight;

	public float scrollTime = 0.7f;

	public AnimationCurve curve;

	public ScrollPos CurrentScrollPos { 
		get {
			var y = transform.localPosition.y;
			if (Math.Abs(topY - y) < Math.Abs(bottomY - y)) {
				return ScrollPos.Top;
			} else {
				return ScrollPos.Bottom;
			}
		}
	}

	private float height;

	private Coroutine scrollRoutine;

	void Start() {
		
		var canvas = GameObject.FindGameObjectWithTag("SettingsCanvas");
		var canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

		if (scrollByScreenHeight) {
			bottomY = topY + canvasHeight;
		}
	}

	public void ScrollToTop() {

		StopCurrentRoutine();

		scrollRoutine = StartCoroutine(ScrollTo(topY, scrollTime, curve));
	}

	public void ScrollToBottom() {

		StopCurrentRoutine();

		scrollRoutine = StartCoroutine(ScrollTo(bottomY, scrollTime, curve));
	}

	private void StopCurrentRoutine() {

		if (scrollRoutine != null) {
			StopCoroutine(scrollRoutine);
		}
	}

	private IEnumerator ScrollTo(float y, float time, AnimationCurve curve) {

		var elapsedTime = 0f;
		var startingPos = transform.localPosition;
		var endingPos = new Vector3(transform.localPosition.x, y, transform.localPosition.z);

		while (elapsedTime <= time) {

			transform.localPosition = Vector3.Lerp(startingPos, endingPos, curve.Evaluate(elapsedTime / time));

			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}
}
