using System.Collections;
using UnityEngine;

public class ClimbingBrain : Brain {

	protected override int NUMBER_OF_INPUTS {
		get {
			return 6;
		}
	}

	protected override int[] IntermediateLayerSizes {
		get {
			return new int[]{ 10 };
		}
	}

	private float MAX_HEIGHT = 100f;


	// Use this for initialization
	void Start () {
		if(IntermediateLayerSizes.Length != NUMBER_OF_LAYERS - 2) {
			Debug.LogError("IntermediateLayerSizes has too many or not enough elements.");
		}
	}

	// Update is called once per frame
	void Update () {
		base.Update();
	
	}

	public override void EvaluateFitness (){

		MAX_HEIGHT *= SimulationTime / 10f;
		// The fitness for the climbing task is made up of the final distance from the ground.
		fitness = Mathf.Clamp((creature.DistanceFromFlatFloor() / MAX_HEIGHT) + 0.5f, 0f, 1f);
		//print(string.Format("Climbing fitness: {0}",fitness));
		//print(string.Format("Distance from floor: {0}", creature.DistanceFromFlatFloor()));
	}

	/*Inputs:
	* 
	* - distance from ground
	* - dx velocity
	* - dy velocity
	* - rotational velocity
	* - number of points touching ground
	* - creature rotation
	*/
	protected override void UpdateInputs (){

		// distance from ground
		inputs[0][0] = creature.DistanceFromGround();
		// horizontal velocity
		Vector3 velocity = creature.GetVelocity();
		inputs[0][1] = velocity.x;
		// vertical velocity
		inputs[0][2] = velocity.y;
		// rotational velocity
		inputs[0][3] = creature.GetAngularVelocity().z;
		// number of points touching ground
		inputs[0][4] = creature.GetNumberOfPointsTouchingGround();
		// creature rotation
		inputs[0][5] = creature.GetRotation();
	}
}
