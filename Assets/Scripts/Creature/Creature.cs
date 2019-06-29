using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct BasicBrainInputs {

	public float DistanceFromFloor { get; set; }
	public float VelocityX { get; set; }
	public float VelocityY { get; set; }
	public float AngularVelocity { get; set; }
	public float PointsTouchingGroundCount { get; set; }
	public float Rotation { get; set; }
}

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
		get { return obstacle; }
		set { obstacle = value; }	
	}
	private GameObject obstacle;

	public PhysicsScene PhysicsScene { get; set; }

	private static LayerMask groundLayerMask = 1 << 9; //LayerMask.NameToLayer("Ground");

	private float maxJumpingHeight;

	//private Vector3 currentLowest;

	//public bool DEBUG = false;

	void Update () {

		//maxJumpingHeight = Mathf.Max(maxJumpingHeight, DistanceFromGround());

		if (Alive) {
			maxJumpingHeight = Mathf.Max(maxJumpingHeight, DistanceFromFlatFloor());
		}
		//currentLowest = GetLowestPoint();
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
		foreach (Muscle muscle in muscles) {
			muscle.DeleteAndAddLineRenderer();
		}
	}

	public void RefreshMuscleContractionVisibility(bool visible) {
		foreach (Muscle muscle in muscles) {
			muscle.ShouldShowContraction = visible;
		}
	}

	private void UpdateMuscleAliveness(bool alive) {
		foreach (Muscle muscle in muscles) {
			muscle.living = alive;
		}
	}

	/// <summary>
	/// Calculates all of the basic brain inputs at once for performance reasons.
	/// </summary>
	/// <returns>The basic brain inputs.</returns>
	public BasicBrainInputs CalculateBasicBrainInputs() {

		var minJointY = joints[0].transform.position.y;
		var maxJointY = joints[0].transform.position.y;
		var velocityX = 0f;
		var velocityY = 0f;
		var angularVelZ = 0f;
		var jointsCountTouchingGround = 0f;
		var rotationZ = 0f;

		var jointCount = joints.Count;
		var boneCount = bones.Count;

		//var jointRadius = joints[0].GetComponent<Collider>().bounds.size.y / 2;

		for (int i = 0; i < jointCount; i++) {
			
			var joint = joints[i];
			var jointPos = joint.transform.position;

			// Determine lowest and highest joints
			if (jointPos.y > maxJointY)
				maxJointY = jointPos.y;
			else if (jointPos.y < minJointY)
				minJointY = jointPos.y;

			// Accumulate the velocity
			velocityX += joint.Body.velocity.x;
			velocityY += joint.Body.velocity.y;

			// Check if the joint is touching the ground
			jointsCountTouchingGround += joint.isCollidingWithGround ? 1f : 0f;
		}

		for (int i = 0; i < boneCount; i++) {
			var bone = bones[i];

			angularVelZ += bone.Body.angularVelocity.z;
			// Not actually the rotation value I originally wanted to have 
			// but I have to keep this for the sake of not breaking existing
			// creature behaviour
			rotationZ += bone.transform.rotation.z;
		}

		var distFromFloor = minJointY;
		velocityX /= jointCount;
		velocityY /= jointCount;
		angularVelZ /= boneCount;
		rotationZ /= boneCount;


		return new BasicBrainInputs() {
			DistanceFromFloor = distFromFloor,
			VelocityX = velocityX,
			VelocityY = velocityY,
			AngularVelocity = angularVelZ,
			PointsTouchingGroundCount = jointsCountTouchingGround,
			Rotation = rotationZ
		};
	}

	public float DistanceFromGround() {
		RaycastHit hit;

		if (PhysicsScene.Raycast(GetLowestPoint(), Vector3.down, out hit, Mathf.Infinity, groundLayerMask)) {
			return hit.distance;
		}

		// Debug.Log(hit.distance);

		return 0f;
	}

	public float DistanceFromFlatFloor() {

		float min = joints[0].center.y;

		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			var joint = joints[i];
			float height = joint.center.y;// - joint.GetComponent<Collider>().bounds.size.y / 2;
			min = height < min ? height : min; 
		}

		return min - floorHeight;
	}

	/** Returns the velocity of the creature */
	public Vector3 GetVelocity() {

		if (joints.Count == 0) return Vector3.zero;

		//calculate the average velocity of the joints.
		//Vector3 velocity = Vector3.zero;
		var velX = 0f;
		var velY = 0f;
		var velZ = 0f;

		int jointsCount = joints.Count;

		for (int i = 0; i < jointsCount; i++) {
			var jointVel = joints[i].Body.velocity;
			velX += jointVel.x;
			velY += jointVel.y;
			velZ += jointVel.z;
		}

		velX /= jointsCount;
		velY /= jointsCount;
		velZ /= jointsCount;

		return new Vector3(velX, velY, velZ);
	}

	public Vector3 GetAngularVelocity() {

		if (bones.Count == 0) return Vector3.zero;

		//calculate the average velocity of the bones.
		Vector3 velocity = Vector3.zero;

		var boneCount = bones.Count;
		for (int i = 0; i < boneCount; i++) {
			velocity += bones[i].GetComponent<Rigidbody>().angularVelocity;
		}

		velocity.x /= bones.Count;
		velocity.y /= bones.Count;
		velocity.z /= bones.Count;

		return velocity;
	}

	public float GetNumberOfPointsTouchingGround() {

		int count = 0;
		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			count += joints[i].isCollidingWithGround ? 1 : 0 ;
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

		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			if (joints[i].isCollidingWithObstacle) {
				collidedJoints.Add(joints[i]);
			}
		}
		//print(string.Format("Percentage of collided joints: {0}%", ((float)collidedJoints.Count/joints.Count) * 100f)); 
	}

	public float GetRotation() {

		if (bones.Count == 0) return 0;

		float rotation = 0;

		var boneCount = bones.Count;
		for (int i = 0; i < boneCount; i++) {
			rotation += bones[i].transform.rotation.z;
		}
			
		return rotation / boneCount;
	}

	public float GetXPosition() {

		float total = 0;

		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			total += joints[i].transform.position.x;
		}

		return jointsCount == 0 ? 0 : total / jointsCount ;
	}

	public float GetYPosition() {

		float total = 0;

		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			total += joints[i].transform.position.y;
		}

		return jointsCount == 0 ? 0 : total / jointsCount ;
	}

	public Vector3 GetLowestPoint() {

		Vector3 min = joints[0].transform.position;

		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			var jointPos = joints[i].transform.position;

			if (min.y > jointPos.y) {
				min = jointPos;
			}
		}

		return min;
	}

	public Vector3 GetHighestPoint() {

		Vector3 max = joints[0].transform.position;

		var jointsCount = joints.Count;
		for (int i = 0; i < jointsCount; i++) {
			var jointPos = joints[i].transform.position;

			if (max.y < jointPos.y) {
				max = jointPos;
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
		stats.weight = joints.Select(x => x.JointData.weight).Sum() + 2 * bones.Select(x => x.BoneData.weight).Sum();
		stats.maxJumpingHeight = maxJumpingHeight;

		return stats;
	}

	public void SetOnVisibleLayer() {

		foreach (var bone in bones) {
			if (bone == null) continue;
			bone.gameObject.layer = LayerMask.NameToLayer("VisibleCreature");
			bone.muscleJoint.gameObject.layer = LayerMask.NameToLayer("VisibleCreature");
		}

		foreach (var joint in joints) {
			if (joint == null) continue;
			joint.gameObject.layer = LayerMask.NameToLayer("VisibleJoint");
		}

		foreach (var muscle in muscles) {
			if (muscle == null) continue;
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

	public void Reset() {

		for (int i = 0; i < bones.Count; i++)
			bones[i].Reset();

		for (int i = 0; i < joints.Count; i++)
			joints[i].Reset();

		for (int i = 0; i < muscles.Count; i++)
			muscles[i].Reset();
	}

	public void RemoveMuscleColliders() {
		for (int i = 0; i < muscles.Count; i++) {
			muscles[i].RemoveCollider();
		}
	}
}

