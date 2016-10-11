using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Creature : MonoBehaviour {

	public List<Joint> joints;
	public List<Bone> bones;
	public List<Muscle> muscles;

	public Brain brain;

	private bool alive;
	public bool Alive { 
		get { return alive; } 
		set {
			if (!alive) {
				prepareForEvolution();
			} 
			alive = value;
			setKinematic(!alive);
			updateMuscleAliveness(alive);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setKinematic(bool enabled) {

		foreach (Joint joint in joints) {
			joint.GetComponent<Rigidbody>().isKinematic = enabled;
		}

		foreach (Bone bone in bones) {
			bone.GetComponent<Rigidbody>().isKinematic = enabled;
		}
	}

	public void prepareForEvolution() {

		foreach (Joint joint in joints) {
			joint.prepareForEvolution();
		}

		foreach (Bone bone in bones) {
			bone.prepareForEvolution();
		}

		foreach (Muscle muscle in muscles) {
			muscle.prepareForEvolution();
		}

		alive = true;
	}

	public void refreshLineRenderers(){
		foreach(Muscle muscle in muscles) {
			muscle.deleteAndAddLineRenderer();
		}
	}

	private void updateMuscleAliveness(bool alive) {
		foreach (Muscle muscle in muscles) {
			muscle.living = alive;
		}
	}

	public float distanceFromGround() {

		float[] heights = new float[joints.Count];

		int i = 0;
		foreach (Joint joint in joints) {
			heights[i] = joint.center.y - joint.GetComponent<Collider>().bounds.size.y / 2;	
		}

		return Mathf.Min( heights );
	}

	/** Returns the velocity of the creature */
	public Vector3 getVelocity() {

		if (joints.Count == 0) return Vector3.zero;

		//calculate the average velocity of the joints.
		Vector3 velocity = Vector3.zero;

		foreach (Joint joint in joints) {
			velocity += joint.GetComponent<Rigidbody>().velocity;
		}

		velocity.x /= joints.Count;
		velocity.y /= joints.Count;
		velocity.z /= joints.Count;

		return velocity;
	}

	public Vector3 getAngularVelocity() {

		if (joints.Count == 0) return Vector3.zero;

		//calculate the average velocity of the joints.
		Vector3 velocity = Vector3.zero;

		foreach (Joint joint in joints) {
			velocity += joint.GetComponent<Rigidbody>().angularVelocity;
		}

		velocity.x /= joints.Count;
		velocity.y /= joints.Count;
		velocity.z /= joints.Count;

		return velocity;
	}

	public float getNumberOfPointsTouchingGround() {

		int count = 0;
		foreach (Joint joint in joints) {
			count += joint.isColliding ? 1 : 0 ;
		}

		return count;
	}

	public float getRotation() {

		if (bones.Count == 0) return 0;

		float rotation = 0;

		foreach( Bone bone in bones) {
			rotation += bone.transform.rotation.z;
		}

		return rotation / bones.Count;
	}

	public float getXPosition() {

		float total = 0;

		foreach (Joint joint in joints) {
			total += joint.transform.position.x;
		}

		return joints.Count == 0 ? 0 : total / joints.Count ;
	}

	public float getYPosition() {

		float total = 0;

		foreach (Joint joint in joints) {
			total += joint.transform.position.y;
		}

		return joints.Count == 0 ? 0 : total / joints.Count ;
	}
}
