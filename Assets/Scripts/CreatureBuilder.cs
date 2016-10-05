using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CreatureBuilder : MonoBehaviour {

	public enum BodyPart {
		Joint, 
		Bone, 
		Muscle
	}

	public ButtonManager buttonManager;

	/** The joint that can connect multiple Bones. */
	public GameObject jointPreset;

	/** 
	 * The connection objects that represent the skeleton of the creature. 
	 * Bodyconnections can only be placed between two existing joints.
	*/
	public GameObject bonePreset;

	/** The muscle that connects multiple bodyconnections and can apply pulling forces. */
	public GameObject musclePreset;


	/** The joints of the creature that have been placed in the scene. */
	private List<Joint> joints;
	/** The bones that have been placed in the scene. */
	private List<Bone> bones;
	/** The muscles that have been placed in the scene. */
	private List<Muscle> muscles;

	private BodyPart selectedPart;
	public BodyPart SelectedPart {
		get { return selectedPart; }
		set {
			selectedPart = value;
			updateHoverables();
		}
	}

	/** The Bone that is currently being placed. */
	private Bone currentBone;

	/** The Muscle that is currentyl being placed. */
	private Muscle currentMuscle;


	private float CONNECTION_WIDHT = 0.5f;
	private bool TESTING_ENABLED = true;

	// Use this for initialization
	void Start () {

		// initialize arrays
		joints = new List<Joint>();
		bones = new List<Bone>();
		muscles = new List<Muscle>();

		// Joints are selected by default.
		selectedPart = BodyPart.Joint;
	}
	
	// Update is called once per frame
	void Update () {

		handleClicks();

		handleKeyboardInput();
	}

	/** 
	 * Checks for click / touch events and handles them appropiately depending on the 
	 * currently selected body part.
	 */
	private void handleClicks() {

		if ( Input.GetMouseButtonDown(0) ) { 	// user clicked

			if (EventSystem.current.IsPointerOverGameObject()) return;
			
			if (selectedPart == BodyPart.Joint) {			// Place a JOINT
				placeJoint(ScreenToWorldPoint(Input.mousePosition));

			} else if (selectedPart == BodyPart.Bone ) {	// Start placing BONE
				// find the selected joint
				Joint joint = getHoveringObject<Joint>(joints);

				if (joint != null) {

					createBoneFromJoint(joint);
					placeConnectionBetweenPoints(currentBone.gameObject, joint.center, ScreenToWorldPoint(Input.mousePosition), CONNECTION_WIDHT);
				}

			} else if (selectedPart == BodyPart.Muscle) {	// Start placing MUSCLE
				// find the selected bone
				Bone bone = getHoveringObject<Bone>(bones);

				if (bone != null) {

					Vector3 mousePos = ScreenToWorldPoint(Input.mousePosition);
					MuscleJoint joint = bone.muscleJoint;

					createMuscleFromJoint(joint);
					placeConnectionBetweenPoints(currentMuscle.gameObject, joint.position, mousePos, CONNECTION_WIDHT);
				}
			}

		} else if (Input.GetMouseButton(0)) {
			// Mouse click & hold
			if (selectedPart == BodyPart.Bone ) {

				if (currentBone != null) {
					// check if user is hovering over an ending joint which is not the same as the starting
					// joint of the currentBone
					Joint joint = getHoveringObject<Joint>(joints);
					Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

					if (joint != null && !joint.Equals(currentBone.startingJoint)) {
						endingPoint = joint.center;
						currentBone.endingJoint = joint;
					} 

					placeConnectionBetweenPoints(currentBone.gameObject, currentBone.startingPoint, endingPoint, CONNECTION_WIDHT);	
				}	

			} else if (selectedPart == BodyPart.Muscle) {

				if (currentMuscle != null) {
					// check if user is hovering over an ending joint which is not the same as the starting
					// joint of the currentMuscle
					Bone bone = getHoveringObject<Bone>(bones);
					Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

					if(bone != null) {
						
						MuscleJoint joint = bone.muscleJoint;

						if (!joint.Equals(currentMuscle.startingJoint)) {
							endingPoint = joint.position;
							currentMuscle.endingJoint = joint;	
						}
					}

					placeConnectionBetweenPoints(currentMuscle.gameObject, currentMuscle.startingPoint, endingPoint, CONNECTION_WIDHT);
				}
			}

		} else if ( Input.GetMouseButtonUp(0) ) {

			if (selectedPart == BodyPart.Bone ) {
				
				placeCurrentBone();	
			} else if (selectedPart == BodyPart.Muscle) {

				placeCurrentMuscle();
			}
		} 

	}

	/** Handles all possible keyboard controls / shortcuts. */
	private void handleKeyboardInput() {

		if (Input.anyKeyDown) {
		
			// J = place Joint
			if (Input.GetKeyDown(KeyCode.J)) {
				SelectedPart = BodyPart.Joint;
			}

			// B = place body connection
			else if (Input.GetKeyDown(KeyCode.B)) {
				SelectedPart = BodyPart.Bone;
			}

			// M = place muscle
			else if (Input.GetKeyDown(KeyCode.M)) {
				SelectedPart = BodyPart.Muscle;
			}

			// T = Go to testing scene
			else if (TESTING_ENABLED && Input.GetKeyDown(KeyCode.T)) {
				takeCreatureToTestScene();
			}


			buttonManager.selectButton(selectedPart);
		}
	}

	/**
	 * Sets the shouldHighlight variable appropiately on every hoverable object
	 * depending on the currently selected part.
	 */
	private void updateHoverables() {

		if (selectedPart == BodyPart.Joint ) {
			// disable Highlights
			disableAllHoverables();
		} else if (selectedPart == BodyPart.Bone ) {
			// make the joints hoverable
			setShouldHighlight(joints, true);
			setShouldHighlight(bones, false);
			setShouldHighlight(muscles, false);
		} else if (selectedPart == BodyPart.Muscle ) {
			// make the bones hoverables
			setShouldHighlight(joints, false);
			setShouldHighlight(bones, true);
			setShouldHighlight(muscles, false);
		}
	}

	private void disableAllHoverables() {

		setShouldHighlight(joints, false);
		setShouldHighlight(bones, false);
		setShouldHighlight(muscles, false);
	}

	/** 
	 * Sets the shouldHighlight property of a Hoverable script entry in the specified list to the boolean value specified by
	 * shouldHighlight.
	*/
	private void setShouldHighlight<T>(List<T> hoverables, bool shouldHighlight) where T: Hoverable {

		foreach (var obj in hoverables) {

			obj.shouldHighlight = shouldHighlight;
		}
	} 

	/** 
	 * Returns the Object that the mouse is currently hovering over or null if there is no such object in the given list. 
	 * The list contains scripts that are attached to gameobjects which have a HoverableScript attached.
	*/
	private T getHoveringObject<T>(List<T> objects) where T: MonoBehaviour {

		foreach (T obj in objects) {
			
			if (obj.gameObject.GetComponent<Hoverable>().hovering) {
				return obj;
			}
		}

		return null;
	}


	/** Placed a new joint object at the specified point. */
	private void placeJoint(Vector3 point) {

		point.z = 0;

		GameObject joint = (GameObject) Instantiate(jointPreset, point, Quaternion.identity);
		joints.Add(joint.GetComponent<Joint>());
	}

	/** Instantiates a muscle at the specified point. */
	private void createMuscleFromJoint(MuscleJoint joint) {

		Vector3 point = joint.position;
		point.z = 0;
		currentMuscle = ((GameObject) Instantiate(musclePreset, point, Quaternion.identity)).GetComponent<Muscle>();
		currentMuscle.startingJoint = joint;
	}

	/** Transforms the given gameObject between the specified points. (Points flattened to 2D). */
	private void placeConnectionBetweenPoints(GameObject connection, Vector3 start, Vector3 end, float width) {

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

	/** Instantiates a bone at the specified point. */
	private void createBoneFromJoint(Joint joint){

		Vector3 point = joint.center;
		point.z = 0;
		currentBone = ((GameObject) Instantiate(bonePreset, point, Quaternion.identity)).GetComponent<Bone>();
		currentBone.startingJoint = joint;
	}

	/** Places the currentBone between the specified points. (Points flattened to 2D) */
	private void placeBoneBetweenPoints(Vector3 start, Vector3 end, float width) {

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		currentBone.transform.position = position;
		currentBone.transform.up = offset;
		currentBone.transform.localScale = scale;

	}

	/** 
	 * Checks to see if the current bone is valid (attached to two joints) and if so
	 * adds it to the list of bones.
	 */
	private void placeCurrentBone() {

		if (currentBone == null) return;

		if (currentBone.endingJoint == null || getHoveringObject<Joint>(joints) == null) {
			// The connection has no connected ending -> Destroy
			Destroy(currentBone.gameObject);
		
		} else {
			currentBone.connectToJoints();
			bones.Add(currentBone);
		}

		currentBone = null;
	} 

	/** 
	 * Checks to see if the current muscle is valid (attached to two joints) and if so
	 * adds it to the list of muscles.
	 */
	private void placeCurrentMuscle() {
			
		if (currentMuscle == null) return;

		if (currentMuscle.endingJoint == null || getHoveringObject<Bone>(bones) == null) {
			// The connection has no connected ending -> Destroy
			Destroy(currentMuscle.gameObject);
		} else {
			currentMuscle.connectToJoints();
			muscles.Add(currentMuscle);
		}

		currentMuscle = null;
	}

	private Vector3 ScreenToWorldPoint(Vector3 point) {
		return Camera.main.ScreenToWorldPoint(point);
	}

	/** Returns the gameobject that consists of the bodyparts that have been placed in the scene */
	public Creature buildCreature() {

		GameObject creatureObj = new GameObject();
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

		creature.setKinematic(false);

		return creature;
	} 

	/** Generates a creature and brings it to the testScene. */
	public void takeCreatureToTestScene() {

		Creature creature = buildCreature();
		DontDestroyOnLoad(creature.gameObject);

		SceneManager.LoadScene("TestingScene");
	}

	/** Generates a creature and starts the evolution simulation. */
	public void evolve() {

		Creature creature = buildCreature();
		DontDestroyOnLoad(creature.gameObject);

		// TODO: Start evolution simulation
	}


}
