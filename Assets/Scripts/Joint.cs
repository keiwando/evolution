using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Joint : BodyComponent {

	private const string PATH = "Prefabs/Joint";

	public Vector3 center { 
		get {
			return transform.position;
		} 
	}

	private Dictionary<Bone, HingeJoint> joints = new Dictionary<Bone, HingeJoint>();

	private bool iterating;

	public bool isCollidingWithGround { get; private set; }
	public bool isCollidingWithObstacle { get; private set; }

	private static Joint InstantiateJoint(Vector3 point) {
		return ((GameObject) Instantiate(Resources.Load(PATH), point, Quaternion.identity)).GetComponent<Joint>();
	}

	public static Joint CreateAtPoint(Vector3 point) {
		ID_COUNTER++;
		var joint = Joint.InstantiateJoint(point);
		joint.ID = ID_COUNTER;
		return joint;
	}

	public static Joint CreateFromString(string data) {
		
		var parts = data.Split('%');
		//print(data);
		// Format: ID - pos.x - pos.y - pos.z
		var x = float.Parse(parts[1]);
		var y = float.Parse(parts[2]);
		var z = float.Parse(parts[3]);

		var joint = Joint.InstantiateJoint(new Vector3(x,y,z));
		joint.ID = int.Parse(parts[0]);
		ID_COUNTER = Mathf.Max(ID_COUNTER, joint.ID);

		return joint;
	}

	// Use this for initialization
	public override void Start () {
		base.Start();

		//joints 
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/** Connects a Bone to the gameobject with a hinge joint. */
	public void Connect(Bone bone) {

		HingeJoint joint = gameObject.AddComponent<HingeJoint>();
		joint.anchor = Vector3.zero;
		joint.axis = new Vector3(0, 0, 1);
		joint.autoConfigureConnectedAnchor = true;
		joint.useSpring = true;
		//var spring = joint.spring;
		//spring.spring = 1000f;
		//joint.spring = spring;
		//joint.connectedAnchor = new Vector3(0, 1.14f, 0);
		joint.enablePreprocessing = false;

		joint.connectedBody = bone.gameObject.GetComponent<Rigidbody>();

		joints.Add(bone, joint);
	}

	/** Disconnects the bone from the joint. */
	public void Disconnect(Bone bone) {

		HingeJoint joint = joints[bone];
		Destroy(joint);
		if (!iterating)
			joints.Remove(bone);
	}

	/** Deletes the joint and all attached objects from the scene. */
	public override void Delete() {
		base.Delete();

		iterating = true;

		List<Bone> toDelete = new List<Bone>();
		// disconnect the bones
		foreach(Bone bone in joints.Keys) {

			bone.Delete();
			toDelete.Add(bone);
		}

		foreach(Bone bone in toDelete) {
			joints.Remove(bone);
		}

		iterating = false;

		Destroy(gameObject);
	}

	public override void PrepareForEvolution () {
		
		GetComponent<Rigidbody>().isKinematic = false;
	}

	/// <summary>
	/// Generates a string that holds all the information needed to save and rebuild this BodyComponent.
	/// Format: Own ID % pos.x % pos.y % pos.z
	/// </summary>
	/// <returns>The save string.</returns>
	public override string GetSaveString () {
		var pos = transform.position;
		return string.Format("{0}%{1}%{2}%{3}", ID, pos.x, pos.y, pos.z);
	}

		
	void OnTriggerEnter(Collider collider) {

		switch(collider.gameObject.tag.ToUpper()) {

		case "GROUND": isCollidingWithGround = true; break;
		case "OBSTACLE": isCollidingWithObstacle = true; break;

		default: return;
		}	
	}

	void OnTriggerExit(Collider collider) {

		if (collider.tag.ToUpper() == "OBSTACLE") 
			isCollidingWithObstacle = false;
		else
			isCollidingWithGround = false;
		
	}
}
