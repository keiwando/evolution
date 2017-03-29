using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MuscleJoint : Hoverable {

	public int ID;

	public Vector3 position { 
		get {
			return transform.position;
		} 
	}

	private List<Muscle> connectedMuscles = new List<Muscle>();

	// Use this for initialization
	void Start () {
		base.Start();

		//connectedMuscles = 
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Connect(Muscle muscle) {

		connectedMuscles.Add(muscle);
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

	/** Connects the end of a Muscle prefab object to the gameobject with a hinge joint. */
	/*public void Connect(Muscle muscle) {

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
