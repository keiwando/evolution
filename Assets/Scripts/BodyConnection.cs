using UnityEngine;
using System.Collections;

public class BodyConnection : Hoverable {

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

}
