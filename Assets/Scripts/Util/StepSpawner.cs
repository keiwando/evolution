using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSpawner : MonoBehaviour {

	public GameObject step;

	/// <summary>
	/// Angle in degrees.
	/// </summary>
	public float angle;

	public int numberOfSteps = 4000;

	private Vector3 spawnPosition;
	private Vector3 spawnDistance;

	// Use this for initialization
	void Start () {
		
		spawnPosition = step.transform.position;

		var stepDistance = step.transform.localScale.x / 2;
		spawnDistance = new Vector3(stepDistance, Mathf.Sin(Mathf.PI / 2) * stepDistance, 0);

		SpawnSteps();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SpawnSteps() {

		spawnPosition -= spawnDistance * (numberOfSteps / 2);

		for (int i = 0; i < numberOfSteps; i++) {
			
			spawnPosition += spawnDistance;
			Instantiate(step, spawnPosition, step.transform.rotation);
		}
	}
}
