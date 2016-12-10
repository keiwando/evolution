using UnityEngine;
using System.Collections;

public class Bone : BodyComponent {

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

	// Use this for initialization
	public override void Start () {
		base.Start();

	}

	/** Connects the gameobject to the starting end endingJoint */
	public void connectToJoints() {

		if (startingJoint == null || endingJoint == null) return;

		startingJoint.connect(this);
		endingJoint.connect(this);
	}

	/** Deletes the bone and the connected muscles from the scene. */
	public override void delete() {
		base.delete();
		// Delete the connected muscles
		muscleJoint.deleteAllConnected();

		// Disconnect from the joints
		startingJoint.disconnect(this);
		endingJoint.disconnect(this);

		Destroy(gameObject);

	}

	public override void prepareForEvolution ()
	{
		GetComponent<Rigidbody>().isKinematic = false;
	}

	/*
	public MuscleJoint getClosestMuscleJoint(Vector3 mousePosition){

		float distanceToUpper = (upperMuscleJoint.position - mousePosition).magnitude;
		float distanceToLower = (lowerMuscleJoint.position - mousePosition).magnitude;

		return distanceToLower < distanceToUpper ? lowerMuscleJoint : upperMuscleJoint;
	}*/

}
