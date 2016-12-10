using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MuscleJoint : Hoverable {

	public Vector3 position { 
		get {
			return transform.position;
		} 
	}

	private List<Muscle> connectedMuscles;

	// Use this for initialization
	void Start () {
		base.Start();

		connectedMuscles = new List<Muscle>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void connect(Muscle muscle) {

		connectedMuscles.Add(muscle);
	}

	public void disconnect(Muscle muscle) {
		
		connectedMuscles.Remove(muscle);
	}

	public void deleteAllConnected() {

		foreach (Muscle muscle in connectedMuscles) {
			if (muscle != null) {
				muscle.deleteWithoutDisconnecting();	
			}
		} 

		connectedMuscles.Clear();
	}

	/** Connects the end of a Muscle prefab object to the gameobject with a hinge joint. */
	/*public void connect(Muscle muscle) {

		HingeJoint joint = gameObject.AddComponent<HingeJoint>();
		joint.anchor = Vector3.zero;
		joint.axis = new Vector3(0, 0, 1);
		joint.autoConfigureConnectedAnchor = true;
		//joint.connectedAnchor = new Vector3(0, 1.14f, 0);
		joint.enablePreprocessing = true;

		joint.connectedBody = muscle.gameObject.GetComponent<Rigidbody>();

		joints.Add(joint);
	}*/
}
