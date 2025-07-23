using UnityEngine;
using System;
using System.Collections.Generic;

public class Bone : BodyComponent {

	private const string PATH = "Prefabs/Bone";

	public Joint startingJoint;
	public Joint endingJoint;

	public Vector3 startingPoint => startingJoint.center;
	public Vector3 endingPoint => endingJoint.center;

	public Vector3 Center => BoneData.legacy ? legacyWeightObj.transform.position : transform.position;

	private List<Muscle> connectedMuscles = new List<Muscle>();
	public GameObject legacyWeightObj => _legacyWeightObj;
	private GameObject _legacyWeightObj;

	public BoneData BoneData { get; set; }

	private Vector3 resetPosition;
	private Quaternion resetRotation;

	public Rigidbody Body => body;
	private Rigidbody body;

	SpriteRenderer wingSpriteRenderer;

	public static Bone CreateAtPoint(Vector3 point, BoneData data) {
		
		var bone = ((GameObject) Instantiate(Resources.Load(PATH), point, Quaternion.identity)).GetComponent<Bone>();
		bone.BoneData = data;

		var body = bone.GetComponent<Rigidbody>();
		bone.body = body;
		if (data.legacy) {
			body.mass = 2 * data.weight - 1;
			// Add a second weight representing the old muscle joint object
			var weightObj = new GameObject();
			var weightBody = weightObj.AddComponent<Rigidbody>();
			weightBody.mass = 1f;
			weightBody.angularDamping = 0.05f;
			var fixedJoint = weightObj.AddComponent<FixedJoint>();
			fixedJoint.connectedBody = body;
			fixedJoint.enablePreprocessing = false;
			weightObj.transform.parent = bone.transform;
			weightObj.transform.localPosition = Vector3.zero;
			weightObj.transform.localScale = Vector3.one;
			weightBody.constraints = RigidbodyConstraints.FreezePositionZ;
			bone._legacyWeightObj = weightObj;

		} else {
			
			body.mass = data.weight;
		}

		bone.wingSpriteRenderer = bone.GetComponentInChildren<SpriteRenderer>();

		bone.UpdateFeatherVisibility();

		return bone;
	}
	
	public override void Start () {
		base.Start();

		resetPosition = transform.position;
		resetRotation = transform.rotation;
	}

	public void FixedUpdate() {

		UpdateFeatherVisibility();

		if (BoneData.inverted != (transform.localScale.x < 0f)) {
			var scale = transform.localScale;
			scale.x *= -1f;
			transform.localScale = scale;
		}

		if (!BoneData.isWing) { 
			return; 
		}
		var localBoneVelocity = transform.InverseTransformDirection(body.linearVelocity);
		if (localBoneVelocity.magnitude < 1.0) { return; }
		var localAngle = Vector3.SignedAngle(localBoneVelocity, Vector3.up, Vector3.forward);
		if (BoneData.inverted != (localAngle < 0)) {
			// We make it easier to move the wing up by not generating any opposing force
			return;
		}
		// The length of the wing should also contribute here!
		var maxForce = 30.0f * transform.localScale.y;
		var angleFactor = 1.0f - (Math.Abs(Math.Abs(localAngle) - 90.0f) / 90.0f);
		var velocityFactor = localBoneVelocity.magnitude; // (float)Math.Pow(localBoneVelocity.magnitude / 10.0f, 3.0f);
		var force = velocityFactor * -Math.Sign(localAngle) * maxForce * angleFactor;

		var forceVec = new Vector3(force, 0.0f, 0.0f);
		body.AddRelativeForce(forceVec);
		// Debug.DrawRay(transform.position, transform.TransformDirection(localBoneVelocity), Color.red, 0, false);
		// Debug.DrawRay(transform.position, transform.TransformDirection(-0.1f * forceVec), Color.green, 0, false);

		// Debug.Log("velocity.magnitude: " + localBoneVelocity.magnitude + " localAngle: " + localAngle + " angleFactor: " + angleFactor + " force: " + force);
	}

	private void UpdateFeatherVisibility() {
		if (body.isKinematic && wingSpriteRenderer != null) {
			// We are in the editor, refresh the wing sprite visibility when the 
			if (!BoneData.isWing && wingSpriteRenderer.enabled) {
				wingSpriteRenderer.enabled = false;
			} else if (BoneData.isWing && !wingSpriteRenderer.enabled) {
				wingSpriteRenderer.enabled = true;
			}
		}
	}

	/** Places the bone between the specified points. (Points flattened to 2D) */
	private void PlaceBetweenPoints(Vector3 start, Vector3 end, float width) {

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
		DeleteAllConnectedMuscles();

		// Disconnect from the joints
		startingJoint.Disconnect(this);
		endingJoint.Disconnect(this);

		Destroy(gameObject);

		// TODO: Delete connected decorations
	}

	private void DeleteAllConnectedMuscles() {

		var connected = new List<Muscle>(connectedMuscles);
		foreach (Muscle muscle in connected) {
			if (muscle != null) {
				muscle.Delete();
			}
		}
		connectedMuscles.Clear();
	}

	public void Connect(Muscle muscle) {
		connectedMuscles.Add(muscle);
	}

	public void Disconnect(Muscle muscle) {
		connectedMuscles.Remove(muscle);
	}

	public override void PrepareForEvolution () {
		
		body = GetComponent<Rigidbody>();
		body.isKinematic = false;

		if (!BoneData.isWing && wingSpriteRenderer != null) {
			Destroy(wingSpriteRenderer.gameObject);
		}
	}

	public override int GetId() {
		return BoneData.id;
	}

	public void SetLayer(LayerMask layer) {
		this.gameObject.layer = layer;
		if (this._legacyWeightObj != null) {
			this._legacyWeightObj.layer = layer;
		}
		if (this.wingSpriteRenderer != null) {
			this.wingSpriteRenderer.gameObject.layer = layer;
		}
	}

	public float GetWeight() {
		return body.mass + (_legacyWeightObj != null ? 1f : 0f);
	}
}
