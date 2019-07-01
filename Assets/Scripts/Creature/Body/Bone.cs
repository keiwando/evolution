using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bone : BodyComponent {

	private const string PATH = "Prefabs/Bone";

	public Joint startingJoint;
	public Joint endingJoint;

	public Vector3 startingPoint {
		get { return startingJoint.center; }
	}

	public Vector3 endingPoint {
		get { return endingJoint.center; }	
	}

	public MuscleJoint muscleJoint;

	public BoneData BoneData { get; set; }

	private Vector3 resetPosition;
	private Quaternion resetRotation;

	public Rigidbody Body {
		get { return body; }
	}
	private Rigidbody body;

	public static Bone CreateAtPoint(Vector3 point, BoneData data) {
		
		var bone = ((GameObject) Instantiate(Resources.Load(PATH), point, Quaternion.identity)).GetComponent<Bone>();
		bone.BoneData = data;
		return bone;
	}
	
	// Use this for initialization
	public override void Start () {
		base.Start();

		resetPosition = transform.position;
		resetRotation = transform.rotation;

		body = GetComponent<Rigidbody>();

		body.mass = 2 * BoneData.weight - 1;
	}

	public void Reset() {
		transform.SetPositionAndRotation(resetPosition, resetRotation);
		body.velocity = Vector3.zero;
		body.angularVelocity = Vector3.zero;
		muscleJoint.Body.velocity = Vector3.zero;
		muscleJoint.Body.angularVelocity = Vector3.zero;
		muscleJoint.transform.localPosition = Vector3.zero;
		muscleJoint.transform.localRotation = Quaternion.identity;
	}

	/** Places the bone between the specified points. (Points flattened to 2D) */
	private void PlaceBetweenPoints(Vector3 start, Vector3 end, float width) {

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		PlaceBetweenPoints3D(start, end, width);
	}

	private void PlaceBetweenPoints3D(Vector3 start, Vector3 end, float width) {

		Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		transform.position = position;
		transform.up = offset;
		transform.localScale = scale;
	}

	/// <summary>
	/// Places the bone between the two joints.
	/// </summary>
	public void RefreshBonePlacement() {

		if (startingJoint == null || endingJoint == null) return;

		var width = transform.localScale.z;
		var jPos1 = startingJoint.transform.position;
		var jPos2 = endingJoint.transform.position;

		PlaceBetweenPoints(jPos1, jPos2, width);
	}

	public void RefreshBonePlacement3D() {

		if (startingJoint == null || endingJoint == null) return;

		var width = transform.localScale.z;
		var jPos1 = startingJoint.transform.position;
		var jPos2 = endingJoint.transform.position;

		PlaceBetweenPoints3D(jPos1, jPos2, width);
	}

	/** Connects the gameobject to the starting end endingJoint */
	public void ConnectToJoints() {

		if (startingJoint == null || endingJoint == null) return;

		startingJoint.Connect(this);
		endingJoint.Connect(this);
	}

	/** Deletes the bone and the connected muscles from the scene. */
	public override void Delete() {
		base.Delete();
		// Delete the connected muscles
		muscleJoint.deleteAllConnected();

		// Disconnect from the joints
		startingJoint.Disconnect(this);
		endingJoint.Disconnect(this);

		Destroy(gameObject);

	}

	public override void PrepareForEvolution () {

		body = GetComponent<Rigidbody>();

		body.isKinematic = false;
	}
}
