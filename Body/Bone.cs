using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bone : BodyComponent {

	private const string PATH = "Prefabs/Bone";

	public Joint startingJoint;
	public Joint endingJoint;

	public Vector3 startingPoint {
		get {
			return startingJoint.center;
		}
	}

	public Vector3 endingPoint {
		get {
			return endingJoint.center;
		}	
	}

	public MuscleJoint muscleJoint;

	private static Bone InstantiateAtPoint(Vector3 point) {
		return ((GameObject) Instantiate(Resources.Load(PATH), point, Quaternion.identity)).GetComponent<Bone>();
	}

	public static Bone CreateAtPoint(Vector3 point) {
		ID_COUNTER++;
		var bone = Bone.InstantiateAtPoint(point);
		bone.ID = ID_COUNTER;
		bone.muscleJoint.ID = bone.ID;
		return bone;
	}

	public static Bone CreateFromString(string data, List<Joint> joints) {
		//print(data);
		var parts = data.Split('%');
		var boneID = int.Parse(parts[0]);
		var jointID1 = int.Parse(parts[1]);
		var jointID2 = int.Parse(parts[2]);

		// Format: ID - startingJoint.ID - endingJoint.ID
		var bone = Bone.InstantiateAtPoint(Vector3.zero);
		bone.ID = boneID;
		bone.muscleJoint.ID = bone.ID;
		ID_COUNTER = Mathf.Max(ID_COUNTER, bone.ID);

		// attach to joints
		foreach (var joint in joints) {

			if (joint.ID == jointID1) {
				bone.startingJoint = joint;	
			} else if (joint.ID == jointID2) {
				bone.endingJoint = joint;
			}
		}

		CreatureBuilder.PlaceConnectionBetweenPoints(bone.gameObject, bone.startingPoint, bone.endingPoint, CreatureBuilder.CONNECTION_WIDHT);
		bone.ConnectToJoints();

		return bone;
	}

	// Use this for initialization
	public override void Start () {
		base.Start();

	}

	/** Places the bone between the specified points. (Points flattened to 2D) */
	private void PlaceBetweenPoints(Vector3 start, Vector3 end, float width) {

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		/*Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		transform.position = position;
		transform.up = offset;
		transform.localScale = scale;*/
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
		
		GetComponent<Rigidbody>().isKinematic = false;
	}

	public override string GetSaveString () {
		
		return string.Format("{0}%{1}%{2}", ID, startingJoint.ID, endingJoint.ID);
	}

}
