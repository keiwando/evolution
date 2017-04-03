using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour {

	public GameObject JumpingTaskAddons;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SetupTask() {

		switch(Evolution.task) {
			
		case Evolution.Task.RUNNING: SetupRunningTask(); break;
		case Evolution.Task.JUMPING: SetupJumpingTask(); break;
		}
	}

	private void SetupRunningTask() {
		JumpingTaskAddons.SetActive(false);
	}

	private void SetupJumpingTask() {
		JumpingTaskAddons.SetActive(true);
	}
}
