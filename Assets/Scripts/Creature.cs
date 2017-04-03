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
				PrepareForEvolution();
			} 
			alive = value;
			SetKinematic(!alive);
			UpdateMuscleAliveness(alive);
		}
	}

	/// <summary>
	/// The y-position of the floor.
	/// </summary>
	public float FloorHeight {
		set { floorHeight = value; }
		get { return floorHeight; }
	}
	private float floorHeight = 0;

	public GameObject Obstacle {
		set {
			obstacle = value;
		}	
	}
	private GameObject obstacle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetKinematic(bool enabled) {

		foreach (Joint joint in joints) {
			joint.GetComponent<Rigidbody>().isKinematic = enabled;
		}

		foreach (Bone bone in bones) {
			bone.GetComponent<Rigidbody>().isKinematic = enabled;
		}
	}

	public void PrepareForEvolution() {

		foreach (Joint joint in joints) {
			joint.PrepareForEvolution();
		}

		foreach (Bone bone in bones) {
			bone.PrepareForEvolution();
		}

		foreach (Muscle muscle in muscles) {
			muscle.PrepareForEvolution();
		}

		alive = true;
	}

	public void RefreshLineRenderers(){
		foreach(Muscle muscle in muscles) {
			muscle.DeleteAndAddLineRenderer();
		}
	}

	private void UpdateMuscleAliveness(bool alive) {
		foreach (Muscle muscle in muscles) {
			muscle.living = alive;
		}
	}

	public float DistanceFromGround() {

		float min = joints[0].center.y;

		foreach (Joint joint in joints) {
			float height =  joint.center.y - joint.GetComponent<Collider>().bounds.size.y / 2;
			min = height < min ? height : min; 
		}

		return min - floorHeight;
	}

	/** Returns the velocity of the creature */
	public Vector3 GetVelocity() {

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

	public Vector3 GetAngularVelocity() {

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

	public float GetNumberOfPointsTouchingGround() {

		int count = 0;
		foreach (Joint joint in joints) {
			count += joint.isCollidingWithGround ? 1 : 0 ;
		}

		return count;
	}

	public long GetNumberOfObstacleCollisions() {

		long count = 0;
		foreach (Joint joint in joints) {
			count += joint.isCollidingWithObstacle ? 1 : 0 ;
		}

		return count;
	}

	public float GetRotation() {

		if (bones.Count == 0) return 0;

		float rotation = 0;

		foreach( Bone bone in bones) {
			rotation += bone.transform.rotation.z;
		}

		return rotation / bones.Count;
	}

	public float GetXPosition() {

		float total = 0;

		foreach (Joint joint in joints) {
			total += joint.transform.position.x;
		}

		return joints.Count == 0 ? 0 : total / joints.Count ;
	}

	public float GetYPosition() {

		float total = 0;

		foreach (Joint joint in joints) {
			total += joint.transform.position.y;
		}

		return joints.Count == 0 ? 0 : total / joints.Count ;
	}

	public float GetDistanceFromObstacle() {
		//return Vector3.Distance(new Vector3(GetXPosition(), GetYPosition(), 0f), obstacle.transform.position);
		float minDistance = float.PositiveInfinity;

		if (obstacle == null) return minDistance;

		var obstaclePos = obstacle.transform.position;
		var distances = new float[joints.Count];

		for (int i = 0; i < distances.Length; i++) {
			distances[i] = Vector3.Distance(joints[i].center, obstaclePos);
		}

		return Mathf.Min(distances);
	}
}

