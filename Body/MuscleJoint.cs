using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MuscleJoint : MonoBehaviour { //: Hoverable

	public int ID;

//	public Vector3 position { 
//		get {
//			return transform.position;
//		} 
//	}

//	public FixedJoint FixedJoint {
//		get { return fixedJoint; }
//	}
//	private FixedJoint fixedJoint;

	public Rigidbody ConnectedBone {
		get { return bone; }
	}
	private Rigidbody bone;

	private List<Muscle> connectedMuscles = new List<Muscle>();


	// Use this for initialization
	void Start () {
		//fixedJoint = GetComponent<FixedJoint>();
		bone = GetComponentInParent<Rigidbody>();
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
