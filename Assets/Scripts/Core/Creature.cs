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

	private LayerMask groundDistanceLayerMask;

	private float maxJumpingHeight;

	//public bool DEBUG = false;

	// Use this for initialization
	void Start () {
		groundDistanceLayerMask = LayerMask.NameToLayer("Ground");
	}
	
	// Update is called once per frame
	void Update () {

		maxJumpingHeight = Mathf.Max(maxJumpingHeight, DistanceFromGround());
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
		RaycastHit hit;

		if(Physics.Raycast(GetLowestPoint(), Vector3.down, out hit, groundDistanceLayerMask)) {
			
			if (hit.collider.gameObject.tag.ToUpper() == "GROUND") {
				return hit.distance;
			}
		}

		return 0f;
	}

	public float DistanceFromFlatFloor() {

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

		if (bones.Count == 0) return Vector3.zero;

		//calculate the average velocity of the bones.
		Vector3 velocity = Vector3.zero;

		foreach (var bone in bones) {
			velocity += bone.GetComponent<Rigidbody>().angularVelocity;
		}

		velocity.x /= bones.Count;
		velocity.y /= bones.Count;
		velocity.z /= bones.Count;

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

	public void AddObstacleCollidingJointsToSet(HashSet<Joint> collidedJoints) {
		foreach (var joint in joints) {
			if (joint.isCollidingWithObstacle) {
				collidedJoints.Add(joint);
			}
		}
		//print(string.Format("Percentage of collided joints: {0}%", ((float)collidedJoints.Count/joints.Count) * 100f)); 
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

	public Vector3 GetLowestPoint() {

		Vector3 min = joints[0].transform.position;

		foreach (var joint in joints) {
			if (min.y > joint.transform.position.y) {
				min = joint.transform.position;
			}
		}

		return min;
	}

	public Vector3 GetHighestPoint() {

		Vector3 max = joints[0].transform.position;

		foreach (var joint in joints) {
			if (max.y < joint.transform.position.y) {
				max = joint.transform.position;
			}
		}

		return max;
	}

	public float GetDistanceFromObstacle() {
		
		float minDistance = float.PositiveInfinity;

		if (obstacle == null) return minDistance;

		var obstaclePos = obstacle.transform.position;
		var distances = new float[joints.Count];

		for (int i = 0; i < distances.Length; i++) {
			distances[i] = Vector3.Distance(joints[i].center, obstaclePos);
		}

		return Mathf.Min(distances);
	}

	public CreatureStats GetStatistics() {

		var stats = new CreatureStats();

		brain.EvaluateFitness();

		stats.fitness = brain.fitness;
		stats.horizontalDistanceTravelled = GetXPosition();
		stats.verticalDistanceTravelled = GetYPosition();
		stats.averageSpeed = Mathf.Sqrt(Mathf.Pow(stats.horizontalDistanceTravelled / brain.SimulationTime, 2) + Mathf.Pow(stats.verticalDistanceTravelled / brain.SimulationTime, 2));
		stats.numberOfBones = bones.Count;
		stats.numberOfMuscles = muscles.Count;
		stats.simulationTime = Mathf.RoundToInt(brain.SimulationTime);
		stats.weight = joints.Count + 2 * bones.Count;
		stats.maxJumpingHeight = maxJumpingHeight;

		return stats;
	}

	public void SetOnVisibleLayer() {

		foreach (var bone in bones) {
			bone.gameObject.layer = LayerMask.NameToLayer("VisibleCreature");
			bone.muscleJoint.gameObject.layer = LayerMask.NameToLayer("VisibleCreature");
		}

		foreach (var joint in joints) {
			joint.gameObject.layer = LayerMask.NameToLayer("VisibleJoint");
		}

		foreach (var muscle in muscles) {
			muscle.gameObject.layer = LayerMask.NameToLayer("VisibleCreature");
		}

		gameObject.layer = LayerMask.NameToLayer("VisibleCreature");
	}

	public void SetOnInvisibleLayer() {

		foreach (var bone in bones) {
			bone.gameObject.layer = LayerMask.NameToLayer("Creature");
			bone.muscleJoint.gameObject.layer = LayerMask.NameToLayer("Creature");
		}

		foreach (var joint in joints) {
			joint.gameObject.layer = LayerMask.NameToLayer("Joint");
		}

		foreach (var muscle in muscles) {
			muscle.gameObject.layer = LayerMask.NameToLayer("Creature");
		}

		gameObject.layer = LayerMask.NameToLayer("Creature");
	}

	public void SetOnBestCreatureLayer() {

		foreach (var bone in bones) {
			bone.gameObject.layer = LayerMask.NameToLayer("BestCreatureCreature");
			bone.muscleJoint.gameObject.layer = LayerMask.NameToLayer("BestCreatureCreature");
		}

		foreach (var joint in joints) {
			joint.gameObject.layer = LayerMask.NameToLayer("BestCreatureJoint");
		}

		foreach (var muscle in muscles) {
			muscle.gameObject.layer = LayerMask.NameToLayer("BestCreatureCreature");
		}

		gameObject.layer = LayerMask.NameToLayer("BestCreatureCreature");
	}

	void OnDestroy() {
		//print("Creature script destroyed");
	}
}

