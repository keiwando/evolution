// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;

// [RequireComponent(typeof(Camera))]
// public class CameraFollowScript : MonoBehaviour {

// 	public RenderTexture renderTexture;

// 	public Creature toFollow;

// 	public bool DiagonalLock = false;

// 	private Vector3 startPos;

// 	void Start () {
// 		startPos = transform.position;
// 	}

// 	void Update () {

// 		if (toFollow == null) return;

// 		Vector3 newPos = transform.position;
// 		newPos.x = toFollow.GetXPosition();

// 		if (true || DiagonalLock) {
// 			newPos.y = (newPos.x - startPos.x) + startPos.y;
// 		}

// 		transform.position = newPos;
// 	}
// }
