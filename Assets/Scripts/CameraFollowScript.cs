using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/** Has to be attached to a camera. */
public class CameraFollowScript : MonoBehaviour {

	public RenderTexture renderTexture;

	public Creature toFollow;

	public int currentlyWatchingIndex;

	private Camera camera;

	// Use this for initialization
	void Start () {

		camera = GetComponent<Camera>();

		toFollow = GameObject.Find("Creature").GetComponent<Creature>();

		if (gameObject.tag == "SecondCamera") {
			SwitchToMiniViewport();
		}
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 newPos = transform.position;
		newPos.x = toFollow.GetXPosition();
		transform.position = newPos;
	}

	public void SwitchToMiniViewport() {
		camera.targetTexture = renderTexture;
	}

	public void SwitchToFullscreen() {
		camera.targetTexture = null;
	}

}
