using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Joint : BodyComponent {

	public Vector3 center { 
		get {
			return transform.position;
		} 
	}

	private Dictionary<Bone, HingeJoint> joints;

	private bool iterating;

	public bool isColliding { get; private set; }

	// Use this for initialization
	public override void Start () {
		base.Start();

		joints = new Dictionary<Bone, HingeJoint>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/** Connects a Bone to the gameobject with a hinge joint. */
	public void connect(Bone bone) {

		HingeJoint joint = gameObject.AddComponent<HingeJoint>();
		joint.anchor = Vector3.zero;
		joint.axis = new Vector3(0, 0, 1);
		joint.autoConfigureConnectedAnchor = true;
		//joint.connectedAnchor = new Vector3(0, 1.14f, 0);
		joint.enablePreprocessing = true;

		joint.connectedBody = bone.gameObject.GetComponent<Rigidbody>();

		joints.Add(bone, joint);
	}

	/** Disconnects the bone from the joint. */
	public void disconnect(Bone bone) {

		HingeJoint joint = joints[bone];
		Destroy(joint);
		if (!iterating)
			joints.Remove(bone);
	}

	/** Deletes the joint and all attached objects from the scene. */
	public override void delete() {
		base.delete();

		iterating = true;

		List<Bone> toDelete = new List<Bone>();
		// disconnect the bones
		foreach(Bone bone in joints.Keys) {

			bone.delete();
			toDelete.Add(bone);
		}

		foreach(Bone bone in toDelete) {
			joints.Remove(bone);
		}

		iterating = false;

		Destroy(gameObject);
	}

	public override void prepareForEvolution ()
	{
		GetComponent<Rigidbody>().isKinematic = false;
	}
		
	void OnCollisionEnter() {
		isColliding = true;
	}

	void OnCollisionExit() {
		isColliding = false;
	}
}
