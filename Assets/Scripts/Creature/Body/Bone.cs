using UnityEngine;
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
			weightBody.angularDrag = 0.05f;
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

		return bone;
	}
	
	public override void Start () {
		base.Start();

		resetPosition = transform.position;
		resetRotation = transform.rotation;
	}

	public void Reset() {
		transform.SetPositionAndRotation(resetPosition, resetRotation);
		body.velocity = Vector3.zero;
		body.angularVelocity = Vector3.zero;

		if (BoneData.legacy) {
			var weightBody = _legacyWeightObj.GetComponent<Rigidbody>();
			weightBody.velocity = Vector3.zero;
			weightBody.angularVelocity = Vector3.zero;
			_legacyWeightObj.transform.localPosition = Vector3.zero;
			_legacyWeightObj.transform.localRotation = Quaternion.identity;
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
		DeleteAllConnectedMuscles();

		// Disconnect from the joints
		startingJoint.Disconnect(this);
		endingJoint.Disconnect(this);

		Destroy(gameObject);

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
	}

	public void SetLayer(LayerMask layer) {
		this.gameObject.layer = layer;
		if (this._legacyWeightObj != null) {
			this._legacyWeightObj.layer = layer;
		}
	}

	public float GetWeight() {
		return body.mass + (_legacyWeightObj != null ? 1f : 0f);
	}
}
