using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

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

		public ISceneContext SceneContext { get; set; }

		public PhysicsScene PhysicsScene { get; set; }

		public bool usesLegacyRotationCalculation { get; set; } = false;

		public Vector3 InitialPosition { get; private set; }

		private float maxJumpingHeight;

		//public bool DEBUG = false;

		void Start() {
			this.InitialPosition = new Vector3(
				GetXPosition(),
				GetYPosition()
			);
		}

		void Update () {

			if (Alive) {
				var jumpHeight = GetLowestPoint().y - InitialPosition.y;
				maxJumpingHeight = Mathf.Max(maxJumpingHeight, jumpHeight);
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

				if (usesLegacyRotationCalculation) {
					// Not actually the rotation value I originally wanted to have 
					// but I have to keep this for the sake of not breaking legacy
					// creature behaviour
					rotationZ += bone.transform.rotation.z;
				} else {
					rotationZ += (bone.transform.rotation.eulerAngles.z - 180f) * 0.002778f;
				}
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

		public float RaycastDistance(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity) {

			return RaycastDistance(origin, direction,
				(1 << SceneContext.GetStaticForegroundLayer()) | (1 << SceneContext.GetDynamicForegroundLayer()),
				maxDistance
			);
		}

		public float RaycastDistance(Vector3 origin, Vector3 direction, int layerMask, float maxDistance = Mathf.Infinity) {
			RaycastHit hit;

			if (PhysicsScene.Raycast(
				origin, direction,
				out hit, maxDistance,
				layerMask)
			) {
				return hit.distance;
			}

			return 0f;
		}

		public float DistanceFromGround(Vector3 position) {
			RaycastHit hit;
			if (PhysicsScene.Raycast(
				position, 
				Vector3.down, 
				out hit, Mathf.Infinity, 
				1 << SceneContext.GetStaticForegroundLayer())
			) {
				return hit.distance;
			}

			return 0f;
		}

		public float DistanceFromGround() {
			return DistanceFromGround(GetLowestPoint());
		}

		/// <summary>
		/// Returns the smallest distance of the lowest 5 joints from the ground.
		/// Should be used if the ground is uneven.
		/// Does not guarantee that there is no other joint with a smaller distance to the ground.
		/// </summary>
		public float SemiSafeDistanceFromGround() {
			int jointCount = System.Math.Min(5, joints.Count);
			var sortedJoints = new List<Joint>(joints);
			sortedJoints.Sort((lhs, rhs) => lhs.center.y.CompareTo(rhs.center.y));
			float minDistance = float.MaxValue;
			for (int i = 0; i < jointCount; i++) {
				var distance = DistanceFromGround(sortedJoints[i].center);
				minDistance = System.Math.Min(minDistance, distance);
			}
			return minDistance;
		}

		/// <summary>
		/// Returns the velocity of the creature.
		/// </summary>
		/// <returns></returns>
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

			// Calculate the average velocity of the bones.
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
		}

		public float GetRotation() {

			if (bones.Count == 0) return 0;

			float rotation = 0;

			var boneCount = bones.Count;
			for (int i = 0; i < boneCount; i++) {
				if (usesLegacyRotationCalculation) {
					rotation += bones[i].transform.rotation.z;
				} else {
					rotation += (bones[i].transform.rotation.eulerAngles.z - 180f) * 0.002778f;
				}
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

		public float GetDistanceFromObstacle(GameObject obstacle) {
			
			float minDistance = float.PositiveInfinity;

			if (obstacle == null) return minDistance;

			var obstaclePos = obstacle.transform.position;
			var distances = new float[joints.Count];

			for (int i = 0; i < distances.Length; i++) {
				distances[i] = Vector3.Distance(joints[i].center, obstaclePos);
			}

			return Mathf.Min(distances);
		}

		public CreatureStats GetStatistics(int simulationTime) {

			var stats = new CreatureStats();

			var objectiveTracker = GetComponent<ObjectiveTracker>();
			var fitness = objectiveTracker.EvaluateFitness(simulationTime);

			stats.fitness = fitness;
			stats.horizontalDistanceTravelled = GetXPosition();
			stats.verticalDistanceTravelled = GetYPosition();
			stats.averageSpeed = Mathf.Sqrt(Mathf.Pow(stats.horizontalDistanceTravelled / simulationTime, 2) + Mathf.Pow(stats.verticalDistanceTravelled / simulationTime, 2));
			stats.numberOfBones = bones.Count;
			stats.numberOfMuscles = muscles.Count;
			stats.simulationTime = Mathf.RoundToInt(simulationTime);
			stats.weight = joints.Select(x => x.JointData.weight).Sum() + bones.Select(x => x.GetWeight()).Sum();
			stats.maxJumpingHeight = maxJumpingHeight;

			return stats;
		}

		public void SetOnVisibleLayer() {

			var boneLayer = LayerMask.NameToLayer("VisibleCreature");
			var jointLayer = LayerMask.NameToLayer("VisibleJoint");

			foreach (var bone in bones) {
				if (bone == null) continue;
				bone.SetLayer(boneLayer);
			}

			foreach (var joint in joints) {
				if (joint == null) continue;
				joint.gameObject.layer = jointLayer;
			}

			foreach (var muscle in muscles) {
				if (muscle == null) continue;
				muscle.gameObject.layer = boneLayer;
			}

			if (gameObject == null) return;
			gameObject.layer = boneLayer;
		}

		public void SetOnInvisibleLayer() {

			var boneLayer = LayerMask.NameToLayer("Creature");
			var jointLayer = LayerMask.NameToLayer("Joint");

			foreach (var bone in bones) {
				bone.SetLayer(boneLayer);
			}

			foreach (var joint in joints) {
				joint.gameObject.layer = jointLayer;
			}

			foreach (var muscle in muscles) {
				muscle.gameObject.layer = boneLayer;
			}

			gameObject.layer = boneLayer;
		}

		public void SetOnBestCreatureLayer() {

			var boneLayer = LayerMask.NameToLayer("PlaybackCreature");
			var jointLayer = LayerMask.NameToLayer("PlaybackJoint");

			foreach (var bone in bones) {
				bone.SetLayer(boneLayer);
			}

			foreach (var joint in joints) {
				joint.gameObject.layer = jointLayer;
			}

			foreach (var muscle in muscles) {
				muscle.gameObject.layer = boneLayer;
			}

			gameObject.layer = boneLayer;
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


}

