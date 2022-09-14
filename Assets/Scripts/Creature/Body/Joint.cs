using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Joint : BodyComponent {

	private const string PATH = "Prefabs/Joint";

	public Vector3 center { 
		get { return transform.position; } 
	}

	public JointData JointData { get; set; }

	public Rigidbody Body {
		get { return body; }
	}
	private Rigidbody body;

	public bool isCollidingWithGround { get; private set; }
	public bool isCollidingWithObstacle { get; private set; }

	private Dictionary<Bone, UnityEngine.Joint> joints = new Dictionary<Bone, UnityEngine.Joint>();

	private Vector3 resetPosition;
	private Quaternion resetRotation;
	private bool iterating;

	public static Joint CreateFromData(JointData data) {
		
		var joint = ((GameObject) Instantiate(Resources.Load(PATH), data.position, Quaternion.identity)).GetComponent<Joint>();
		var renderer = joint.GetComponent<MeshRenderer>();
		renderer.sortingOrder = 2;
		joint.JointData = data;
		return joint;
	}

	public override void Start () {
		base.Start();

		resetPosition = transform.position;
		resetRotation = transform.rotation;

		body = GetComponent<Rigidbody>();

		body.mass = JointData.weight;
	}

	/// <summary>
	/// Moves the joint to the specified position and updates all of the connected objects.
	/// </summary>
	public void MoveTo(Vector3 newPosition) {

		transform.position = newPosition;

		foreach (var connectedBone in joints.Keys) {
			connectedBone.RefreshBonePlacement();
		}
	}

	public void Connect(Bone bone) {

		HingeJoint joint = gameObject.AddComponent<HingeJoint>();
		//ConfigurableJoint joint = gameObject.AddComponent<ConfigurableJoint>();
		joint.anchor = Vector3.zero;
		joint.axis = new Vector3(0, 0, 1);
		joint.autoConfigureConnectedAnchor = true;
		joint.useSpring = false;
		//var spring = joint.spring;
		//spring.spring = 1000f;
		//joint.spring = spring;
		//joint.connectedAnchor = new Vector3(0, 1.14f, 0);
		joint.enablePreprocessing = true;
		joint.enableCollision = false;

		joint.connectedBody = bone.gameObject.GetComponent<Rigidbody>();

		joints.Add(bone, joint);
	}

	/** Disconnects the bone from the joint. */
	public void Disconnect(Bone bone) {

		if (!joints.ContainsKey(bone)) return;
		
		UnityEngine.Joint joint = joints[bone];
		Destroy(joint);
		if (!iterating)
			joints.Remove(bone);
	}
		
	/// <summary>
	/// Deletes the joint and all attached objects from the scene.
	/// </summary>
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

	public override void PrepareForEvolution() {

		body = GetComponent<Rigidbody>();
		body.isKinematic = false;
	}
		
	void OnTriggerEnter(Collider collider) {

		if (collider.CompareTag("Ground")) {
			isCollidingWithGround = true;
		} else if (collider.CompareTag("Obstacle")) {
			isCollidingWithObstacle = true;
		}
	}

	void OnTriggerExit(Collider collider) {

		//if (collider.tag.ToUpper() == "OBSTACLE") 
		if (collider.CompareTag("Obstacle")) 
			isCollidingWithObstacle = false;
		else
			isCollidingWithGround = false;	
	}
}
