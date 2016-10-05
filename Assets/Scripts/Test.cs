using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	public GameObject cylinderPrefab; //assumed to be 1m x 1m x 2m default unity cylinder to make calculations easy
	public GameObject spherePrefab;

	private GameObject cylinder;

	//added the start function for testing purposes
	void Start()
	{

		//testBuilding();
	}

	void Update() {

		if (Input.GetMouseButton(0)) {
			
			createCylinderBetweenPoints(Vector3.zero, Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.5f);
		}
	}

	private void createCylinderBetweenPoints(Vector3 start, Vector3 end, float width) {

		if ( cylinder != null ) Destroy(cylinder);

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		cylinder = (GameObject) Instantiate(cylinderPrefab, position, Quaternion.identity);
		cylinder.transform.up = offset;
		cylinder.transform.localScale = scale;

	}

	// Creates two Cylinders as bodyconnections and connects them using a
	// Hinge Joint
	private void testBuilding() {

		Vector3 leftPosition = new Vector3(-10,0,0);
		Vector3 rightPosition = new Vector3(10, 0,0);

		GameObject body1 = (GameObject) Instantiate(cylinderPrefab, leftPosition, Quaternion.identity);
	}
}
