using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CreatureBuilder : MonoBehaviour {

	private enum BodyParts {
		Joint, 
		BodyConnection, 
		Muscle
	}

	/** The joint that can connect multiple BodyConnections. */
	public GameObject jointPreset;

	/** 
	 * The connection objects that represent the skeleton of the creature. 
	 * Bodyconnections can only be placed between two existing joints.
	*/
	public GameObject bodyConnectionPreset;

	/** The muscle that connects multiple bodyconnections and can apply pulling forces. */
	public GameObject musclePreset;


	/** The joints of the creature that have been placed in the scene. */
	private List<Joint> joints;
	/** The bodyConnections that have been placed in the scene. */
	private List<BodyConnection> bodyConnections;
	/** The muscles that have been placed in the scene. */
	private List<Muscle> muscles;

	private BodyParts selectedPart;

	/** The BodyConnection that is currently being placed. */
	private BodyConnection currentBodyConnection;


	private bool TESTING_ENABLED = true;

	// Use this for initialization
	void Start () {

		// initialize arrays
		joints = new List<Joint>();
		bodyConnections = new List<BodyConnection>();
		muscles = new List<Muscle>();

		// Joints are selected by default.
		selectedPart = BodyParts.Joint;
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

		if ( Input.GetMouseButtonDown(0) ) { 	// user touched
			
			if (selectedPart == BodyParts.Joint) {
				placeJoint(ScreenToWorldPoint(Input.mousePosition));

			} else if (selectedPart == BodyParts.BodyConnection ) {
				// find the selected joint
				Joint joint = getHoveringObject<Joint>(joints);

				if (joint != null) {

					createConnectionFromJoint(joint);
					placeConnectionBetweenPoints(joint.center, ScreenToWorldPoint(Input.mousePosition), 0.5f);
				}
			}
		} else if (Input.GetMouseButton(0)) {
			// Mouse click & hold
			if (selectedPart == BodyParts.BodyConnection ) {

				if (currentBodyConnection != null){
					// check if user is hovering over an ending joint which is not the same as the starting
					// joint of the currentBodyConnection
					Joint joint = getHoveringObject<Joint>(joints);
					Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

					if (joint != null && !joint.Equals(currentBodyConnection.startingJoint)) {
						endingPoint = joint.center;
						currentBodyConnection.endingJoint = joint;
					} 

					placeConnectionBetweenPoints(currentBodyConnection.startingPoint, endingPoint, 0.5f);	
				}	

			}

		} else if ( Input.GetMouseButtonUp(0) ) {

			if (selectedPart == BodyParts.BodyConnection ) {
				
				placeCurrentBodyConnection();	
			}
		} 



	}

	/** Handles all possible keyboard controls / shortcuts. */
	private void handleKeyboardInput() {

		// J = place Joint
		if (Input.GetKeyDown(KeyCode.J)) {
			selectedPart = BodyParts.Joint;
			updateHoverables();
		}

		// B = place body connection
		else if (Input.GetKeyDown(KeyCode.B)) {
			selectedPart = BodyParts.BodyConnection;
			updateHoverables();
		}

		// M = place muscle
		else if (Input.GetKeyDown(KeyCode.M)) {
			selectedPart = BodyParts.Muscle;
			updateHoverables();
		}

		// T = Go to testing scene
		else if (TESTING_ENABLED && Input.GetKeyDown(KeyCode.T)) {
			takeCreatureToTestScene();
		}



	}

	/** 
	 * Sets the shouldHighlight variable appropiately on every hoverable object
	 * depending on the currently selected part.
	 */
	private void updateHoverables() {

		if (selectedPart == BodyParts.Joint ) {
			// disable Highlights
			disableAllHoverables();
		} else if (selectedPart == BodyParts.BodyConnection ) {
			// make the joints hoverable
			setShouldHighlight(joints, true);
			setShouldHighlight(bodyConnections, false);
			setShouldHighlight(muscles, false);
		} else if (selectedPart == BodyParts.Muscle ) {
			// make the bodyConnections hoverables
			setShouldHighlight(joints, false);
			setShouldHighlight(bodyConnections, true);
			setShouldHighlight(muscles, false);
		}
	}

	private void disableAllHoverables() {

		setShouldHighlight(joints, false);
		setShouldHighlight(bodyConnections, false);
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

	/** Instantiates a bodyConnection at the specified point. */
	private void createConnectionFromJoint(Joint joint){

		Vector3 point = joint.center;
		point.z = 0;
		currentBodyConnection = ((GameObject) Instantiate(bodyConnectionPreset, point, Quaternion.identity)).GetComponent<BodyConnection>();
		currentBodyConnection.startingJoint = joint;
	}

	/** Places the currentBodyConnection between the specified points. (Points flattened to 2D) */
	private void placeConnectionBetweenPoints(Vector3 start, Vector3 end, float width) {

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		currentBodyConnection.transform.position = position;
		currentBodyConnection.transform.up = offset;
		currentBodyConnection.transform.localScale = scale;

	}

	/** 
	 * Checks to see if the current bodyconnection is valid (attached to two joints) and
	 * adds it to the list of connections.
	 */
	private void placeCurrentBodyConnection() {

		if (currentBodyConnection == null) return;

		if (currentBodyConnection.endingJoint == null || getHoveringObject<Joint>(joints) == null) {
			// The connection has no connected ending -> Destroy
			Destroy(currentBodyConnection.gameObject);
		
		} else {
			currentBodyConnection.connectToJoints();
			bodyConnections.Add(currentBodyConnection);
		}

		currentBodyConnection = null;
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

		foreach (BodyConnection connection in bodyConnections) {
			connection.transform.SetParent(creatureObj.transform);
		}

		foreach (Muscle muscle in muscles) {
			muscle.transform.SetParent(creatureObj.transform);
		}

		creature.joints = joints;
		creature.bones = bodyConnections;
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


}
