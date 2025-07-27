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
		public List<Joint> joints = new List<Joint>();	
		/// <summary>
		/// The bones that have been placed in the scene.
		/// </summary>
		public List<Bone> bones = new List<Bone>();
		/// <summary>
		/// The muscles that have been placed in the scene.
		/// </summary>
		public List<Muscle> muscles = new List<Muscle>();
		/// <summary>
		/// The decorations that have been placed in the scene.
		/// </summary>
		public List<Decoration> decorations = new List<Decoration>();

		/// <summary> 
		/// The Bone that is currently being placed. 
		/// </summary>
		private Bone currentBone;

		/// <summary>
		/// The Muscle that is currently being placed.
		/// </summary>
		private Muscle currentMuscle;

		/// The decoration that is currently being placed.
		private Decoration currentDecoration;

		/// <summary>
		/// The minimum distance between two joints when they are placed 
		/// (Can be moved closer together using "Move").
		/// </summary>
		private float jointNonOverlapRadius = 0.6f;

		/// <summary>
		/// The bone thickness.
		/// </summary>
		public static float CONNECTION_WIDTH = 0.5f;

		private Dictionary<int, float>  _absoluteDecorationRotations = new Dictionary<int, float>();
		private static HashSet<int> _idsTmpAlloc = new HashSet<int>();

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

			foreach (var decorationData in design.Decorations) {
				CreateDecorationFromData(decorationData);
			}

			// Update the idCounter
			int maxJointID = design.Joints.Count > 0 ? design.Joints.Max(data => data.id) : 0;
			int maxBoneID = design.Bones.Count > 0 ? design.Bones.Max(data => data.id) : 0;
			int maxMuscleID = design.Muscles.Count > 0 ? design.Muscles.Max(data => data.id) : 0;
			int maxDecorationID = design.Decorations.Count > 0 ? design.Decorations.Max(decoration => decoration.id) : 0;
			idCounter = Mathf.Max(Mathf.Max(Mathf.Max(maxJointID, maxBoneID), maxMuscleID), maxDecorationID) + 1;

			Creature.RefreshDecorationSortingOrders(this.decorations);
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

			var jointData = new JointData(id: idCounter++, position: position, weight: 1f, penalty: 0f);
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
			if (startingJoint == null || endingJoint == null) {
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

			var muscleData = new MuscleData(idCounter++, bone.BoneData.id, bone.BoneData.id, Muscle.Defaults.MaxForce, true, "");
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
							oldData.strength, oldData.canExpand, oldData.userId
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
		#region Decoration Placement

		public void CreateDecorationFromBone(Bone referenceBone, Vector3 atPosition, DecorationType decorationType) {

			LocalDecorationTransform localTransform = CalculateDecorationTransform(
				bone: referenceBone, 
				worldPosition: atPosition, 
				worldRotation: 0f
			);
			if (decorationType == DecorationType.GooglyEye) {
				localTransform.scale = Decoration.DEFAULT_GOOGLY_EYE_SCALE;
			} else {
				localTransform.scale = Decoration.DEFAULT_EMOJI_SCALE;
			}

			var decorationData = new DecorationData(
				id: idCounter++,
				boneId: referenceBone.BoneData.id,
				offset: localTransform.offset,
				scale: localTransform.scale,
				rotation: localTransform.rotation,
				flipX: false,
				flipY: false,
				decorationType: decorationType
			);
			currentDecoration = Decoration.CreateFromData(referenceBone, decorationData);
			currentDecoration.SetVisualizeConnection(true);
		}

		public void CreateDecorationFromData(DecorationData data) {

			var bone = FindBoneWithId(data.boneId);
			if (bone == null) {
				Debug.LogError($"The connecting bone for decoration {data.id} was not found!");
				return;
			}
			var decoration = Decoration.CreateFromData(bone, data);
			decorations.Add(decoration);
		}

		public void UpdateCurrentDecoration(Vector3 worldPosition) {
			if (currentDecoration == null) { return; }
			MoveDecorationToWorldPosition(currentDecoration, worldPosition);
		}

		private void MoveDecorationToWorldPosition(Decoration decoration, Vector3 worldPosition) {
			LocalDecorationTransform newLocalTransform = CalculateNewDecorationTransform(decoration, worldPosition);
			DecorationData newDecorationData = decoration.DecorationData;
			newDecorationData.offset = newLocalTransform.offset;
			newDecorationData.rotation = newLocalTransform.rotation;
			newDecorationData.scale = newLocalTransform.scale;
			decoration.DecorationData = newDecorationData;

			decoration.UpdateOrientation();
		}

		private void SetDecorationScale(Decoration decoration, float scale) {
			DecorationData newDecorationData = decoration.DecorationData;
			newDecorationData.scale = scale;
			decoration.DecorationData = newDecorationData;
			decoration.UpdateOrientation();
		}

		private void SetDecorationRotation(Decoration decoration, float rotation) {
			DecorationData newDecorationData = decoration.DecorationData;
			newDecorationData.rotation = rotation;
			decoration.DecorationData = newDecorationData;
			decoration.UpdateOrientation();
		}

		public void CancelCurrentDecoration() {

			if (currentDecoration == null) return;

			UnityEngine.Object.Destroy(currentDecoration.gameObject);
			currentDecoration = null;
		}

		/// Checks to see if the current decoration is valid (attached to a bone) and if so
		/// adds it to the list of decorations.
		public bool PlaceCurrentDecoration() {
			if (currentDecoration == null) { return false; }

			if (currentDecoration.bone == null) {
				UnityEngine.Object.Destroy(currentDecoration.gameObject);
				currentDecoration = null;
				return false;
			} else {
				currentDecoration.SetVisualizeConnection(false);
				decorations.Add(currentDecoration);
				Creature.RefreshDecorationSortingOrders(this.decorations);
				currentDecoration = null;
				return true;
			}
		}

		struct LocalDecorationTransform {
			public Vector2 offset;
			public float rotation;
			public float scale;
		}

		private LocalDecorationTransform CalculateDecorationTransform(
			Bone bone, 
			Vector3 worldPosition, 
			float worldRotation
		) {
			Vector3 localOffset = bone.transform.InverseTransformPoint(worldPosition);
			Quaternion worldRotationQuat = Quaternion.Euler(0f, 0f, worldRotation);
			Quaternion localRotation = Quaternion.Inverse(bone.transform.rotation) * worldRotationQuat;

			return new LocalDecorationTransform {
				offset = new Vector2(localOffset.x, localOffset.y),
				rotation = localRotation.eulerAngles.z * Mathf.Deg2Rad,
				scale = 1f
			};
		}

		private LocalDecorationTransform CalculateNewDecorationTransform(
			Decoration decoration,
			Vector3 worldPosition
		) {
			Vector3 localOffset = decoration.bone.transform.InverseTransformPoint(worldPosition);
			return new LocalDecorationTransform {
				offset = localOffset,
				rotation = decoration.DecorationData.rotation,
				scale = decoration.DecorationData.scale
			};
		}

		#endregion
		#region Move

		/// <summary>
		/// Moves the currently selected components to the specified position.
		/// </summary>
		public void MoveSelection(Dictionary<int, Joint> jointsToMove, Dictionary<int, Decoration> decorationsToMove, Vector3 movement) {

			foreach (var joint in jointsToMove.Values) {
				var newPosition = joint.transform.position + movement;
				joint.MoveTo(newPosition);		
			}
			foreach (var decoration in decorationsToMove.Values) {
				var newWorldPosition = decoration.transform.position + movement;
				MoveDecorationToWorldPosition(decoration, newWorldPosition);
			}
			if (jointsToMove.Count > 0) {
				// We have to refresh the position of all decorations, since the joint movements could
				// have caused transitive decoration movements
				RefreshAllDecorationOrientations();
			}
		}

		public void ScaleSelection(Dictionary<int, Joint> jointsToScale, Dictionary<int, Decoration> decorationsToScsale, float scale, Vector2 scaleOrigin) {
			foreach (var joint in jointsToScale.Values) {
				Vector3 newPosition = GetNewPositionForScalingAroundPoint(joint.gameObject, scale, scaleOrigin);
				joint.MoveTo(newPosition);
			}
			foreach (var decoration in decorationsToScsale.Values) {
				Vector3 newPosition = GetNewPositionForScalingAroundPoint(decoration.gameObject, scale, scaleOrigin);
				MoveDecorationToWorldPosition(decoration, newPosition);
				float newDecorationScale = decoration.DecorationData.scale * scale;
				SetDecorationScale(decoration, newDecorationScale);
			}
			if (jointsToScale.Count > 0) {
				// We have to refresh the position of all decorations, since the joint movements could
				// have caused transitive decoration movements
				RefreshAllDecorationOrientations();
			}
		}

		private Vector3 GetNewPositionForScalingAroundPoint(GameObject gameObject, float scale, Vector2 scaleOrigin) {
				Vector3 pos = gameObject.transform.position;
				Vector3 scaleOrigin3D = new Vector3(scaleOrigin.x, scaleOrigin.y, pos.z);
				Vector3 offset = pos - scaleOrigin3D;
				Vector3 scaledOffset = new Vector3(offset.x * scale, offset.y * scale, 0f);
				Vector3 newPosition = scaleOrigin3D + scaledOffset;
				return newPosition;
		}

		public void RotateSelection(Dictionary<int, Joint> jointToRotate, Dictionary<int, Decoration> decorationsToRotate, float rotation, Vector2 rotationOrigin) {
			foreach (var decoration in decorationsToRotate.Values) {
				var newAbsoluteRotation = decoration.transform.rotation.eulerAngles.z + rotation;
				_absoluteDecorationRotations[decoration.GetId()] = newAbsoluteRotation;
			}
			foreach (var joint in jointToRotate.Values) {
				Vector3 newPosition = GetNewPositionForRotatingAroundPoint(joint.gameObject, rotation, rotationOrigin);
				joint.MoveTo(newPosition);
			}
			foreach (var decoration in decorationsToRotate.Values) {
				Vector3 newPosition = GetNewPositionForRotatingAroundPoint(decoration.gameObject, rotation, rotationOrigin);
				MoveDecorationToWorldPosition(decoration, newPosition);
				float newAbsoluteRotation = _absoluteDecorationRotations[decoration.GetId()];
				float localDeltaRotation = newAbsoluteRotation - decoration.transform.rotation.eulerAngles.z;
				float newRotation = decoration.DecorationData.rotation + localDeltaRotation * Mathf.Deg2Rad;
				SetDecorationRotation(decoration, newRotation);
			}
			if (jointToRotate.Count > 0) {
				// We have to refresh the position of all decorations, since the joint movements could
				// have caused transitive decoration movements
				RefreshAllDecorationOrientations();
			}
		}

		private Vector3 GetNewPositionForRotatingAroundPoint(GameObject gameObject, float dRotation, Vector2 rotationOrigin) {
			Vector3 pos = gameObject.transform.position;
			Vector3 rotationOrigin3D = new Vector3(rotationOrigin.x, rotationOrigin.y, pos.z);
			Vector3 offset = pos - rotationOrigin3D;
			Quaternion rotationQuat = Quaternion.Euler(0f, 0f, dRotation);
			Vector3 rotatedOffset = rotationQuat * offset;
			Vector3 newPosition = rotationOrigin3D + rotatedOffset;
			return newPosition;
		}

		private void RefreshAllDecorationOrientations() {
				foreach (Decoration decoration in this.decorations) {
					decoration.UpdateOrientation();
				}
		}

		public void CancelMove(Dictionary<int, Joint> jointsToMove, Dictionary<int, Decoration> decorationsToMove, CreatureDesign oldState) {
			foreach (JointData jointData in oldState.Joints) {
				if (jointsToMove.ContainsKey(jointData.id)) {
					Joint joint = jointsToMove[jointData.id];
					joint.MoveTo(jointData.position);
				}
			}
			foreach (DecorationData decorationData in oldState.Decorations) {
				if (decorationsToMove.ContainsKey(decorationData.id)) {
					Decoration decoration = decorationsToMove[decorationData.id];
					decoration.DecorationData = decorationData;
					decoration.UpdateOrientation();
				}
			}
			if (jointsToMove.Count > 0) {
				// We have to refresh the position of all decorations, since the joint movements could
				// have caused transitive decoration movements
				RefreshAllDecorationOrientations();
			}
		}

		/// <summary>
		/// Resets all properties used while moving a body component.
		/// </summary>
		/// <returns>Returns whether the creature design was changed.</returns>
		public bool MoveEnded(Dictionary<int, Joint> jointsToMove, Dictionary<int, Decoration> decorationsToMove) {

			var didChange = false;
			if (jointsToMove != null) {
				didChange |= jointsToMove.Count > 0;
				if (didChange) {
					foreach (var joint in jointsToMove.Values) {
						var oldData = joint.JointData;
						var newData = new JointData(oldData.id, joint.center, oldData.weight, oldData.fitnessPenaltyForTouchingGround);
						joint.JointData = newData;
					}
				}
			}
			if (decorationsToMove != null) {
				didChange |= decorationsToMove.Count > 0;
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
			foreach (var decoration in decorations) {
				decoration.Delete();
			}
			DeleteDecorationsOfDeletedBones();

			RemoveDeletedObjects();
			idCounter = 0;
		}

		public void Delete(List<BodyComponent> selection) {

			for (int i = 0; i < selection.Count; i++) {
				var item = selection[i];
				if (item == null) continue;

				item.Delete();
				
			}
			DeleteDecorationsOfDeletedBones();
			RemoveDeletedObjects();
		}

		private void DeleteDecorationsOfDeletedBones() {
			_idsTmpAlloc.Clear();
			HashSet<int> boneIds = _idsTmpAlloc;
			foreach (Bone bone in bones) {
				if (bone != null && bone.gameObject != null && !bone.deleted) {
					boneIds.Add(bone.BoneData.id);
				}
			}
			foreach (Decoration decoration in this.decorations) {
				if (!boneIds.Contains(decoration.DecorationData.boneId)) {
					decoration.Delete();
				}
			}
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

			foreach (Decoration decoration in decorations) {
				decoration.transform.SetParent(creatureObj.transform);
			}

			creature.joints = joints;
			creature.bones = bones;
			creature.muscles = muscles;
			creature.decorations = decorations;

			return creature;
		}

		/// <summary>
		/// Returns a CreatureDesign representation of the currently placed
		/// body parts 
		/// </summary>
		public CreatureDesign GetDesign(bool queryStateBeforeEdit = false) {
			
			var name = this.Name;
			var jointData = this.joints.Select(j => j.JointData).ToList();
			var boneData = this.bones.Select(b => b.BoneData).ToList();
			var muscleData = this.muscles.Select(m => m.MuscleData).ToList();
			var decorationData = this.decorations.Select(
				d => queryStateBeforeEdit ? d.DecorationDataBeforeEdit ?? d.DecorationData : d.DecorationData
			).ToList();
			return new CreatureDesign(name, jointData, boneData, muscleData, decorationData);
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
			decorations = BodyComponent.RemoveDeletedObjects<Decoration>(decorations);
		}

		#endregion

		/// <summary>
		/// Cancels any temporary body part that has not been placed yet.
		/// </summary>
		public void CancelTemporaryBodyParts() {
			CancelCurrentBone();
			CancelCurrentMuscle();
			CancelCurrentDecoration();
		}
	}

}