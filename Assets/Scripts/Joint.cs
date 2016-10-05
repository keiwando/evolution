using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Joint : Hoverable {

	public Vector3 center { 
		get {
			return transform.position;
		} 
	}

	private List<HingeJoint> joints;

	// Use this for initialization
	public override void Start () {
		base.Start();

		joints = new List<HingeJoint>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/** Connects a BodyConnection to the gameobject with a hinge joint. */
	public void connect(BodyConnection bone) {

		HingeJoint joint = gameObject.AddComponent<HingeJoint>();
		joint.anchor = Vector3.zero;
		joint.axis = new Vector3(0, 0, 1);
		joint.autoConfigureConnectedAnchor = true;
		//joint.connectedAnchor = new Vector3(0, 1.14f, 0);
		joint.enablePreprocessing = true;

		joint.connectedBody = bone.gameObject.GetComponent<Rigidbody>();

		joints.Add(joint);
	}
		
}
