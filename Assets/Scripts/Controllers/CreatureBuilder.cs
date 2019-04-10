using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class CreatureBuilder {

	/// <summary>
	/// A counter used to assign a unique id to each body component created with this CreatureBuilder.
	/// Needs to be incremented after each use. Can be reset if needed. Starts at 0.
	/// </summary>
	private int idCounter = 0;

	/// <summary>
	/// The current creature design state created by this builder.
	/// </summary>
	/// <remarks>
	/// This needs to be kept consistent with the body components that
	/// have been placed in the scene.
	/// </remarks>
	/// // TODO: Figure out if this is needed
	// private CreatureDesign design;

	/// <summary>
	/// The joints of the creature that have been placed in the scene.
	/// </summary>
	private List<Joint> joints;	
	/// <summary>
	/// The bones that have been placed in the scene.
	/// </summary>
	private List<Bone> bones;
	/// <summary>
	/// The muscles that have been placed in the scene.
	/// </summary>
	private List<Muscle> muscles;

	/// <summary> 
	/// The Bone that is currently being placed. 
	/// </summary>
	private Bone currentBone;

	/// <summary>
	/// The Muscle that is currently being placed.
	/// </summary>
	private Muscle currentMuscle;

	/// <summary>
	/// The joint that is currently being moved.
	/// </summary>
	private Joint currentMovingJoint;

	/// <summary>
	/// The minimum distance between two joints when they are placed 
	/// (Can be moved closer together using "Move").
	/// </summary>
	private float jointNonOverlapRadius = 0.6f;

	/// <summary>
	/// The bone thickness.
	/// </summary>
	public static float CONNECTION_WIDTH = 0.5f;

	public CreatureBuilder() {
		// this.design = new CreatureDesign();
	}
	
	// /// <returns>The joints created by this builder.</returns>
	// public List<Joint> GetJoints() {
	// 	return new List<Joint>(joints);
	// }

	// /// <returns>The bones created by this builder.</returns>
	// public List<Bone> GetBones() {
	// 	return new List<Bone>(bones);
	// }

	// /// <returns>The muscles created by this builder.</returns>
	// public List<Muscle> GetMuscles() {
	// 	return new List<Muscle>(muscles);
	// }

	/// <summary>
	/// Enabled / Disables highlighting on hover for the specified body components
	/// </summary>
	public void EnableHighlighting(bool joint, bool bone, bool muscle) {

		HoveringUtil.SetShouldHighlight(this.joints, joint);
		HoveringUtil.SetShouldHighlight(this.bones, bone);
		HoveringUtil.SetShouldHighlight(this.muscles, muscle);
	}

	/// <summary>
	/// Resets the hoverable colliders on joints and bones.
	/// </summary>
	private void ResetHoverableColliders() {

		HoveringUtil.ResetHoverableColliders(joints);
		HoveringUtil.ResetHoverableColliders(bones);
	}

	/// <summary>
	/// Removes the already destroyed object that are still left in the lists.
	/// </summary>
	private void RemoveDeletedObjects() {

		bones = RemoveDeletedObjects<Bone>(bones);
		joints = RemoveDeletedObjects<Joint>(joints);
		muscles = RemoveDeletedObjects<Muscle>(muscles);
	}

	/// <summary>
	/// Removes the already destroyed object that are still left in the list.
	/// This operation doesn't change the original list.
	/// </summary>
	/// <param name="objects">A list of BodyComponents</param>
	/// <typeparam name="T">A BodyComponent subtype.</typeparam>
	/// <returns>A list without the already destroyed objects of the input list.</returns>
	private static List<T> RemoveDeletedObjects<T>(List<T> objects) where T: BodyComponent {

		List<T> removed = new List<T>(objects);
		foreach (T obj in objects) {
			if (obj == null || obj.Equals(null) || obj.gameObject == null 
				|| obj.gameObject.Equals(null) || obj.deleted) {
	
				removed.Remove(obj);
			}
		}
		return removed;
	}

	/// <summary>
	/// Sets the specified mouse hover texture on all body components created 
	/// by this builder.
	/// </summary>
	public void SetMouseHoverTextures(Texture2D texture) {

		HoveringUtil.SetMouseHoverTexture(joints, texture);
		HoveringUtil.SetMouseHoverTexture(bones, texture);
		HoveringUtil.SetMouseHoverTexture(muscles, texture);
	}
	

	#region Joint Placement

	/// <summary>
	/// Attempts to place a joint at the specified world position.
	/// </summary>
	public void TryPlacingJoint(Vector3 pos) {

		// Make sure the joint doesn't overlap another one
		bool noOverlap = true;
		foreach (var joint in joints) {
			if ((joint.center - pos).magnitude < jointNonOverlapRadius) {
				noOverlap = false;
				break;
			}
		}

		if (noOverlap) {
			PlaceJoint(pos);
			ResetCurrentCreatureName();
		}
	}

	/// <summary>
	/// Places a new joint at the specified point.
	/// </summary>
	private void PlaceJoint(Vector3 position) {
		// TODO: Create JointData
		var jointData = new JointData(idCounter++, position, 1f);
		joints.Add(Joint.CreateAtPoint(position));
	}

	#endregion
	#region Bone Placement

	/// <summary>
	/// Attempts to place a bone beginning at the current position.
	/// </summary>
	public void TryStartingBone(Vector3 startPos) {
		// find the selected joint
		Joint joint = GetHoveringObject<Joint>(joints);

		if (joint != null) {

			CreateBoneFromJoint(joint);
			PlaceConnectionBetweenPoints(currentBone.gameObject, joint.center, ScreenToWorldPoint(startPos), CONNECTION_WIDTH);
		}
	}

	/// <summary>
	/// Instantiates a bone at the specified point.
	/// </summary>
	private void CreateBoneFromJoint(Joint joint){

		Vector3 point = joint.center;
		point.z = 0;
		
		currentBone = Bone.CreateAtPoint(point);
		currentBone.startingJoint = joint;
	}

	/// <summary>
	/// Updates the bone that is currently being placed to end at the 
	/// current mouse/touch position.
	/// </summary>
	public void UpdateCurrentBoneEnd() {

		if (currentBone != null) {
			// check if user is hovering over an ending joint which is not the same as the starting
			// joint of the currentBone
			Joint joint = GetHoveringObject<Joint>(joints);
			Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

			if (joint != null && !joint.Equals(currentBone.startingJoint)) {
				endingPoint = joint.center;
				currentBone.endingJoint = joint;
			} 

			PlaceConnectionBetweenPoints(currentBone.gameObject, currentBone.startingPoint, endingPoint, CONNECTION_WIDTH);	
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
	public void PlaceCurrentBone() {

		if (currentBone == null) return;

		if (currentBone.endingJoint == null || GetHoveringObject<Joint>(joints) == null || currentBone.endingJoint.Equals(currentBone.startingJoint)) {
			// The connection has no connected ending -> Destroy
			Destroy(currentBone.gameObject);
		} else {
			currentBone.ConnectToJoints();
			bones.Add(currentBone);
			// The creature was modified
			ResetCurrentCreatureName(); 
		}

		currentBone = null;
	} 

	#endregion
	#region Muscle Placement

	/// <summary>
	/// Attempts to place a muscle starting at the specified position.
	/// </summary>
	public void TryStartingMuscle(Vector3 startPos) {

		// find the selected bone
		Bone bone = GetHoveringObject<Bone>(bones);

		if (bone != null) {
			Vector3 mousePos = ScreenToWorldPoint(Input.mousePosition);
			MuscleJoint joint = bone.muscleJoint;

			CreateMuscleFromJoint(joint);
			currentMuscle.SetLinePoints(joint.transform.position, mousePos);
		}
	}

	/// <summary>
	/// Instantiates a muscle at the specified point.
	/// </summary>
	private void CreateMuscleFromJoint(MuscleJoint joint) {

		Vector3 point = joint.transform.position;
		point.z = 0;

		currentMuscle = Muscle.Create();
		currentMuscle.startingJoint = joint;
		currentMuscle.SetLinePoints(joint.transform.position, joint.transform.position);
	}

	/// <summary>
	/// Updates the muscle that is currently being placed to end at the 
	/// current mouse/touch position.
	/// </summary>
	public void UpdateCurrentMuscleEnd() {

		if (currentMuscle != null) {
			// Check if user is hovering over an ending joint which is not the same as the starting
			// joint of the currentMuscle
			Bone bone = GetHoveringObject<Bone>(bones);
			Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

			if(bone != null) {
				
				MuscleJoint joint = bone.muscleJoint;

				if (!joint.Equals(currentMuscle.startingJoint)) {
					endingPoint = joint.transform.position;
					currentMuscle.endingJoint = joint;	
				} else {
					currentMuscle.endingJoint = null;
				}
			} else {
				currentMuscle.endingJoint = null;
			}

			currentMuscle.SetLinePoints(currentMuscle.startingJoint.transform.position, endingPoint);
		}
	}

	#region Move

	/// <summary>
	/// Attempts to start moving the body component that is currently being
	/// hovered over.
	/// </summary>
	public void TryStartComponentMove() {
		// TODO: Add the option to move bones
		// Make sure the user is hovering over a joint
		Joint joint = GetHoveringObject<Joint>(joints);

		if (joint != null) {
			currentMovingJoint = joint;
			// The creature was modified
			ResetCurrentCreatureName(); 
		}
	}

	/// <summary>
	/// Moves the currently selected components to the mouse position.
	/// </summary>
	public void MoveCurrentComponent() {

		if (currentMovingJoint != null) {
			// Move the joint to the mouse position.
			var newPoint = ScreenToWorldPoint(Input.mousePosition);

			if (grid.gameObject.activeSelf) {
				newPoint = grid.ClosestPointOnGrid(newPoint);
			}
			newPoint.z = 0;

			currentMovingJoint.MoveTo(newPoint);
			// The creature was modified
			ResetCurrentCreatureName(); 
		}
	}

	/// <summary>
	/// Resets all properties used while moving a body component.
	/// </summary>
	public void MoveEnded() {
		currentMovingJoint = null;
	}

	#endregion

	/// <summary>
	/// Checks to see if the current muscle is valid (attached to two joints) and if so
	/// adds it to the list of muscles.
	/// </summary>
	public void PlaceCurrentMuscle() {
			
		if (currentMuscle == null) return;

		if (currentMuscle.endingJoint == null || GetHoveringObject<Bone>(bones) == null) {
			// The connection has no connected ending -> Destroy
			Destroy(currentMuscle.gameObject);
		} else {

			// Validate the muscle doesn't exist already
			foreach (Muscle muscle in muscles) {
				if (muscle.Equals(currentMuscle)) {
					Destroy(currentMuscle.gameObject);
					currentMuscle = null;
					return;
				}
			}

			currentMuscle.ConnectToJoints();
			currentMuscle.AddCollider();
			muscles.Add(currentMuscle);
			// The creature was modified
			ResetCurrentCreatureName(); 
		}

		currentMuscle = null;
	}

	#endregion

	/// <summary>
	/// Deletes the currently visible creature.
	/// </summary>
	public void DeleteCreature() {

		foreach(var joint in joints) {
			joint.Delete();
		}

		UpdateDeletedObjects();
	}

	/// <summary>
	/// Deletes the placed body components that are currently being
	/// hovered over.
	/// </summary>
	private void DeleteHoveringBodyComponent() {

		BodyComponent joint = GetHoveringObject<Joint>(joints);
		BodyComponent bone = GetHoveringObject<Bone>(bones);
		BodyComponent muscle = GetHoveringObject<Muscle>(muscles);

		BodyComponent toDelete = joint != null ? joint : ( bone != null ? bone : muscle ) ;

		if (toDelete != null) {
			toDelete.Delete();
			UpdateDeletedObjects();
			// The creature was modified
			ResetCurrentCreatureName(); 

			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}

	/// <summary>
	/// Resets the current creature name to "Unnamed".
	/// </summary>
	private void ResetCurrentCreatureName() {
		creatureDesignControls.SetUnnamed();
		Settings.CurrentCreatureName = "Unnamed";
	}

	/// <summary>
	/// Updates the view that displays the current creature name.
	/// </summary>
	private void RefreshCurrentCreatureName() {
		creatureDesignControls.SetCurrentCreatureName(CreatureSaver.GetCurrentCreatureName());
	}

	public void SetBodyComponents(List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {
		this.joints = joints;
		this.bones = bones;
		this.muscles = muscles;
	}

	/// <summary>
	/// Creates a Creature object from the currently placed bodyparts.
	/// </summary>
	public Creature BuildCreature() {

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

		DisableAllHoverables();

		return creature;
	} 

	/// <summary>
	/// Creates the creature and attaches it to the evolution.
	/// </summary>
	public void AttachCreatureToEvolution(Evolution evolution) {

		ResetHoverableColliders();

		var name = CreatureSaver.GetCurrentCreatureName();
		CreatureSaver.SaveCurrentCreature(name, joints, bones, muscles);

		Creature creature = BuildCreature();
		DontDestroyOnLoad(creature.gameObject);

		evolution.creature = creature;
	}

	/// <summary>
	/// Generates a creature from the currently places body components
	/// and starts the simulation.
	/// </summary>
	public void Evolve() {

		// Don't attempt evolution if there is no creature
		if (joints.Count == 0) return;

		ResetHoverableColliders();

		var name = CreatureSaver.GetCurrentCreatureName();
		CreatureSaver.SaveCurrentCreature(name, joints, bones, muscles);

		Creature creature = BuildCreature();
		DontDestroyOnLoad(creature.gameObject);

		AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("EvolutionScene");
		//AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("TestEvolutionScene");
		sceneLoading.allowSceneActivation = true;
		DontDestroyOnLoad(evolution.gameObject);
		evolution.creature = creature;

		var settings = settingsMenu.GetSimulationSettings();

		evolution.Settings = settings;
		evolution.BrainSettings = settingsMenu.GetNeuralNetworkSettings();

		SetMobileNoSleep();

		StartCoroutine(WaitForEvolutionSceneToLoad(sceneLoading));
		DontDestroyOnLoad(this);
	}

	IEnumerator WaitForEvolutionSceneToLoad(AsyncOperation loadingOperation) {

		while(!loadingOperation.isDone){
			//print(loadingOperation.progress);
			yield return null;
		}
			
		Destroy(this.gameObject);
		print("Starting Evolution");
		evolution.StartEvolution();
	}

	/// <summary>
	/// Changes to the evolution scene for continuing a saved simulation.
	/// </summary>
	public void ContinueEvolution(Evolution evolution, Action completion) {

		SetMobileNoSleep();

		AttachCreatureToEvolution(evolution);

		AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("EvolutionScene");
		//AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("TestEvolutionScene");
		sceneLoading.allowSceneActivation = true;
		DontDestroyOnLoad(evolution.gameObject);

		StartCoroutine(WaitForEvolutionSceneToLoadForLoad(sceneLoading, completion));
		DontDestroyOnLoad(this);
	}

	IEnumerator WaitForEvolutionSceneToLoadForLoad(AsyncOperation loadingOperation, Action completion) {

		while(!loadingOperation.isDone){
			yield return null;
		}

		Destroy(this.gameObject);
		completion();
	}

	/// <summary>
	/// Attempts to save the current creature. Shows an error screen if something went wrong.
	/// </summary>
	/// <param name="name">Name.</param>
	public void SaveCreature(string name) {

		CreatureSaver.WriteSaveFile(name, joints, bones, muscles);
		CreatureSaver.SaveCurrentCreatureName(name);
		RefreshCurrentCreatureName();
	}

	/// <summary>
	/// Loads a previously saved creature design into the scene.
	/// </summary>
	public void LoadCreature(string name) {

		DeleteCreature();
		CreatureSaver.SaveCurrentCreatureName(name);
		RefreshCurrentCreatureName();

		CreatureSaver.LoadCreature(name, this);
	}

	/// <summary>
	/// Sets the screen to not go to sleep on mobile devices.
	/// </summary>
	private void SetMobileNoSleep() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	/// <summary>
	/// Resets the sleep timeout settings to the system settings.
	/// </summary>
	private void SetMobileDefaultSleep() {
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}
}
