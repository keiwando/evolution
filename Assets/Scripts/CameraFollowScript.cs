using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/** Has to be attached to a camera. */
public class CameraFollowScript : MonoBehaviour {

	public RenderTexture renderTexture;

	public Creature toFollow;

	public int currentlyWatchingIndex;

	public bool DiagonalLock = false;

	private Camera camera;

	private Vector3 startPos;

	// Use this for initialization
	void Start () {

		camera = GetComponent<Camera>();
		startPos = camera.transform.position;

		toFollow = GameObject.Find("Creature").GetComponent<Creature>();

		if (gameObject.tag == "SecondCamera") {
			SwitchToMiniViewport();
		}
	}

	// Update is called once per frame
	void Update () {

		Vector3 newPos = transform.position;
		newPos.x = toFollow.GetXPosition();

		if (DiagonalLock) {
			newPos.y = (newPos.x - startPos.x) + startPos.y;
		}

		transform.position = newPos;
	}

	public void SwitchToMiniViewport() {
		camera.targetTexture = renderTexture;
	}

	public void SwitchToFullscreen() {
		camera.targetTexture = null;
	}


}
