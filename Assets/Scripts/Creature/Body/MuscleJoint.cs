using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MuscleJoint : MonoBehaviour { //: Hoverable

	public Rigidbody ConnectedBone {
		get { return bone; }
	}
	private Rigidbody bone;

	public Rigidbody Body { 
		get { return body; }
	}
	private Rigidbody body;

	private List<Muscle> connectedMuscles = new List<Muscle>();

	void Start () {
		//fixedJoint = GetComponent<FixedJoint>();
		bone = GetComponentInParent<Rigidbody>();
		body = GetComponent<Rigidbody>();

	}

	public void Connect(Muscle muscle) {

		connectedMuscles.Add(muscle);
		//fixedJoint = GetComponent<FixedJoint>();
	}

	public void Disconnect(Muscle muscle) {
		
		connectedMuscles.Remove(muscle);
	}

	public void deleteAllConnected() {

		var connected = new List<Muscle>(connectedMuscles);

		foreach (Muscle muscle in connected) {
			if (muscle != null) {

				muscle.Delete();
			}
		} 

		connectedMuscles.Clear();
	}
}
