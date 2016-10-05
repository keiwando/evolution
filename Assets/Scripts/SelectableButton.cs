using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectableButton : MonoBehaviour {

	private Vector3 defaultPosition;
	private Vector3 selectedPosition;

	public ButtonManager manager;

	public bool selected {
		get { return selected; }
		set {
			selected = value;
			float time = 0.05f;
			if (selected) {
				this.StartCoroutine(SmoothMove(selectedPosition, time));
			} else {
				this.StartCoroutine(SmoothMove(defaultPosition, time));
			}
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnClick() {
		print("Click");
		manager.selectButton(this);
	}


	IEnumerator SmoothMove(Vector3 target, float delta)
	{
		// Will need to perform some of this process and yield until next frames
		float closeEnough = 0.2f;
		float distance = (transform.position - target).magnitude;

		// GC will trigger unless we define this ahead of time
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		// Continue until we're there
		while(distance >= closeEnough)
		{
			// Move a bit then  wait until next  frame
			transform.position = Vector3.Slerp(transform.position, target, delta);
			yield return wait;

			// Check if we should repeat
			distance = (transform.position - target).magnitude;
		}

		// Complete the motion to prevent negligible sliding
		transform.position = target;
	}
}
