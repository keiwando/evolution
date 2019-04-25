using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour {

	public GameObject OBSJumpingTaskAddons;
	public GameObject ClimbingTaskAddons;

	public GameObject[] flatGrounds;

	public GameObject backButtonBG;

	public CameraFollowScript[] cameras;

	public CameraController[] cameraControllers;

	private Evolution evolution;

	// Use this for initialization
	void Start () {

		evolution = GameObject.FindGameObjectWithTag("Evolution").GetComponent<Evolution>();

		SetupTask();
	}

	private void SetupTask() {

		switch(evolution.Settings.Task) {
			
		case EvolutionTask.Running: SetupRunningTask(); break;
		case EvolutionTask.Jumping: SetupJumpingTask(); break;
		case EvolutionTask.ObstacleJump: SetupObstacleJumpingTask(); break;
		case EvolutionTask.Climbing: SetupClimbingTask(); break;
		}
	}

	private void SetupRunningTask() {
		OBSJumpingTaskAddons.SetActive(false);
		ClimbingTaskAddons.SetActive(false);
		SetFlatGroundsActive(true);
		LockCamerasDiagonal(false);
	}

	private void SetupJumpingTask() {
		OBSJumpingTaskAddons.SetActive(false);
		ClimbingTaskAddons.SetActive(false);
		SetFlatGroundsActive(true);
		LockCamerasDiagonal(false);
	}

	private void SetupObstacleJumpingTask() {
		OBSJumpingTaskAddons.SetActive(true);
		ClimbingTaskAddons.SetActive(false);
		SetFlatGroundsActive(true);
		LockCamerasDiagonal(false);
	}

	private void SetupClimbingTask() {
		OBSJumpingTaskAddons.SetActive(false);
		ClimbingTaskAddons.SetActive(true); 
		SetFlatGroundsActive(false);
		backButtonBG.SetActive(false);
		LockCamerasDiagonal(true);
	}

	private void SetFlatGroundsActive(bool value) {
		foreach (var ground in flatGrounds) {
			ground.SetActive(value);
		}
	}

	private void LockCamerasDiagonal(bool lockedDiagonally) {
		foreach (var camera in cameras) {
			camera.DiagonalLock = lockedDiagonally;
		}
		foreach (var cameraController in cameraControllers) {
			var anchor = cameraController.ZoomAnchor;
			anchor.y = lockedDiagonally ? 0.5f : 0.0f;
			cameraController.ZoomAnchor = anchor;
			cameraController.MovementBoundsEnabled = lockedDiagonally ? false : true;
		}
	}
}
