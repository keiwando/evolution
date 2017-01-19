using UnityEngine;
using System.Collections;

/** Has to be attached to a camera. */
public class CameraFollowScript : MonoBehaviour {

	public Creature toFollow;

	public int currentlyWatchingIndex;

	// Use this for initialization
	void Start () {

		toFollow = GameObject.Find("Creature").GetComponent<Creature>();
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 newPos = transform.position;
		newPos.x = toFollow.GetXPosition();
		transform.position = newPos;
	}
}
