using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Keiwando.Evolution {

	public class CreatureBuilder {

		/// <summary>
		/// A counter used to assign a unique id to each body component created with this CreatureBuilder.
		/// Needs to be incremented after each use. Can be reset if needed. Starts at 0.
		/// This needs to be updated when a creature design is loaded!
		/// </summary>
		private int idCounter = 0;

		public string Name { get; set; } = "Unnamed";

		/// <summary>
		/// The joints of the creature that have been placed in the scene.
		/// </summary>
		private List<Joint> joints = new List<Joint>();	
		/// <summary>
		/// The bones that have been placed in the scene.
		/// </summary>
		private List<Bone> bones = new List<Bone>();
		/// <summary>
		/// The muscles that have been placed in the scene.
		/// </summary>
		private List<Muscle> muscles = new List<Muscle>();

		/// <summary> 
		/// The Bone that is currently being placed. 
		/// </summary>
		private Bone currentBone;

		/// <summary>
		/// The Muscle that is currently being placed.
		/// </summary>
		private Muscle currentMuscle;

		/// <summary>
		/// The minimum distance between two joints when they are placed 
		/// (Can be moved closer together using "Move").
		/// </summary>
		private float jointNonOverlapRadius = 0.6f;

		/// <summary>
		/// The bone thickness.
		/// </summary>
		public static float CONNECTION_WIDTH = 0.5f;

		public CreatureBuilder() {}

		public CreatureBuilder(CreatureDesign design) {

			this.Name = design.Name;

			foreach (var jointData in design.Joints) {
				this.joints.Add(Joint.CreateFromData(jointData));
			}

			foreach (var boneData in design.Bones) {
				CreateBoneFromData(boneData);	
			}

			foreach (var muscleData in design.Muscles) {
				CreateMuscleFromData(muscleData);
			}

			// Update the idCounter
			int maxJointID = design.Joints.Count > 0 ? design.Joints.Max(data => data.id) : 0;
			int maxBoneID = design.Bones.Count > 0 ? design.Bones.Max(data => data.id) : 0;
			int maxMuscleID = design.Muscles.Count > 0 ? design.Muscles.Max(data => data.id) : 0;
			idCounter = Mathf.Max(Mathf.Max(maxJointID, maxBoneID), maxMuscleID) + 1;
		}

		
		#region Joint Placement

		/// <summary>
		/// Attempts to place a joint at the specified world position.
		/// </summary>
		/// <remarks>Returns whether a joint was placed.</remarks>
		public bool TryPlacingJoint(Vector3 position) {

			// Make sure the joint doesn't overlap another one
			bool noOverlap = true;
			foreach (var joint in joints) {
				if ((joint.center - position).magnitude < jointNonOverlapRadius) {
					noOverlap = false;
					break;
				}
			}

			if (noOverlap) {
				PlaceJoint(position);
			}

			return noOverlap;
		}

		/// <summary>
		/// Places a new joint at the specified point.
		/// </summary>
		private void PlaceJoint(Vector3 position) {

			var jointData = new JointData(idCounter++, position, 1f);
			joints.Add(Joint.CreateFromData(jointData));
		}

		#endregion
		#region Bone Placement

		/// <summary>
		/// Attempts to place a bone beginning at the current position.
		/// </summary>
		public void TryStartingBone(Joint joint) {
			
			if (joint != null) {

				CreateBoneFromJoint(joint);
				PlaceConnectionBetweenPoints(currentBone.gameObject, joint.center, joint.center, CONNECTION_WIDTH);
			}
		}

		/// <summary>
		/// Instantiates a bone at the specified point.
		/// </summary>
		private void CreateBoneFromJoint(Joint joint){
			
			var boneData = new BoneData(idCounter++, joint.JointData.id, joint.JointData.id, 1f);
			currentBone = Bone.CreateAtPoint(joint.center, boneData);
			currentBone.startingJoint = joint;
		}

		private void CreateBoneFromData(BoneData data) {
			// Find the connecting joints
			var startingJoint = FindJointWithId(data.startJointID);
			var endingJoint = FindJointWithId(data.endJointID);
			if (startingJoint == null || endingJoint == null) {
				Debug.Log(string.Format("The connecting joints for bone {0} were not found!", data.id));
				return;
			}
			var bone = CreateBoneBetween(startingJoint, endingJoint, data);
			bone.ConnectToJoints();
			bones.Add(bone);
		}

		private Bone CreateBoneBetween(Joint startingJoint, Joint endingJoint, BoneData data) {

			var bone = Bone.CreateAtPoint(startingJoint.center, data);
			PlaceConnectionBetweenPoints(bone.gameObject, startingJoint.center, endingJoint.center, CONNECTION_WIDTH);
			bone.startingJoint = startingJoint;
			bone.endingJoint = endingJoint;
			return bone;
		}

		/// <summary>
		/// Updates the bone that is currently being placed to end at the 
		/// current mouse/touch position.
		/// </summary>
		public void UpdateCurrentBoneEnd(Vector3 position, Joint hoveringJoint) {

			if (currentBone != null) {
				// check if user is hovering over an ending joint which is not the same as the starting
				// joint of the currentBone
				var endPoint = position;

				if (hoveringJoint != null && !hoveringJoint.Equals(currentBone.startingJoint)) {
					endPoint = hoveringJoint.center;
					currentBone.endingJoint = hoveringJoint;
					var oldData = currentBone.BoneData;
					var newData = new BoneData(oldData.id, oldData.startJointID, hoveringJoint.JointData.id, oldData.weight, oldData.isWing, oldData.inverted);
					currentBone.BoneData = newData;
				} 

				PlaceConnectionBetweenPoints(currentBone.gameObject, currentBone.startingPoint, endPoint, CONNECTION_WIDTH);	
			}	
		}

		/// <summary>
		/// Transforms the given gameObject between the specified points. 
		/// (Points are flattened to 2D).
		/// </summary>
		/// <param name="connection">The object to place as between the start and end point</param>
		/// <param name="width">The thickness of the connection.</param>
		public static void PlaceConnectionBetweenPoints(GameObject connection, Vector3 start, Vector3 end, float width) {

			// flatten the vectors to 2D
			start.z = 0;
			end.z = 0;

			Vector3 offset = end - start;
			Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
			Vector3 position = start + (offset / 2.0f);

			connection.transform.position = position;
			connection.transform.up = offset;
			connection.transform.localScale = scale;
		}

		/// <summary>
		/// Checks to see if the current bone is valid (attached to two joints) and if so 
		/// adds it to the list of bones.
		/// </summary>
		/// <returns>Returns whether the current bone was placed.</returns>
		public bool PlaceCurrentBone() {

			if (currentBone == null) return false;

			if (currentBone.endingJoint == null || 
				currentBone.endingJoint.Equals(currentBone.startingJoint)) {
				// The connection has no connected ending -> Destroy
				UnityEngine.Object.Destroy(currentBone.gameObject);
				currentBone = null;
				return false;
			} else {
				currentBone.ConnectToJoints();
				bones.Add(currentBone);
				currentBone = null;
				// The creature was modified
				return true;
			}
		} 

		public void CancelCurrentBone() {

			if (currentBone == null) return;

			UnityEngine.Object.Destroy(currentBone.gameObject);
			currentBone = null;
		}

		#endregion
		#region Muscle Placement

		/// <summary>
		/// Attempts to place a muscle starting at the specified position.
		/// </summary>
		public void TryStartingMuscle(Bone bone) {

			if (bone != null) {
				CreateMuscleFromBone(bone);
				var startPos = bone.Center;
				currentMuscle.SetLinePoints(startPos, startPos);
			}
		}

		/// <summary>
		/// Instantiates a muscle at the specified point.
		/// </summary>
		private void CreateMuscleFromBone(Bone bone) {

			var muscleData = new MuscleData(idCounter++, bone.BoneData.id, bone.BoneData.id, Muscle.Defaults.MaxForce, true);
			currentMuscle = Muscle.CreateFromData(muscleData);
			currentMuscle.startingBone = bone;
			currentMuscle.SetLinePoints(bone.Center, bone.Center);
		}

		/// <summary>
		/// Updates the muscle that is currently being placed to end at the 
		/// specified position
		/// </summary>
		public void UpdateCurrentMuscleEnd(Vector3 endPoint, Bone hoveringBone) {

			if (currentMuscle != null) {
				// Check if user is hovering over an ending joint which is not the same as the starting
				// joint of the currentMuscle
				var endingPoint = endPoint;

				if (hoveringBone != null) {
					
					if (!hoveringBone.Equals(currentMuscle.startingBone)) {
						endingPoint = hoveringBone.Center;
						currentMuscle.endingBone = hoveringBone;	
						var oldData = currentMuscle.MuscleData;
						var newData = new MuscleData(
							oldData.id, oldData.startBoneID, hoveringBone.BoneData.id, 
							oldData.strength, oldData.canExpand
						); 
						currentMuscle.MuscleData = newData;
					} else {
						currentMuscle.endingBone = null;
					}
				} else {
					currentMuscle.endingBone = null;
				}

				currentMuscle.SetLinePoints(currentMuscle.startingBone.Center, endingPoint);
			}
		}

		/// <summary>
		/// Checks to see if the current muscle is valid (attached to two joints) and if so
		/// adds it to the list of muscles.
		/// </summary>
		/// <returns>Returns whether the current muscle was placed</returns>
		public bool PlaceCurrentMuscle() {
				
			if (currentMuscle == null) return false;

			if (currentMuscle.endingBone == null) {
				// The connection has no connected ending -> Destroy
				UnityEngine.Object.Destroy(currentMuscle.gameObject);
				currentMuscle = null;
				return false;
			} else {

				// Validate the muscle doesn't exist already
				foreach (Muscle muscle in muscles) {
					if (muscle.Equals(currentMuscle)) {
						UnityEngine.Object.Destroy(currentMuscle.gameObject);
						currentMuscle = null;
						return false;
					}
				}

				currentMuscle.ConnectToJoints();
				currentMuscle.AddCollider();
				muscles.Add(currentMuscle);
				currentMuscle = null;
				// The creature was modified
				return true;
			}
		}

		public void CancelCurrentMuscle() {

			if (currentMuscle == null) return;

			UnityEngine.Object.Destroy(currentMuscle.gameObject);
			currentMuscle = null;
		}

		private void CreateMuscleFromData(MuscleData data) {
			// Find the connecting joints
			var startingBone = FindBoneWithId(data.startBoneID);
			var endingBone = FindBoneWithId(data.endBoneID);
			
			if (startingBone == null || endingBone == null) {
				Debug.Log(string.Format("The connecting joints for bone {0} were not found!", data.id));
				return;
			}
			var muscle = CreateMuscleBetween(startingBone, endingBone, data);
			muscle.ConnectToJoints();
			muscle.AddCollider();
			muscles.Add(muscle);
		}

		private Muscle CreateMuscleBetween(Bone startingBone, Bone endingBone, MuscleData data) {

			var muscle = Muscle.CreateFromData(data);

			muscle.startingBone = startingBone;
			muscle.endingBone = endingBone;

			muscle.SetLinePoints(startingBone.Center, endingBone.Center);
			return muscle;
		}

		#endregion
		#region Move

		/// <summary>
		/// Moves the currently selected components to the specified position.
		/// </summary>
		public void MoveSelection(ICollection<Joint> jointsToMove, Vector3 movement) {

			foreach (var joint in jointsToMove) {
				var newPosition = joint.transform.position + movement;
				joint.MoveTo(newPosition);		
			}
		}

		/// <summary>
		/// Resets all properties used while moving a body component.
		/// </summary>
		/// <returns>Returns whether the creature design was changed.</returns>
		public bool MoveEnded(ICollection<Joint> jointsToMove) {

			if (jointsToMove == null) return false;

			var didChange = jointsToMove.Count > 0;
			if (didChange) {
				foreach (var joint in jointsToMove) {
					var oldData = joint.JointData;
					var newData = new JointData(oldData.id, joint.center, oldData.weight);
					joint.JointData = newData;
				}
			}
			return didChange;
		}

		#endregion

		public void Reset() {
			DeleteAll();
			Name = "Unnamed";
			idCounter = 0;
		}

		/// <summary>
		/// Deletes all body components placed with this builder.
		/// </summary>
		public void DeleteAll() {

			// Deleting joints will recursively delete all connected 
			// body parts as well
			foreach (var joint in joints) {
				joint.Delete();
			}

			RemoveDeletedObjects();
			idCounter = 0;
		}

		public void Delete(List<BodyComponent> selection) {

			for (int i = 0; i < selection.Count; i++) {
				var item = selection[i];
				if (item == null) continue;

				item.Delete();
				
			}
			RemoveDeletedObjects();
		}

		public void SetBodyComponents(List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {
			this.joints = joints;
			this.bones = bones;
			this.muscles = muscles;
		}

		/// <summary>
		/// Creates a Creature object from the currently placed bodyparts.
		/// </summary>
		public Creature Build() {

			GameObject creatureObj = new GameObject();
			creatureObj.name = "Creature";
			Creature creature = creatureObj.AddComponent<Creature>();

			foreach (Joint joint in joints) {
				joint.transform.SetParent(creatureObj.transform);
			}

			foreach (Bone connection in bones) {
				connection.transform.SetParent(creatureObj.transform);
			}

			foreach (Muscle muscle in muscles) {
				muscle.transform.SetParent(creatureObj.transform);
			}

			creature.joints = joints;
			creature.bones = bones;
			creature.muscles = muscles;

			return creature;
		}

		/// <summary>
		/// Returns a CreatureDesign representation of the currently placed
		/// body parts 
		/// </summary>
		public CreatureDesign GetDesign() {
			
			var name = this.Name;
			var jointData = this.joints.Select(j => j.JointData).ToList();
			var boneData = this.bones.Select(b => b.BoneData).ToList();
			var muscleData = this.muscles.Select(m => m.MuscleData).ToList();
			return new CreatureDesign(name, jointData, boneData, muscleData);
		}

		#region Hover Configuration

		public void RefreshMuscleColliders() {
			foreach (Muscle muscle in muscles) {
				muscle.RemoveCollider();
				muscle.AddCollider();
			}
		}

		#endregion

		#region Utils

		public BodyComponent FindWithId(int id) {
			var joint = FindJointWithId(id);
			if (joint != null) return joint;
			var bone = FindBoneWithId(id);
			if (bone != null) return bone;
			var muscle = FindMuscleWithId(id);
			return muscle;
		}

		private Joint FindJointWithId(int id) {
			foreach (var joint in joints) {
				if (joint.JointData.id == id) {
					return joint;
				}
			}
			return null;
		}

		private Bone FindBoneWithId(int id) {
			foreach (var bone in bones) {
				if (bone.BoneData.id == id) {
					return bone;
				} 
			}
			return null;
		}

		private Muscle FindMuscleWithId(int id) {
			foreach (var muscle in muscles) {
				if (muscle.MuscleData.id == id) {
					return muscle;
				}
			}
			return null;
		}

		/// <summary>
		/// Removes the already destroyed object that are still left in the lists.
		/// </summary>
		private void RemoveDeletedObjects() {

			bones = BodyComponent.RemoveDeletedObjects<Bone>(bones);
			joints = BodyComponent.RemoveDeletedObjects<Joint>(joints);
			muscles = BodyComponent.RemoveDeletedObjects<Muscle>(muscles);
		}

		#endregion

		/// <summary>
		/// Cancels any temporary body part that has not been placed yet.
		/// </summary>
		public void CancelTemporaryBodyParts() {
			CancelCurrentBone();
			CancelCurrentMuscle();
		}
	}

}