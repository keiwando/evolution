using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/** Has to be attached to a camera. */
public class CameraFollowScript : MonoBehaviour {

	public RenderTexture renderTexture;

	public Creature toFollow;

	public bool DiagonalLock = false;

	new private Camera camera;

	private Vector3 startPos;

	// Use this for initialization
	void Start () {

		camera = GetComponent<Camera>();
		startPos = camera.transform.position;
	}

	// Update is called once per frame
	void Update () {

		if (toFollow == null) return;

		Vector3 newPos = transform.position;
		newPos.x = toFollow.GetXPosition();

		if (true || DiagonalLock) {
			newPos.y = (newPos.x - startPos.x) + startPos.y;
		}

		transform.position = newPos;
	}
}
