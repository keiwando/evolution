using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {

	private const string OBSTACLE_PREFAB_PATH = "Prefabs/Obstacle Ball"; 

	/// <summary>
	/// The force with which the obstacles are accelerated after spawn.
	/// </summary>
	public float Obstacle_Force = 10f;
	/// <summary>
	/// The time distance between the spawn of two obstacles in seconds.
	/// </summary>
	public float Obstacle_Distance = 4f;

	public Transform spawnPoint;

	public GameObject obstacle;
	private Rigidbody obsRigidbody;

	void Start () {

		rigidbody = obstacle.GetComponent<Rigidbody>();

		StartCoroutine(SpawnObstacle());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Spawns a 
	private IEnumerator SpawnObstacle() {

		while(true) {
			
			yield return new WaitForSeconds(Obstacle_Distance);

			if (obstacle != null) Destroy(obstacle.gameObject);

			obsRigidbody.velocity = Vector3.zero;
			obstacle.transform.position = spawnPoint.position;
			obsRigidbody.AddForce(new Vector3(-Obstacle_Force, 0f, 0f));
		}

		yield return null;
	} 
}
