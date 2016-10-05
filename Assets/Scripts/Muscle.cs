using UnityEngine;
using System.Collections;

public class Muscle : Hoverable {

	public MuscleJoint startingJoint;
	public MuscleJoint endingJoint;

	public Vector3 startingPoint {
		get {
			return startingJoint.position;
		}
	}

	public Vector3 endingPoint {
		get {
			return endingJoint.position;
		}	
	}

	private SpringJoint spring;

	private float CONTRACTION_FACTOR = 0.2f;

	// Use this for initialization
	public override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/** Connects the gameobject to the starting end endingJoint */
	public void connectToJoints() {

		if (startingJoint == null || endingJoint == null) return;

		startingJoint.connect(this);
		endingJoint.connect(this);

		// connect the musclejoints with a spring joint
		spring = startingJoint.gameObject.AddComponent<SpringJoint>();
		spring.minDistance = 0;
		spring.maxDistance = 0;
		//spring.autoConfigureConnectedAnchor = true;
		spring.anchor = startingJoint.position;
		spring.connectedAnchor = endingJoint.position;
		spring.connectedBody = endingJoint.GetComponent<Rigidbody>();

		spring.enablePreprocessing = true;
		spring.enableCollision = false;
	}

	/** Contracts the muscle. */
	public void contract() {
		
	}

	/** Expands the muscle. */
	public void expand() {
		
	}
}
