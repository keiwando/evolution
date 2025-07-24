﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectableButton : MonoBehaviour {

	public Vector2 SelectionOffset = new Vector2(20, 0);
	public Vector2 ReferenceResolution = new Vector2(1920, 1080);

	private Vector3 defaultPosition;
	private Vector3 selectedPosition;

	public ButtonManager manager;

	private bool selected;
	public bool Selected {
		get { return selected; }
		set {
			selected = value;
			float time = 0.5f;
			if (routine != null) this.StopCoroutine(routine);

			if (selected) {
				routine = SmoothMove(selectedPosition, time);
			} else {
				routine = SmoothMove(defaultPosition, time);
			}
			if (this.gameObject.activeSelf) {
				this.StartCoroutine(routine);
			}
		}
	}

	private IEnumerator routine;

	void Start () {
		var offsetX = SelectionOffset.x * (float)Screen.width / (ReferenceResolution.x * transform.lossyScale.x);
		var offsetY = SelectionOffset.y * (float)Screen.height / (ReferenceResolution.y * transform.lossyScale.y);
		defaultPosition = transform.localPosition;
		selectedPosition = new Vector3(defaultPosition.x + offsetX, defaultPosition.y + offsetY, 0);
	}

	public void OnClick() {
		manager.SelectButton(this);
	}

	IEnumerator SmoothMove(Vector3 target, float delta)
	{
		// Will need to perform some of this process and yield until next frames
		float closeEnough = 0.2f;
		float distance = (transform.localPosition - target).magnitude;

		// GC will trigger unless we define this ahead of time
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		// Continue until we're there
		while(distance >= closeEnough)
		{
			// Move a bit then  wait until next  frame
			transform.localPosition = Vector3.Slerp(transform.localPosition, target, delta);
			yield return wait;

			// Check if we should repeat
			distance = (transform.localPosition - target).magnitude;
		}

		// Complete the motion to prevent negligible sliding
		transform.localPosition = target;
	}
}
