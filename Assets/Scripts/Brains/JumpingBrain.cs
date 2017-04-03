using System.Collections;
using UnityEngine;

public class JumpingBrain : Brain {

	protected override int NUMBER_OF_INPUTS {
		get {
			return 7;
		}
	}

	protected override int[] IntermediateLayerSizes {
		get {
			return new int[]{ 10 };
		}
	}

	private long numOfCollisionsWithObstacle = 0;

	// Use this for initialization
	void Start () {
		if(IntermediateLayerSizes.Length != NUMBER_OF_LAYERS - 2) {
			Debug.LogError("IntermediateLayerSizes has too many or not enough elements.");
		}
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();

		numOfCollisionsWithObstacle += creature.GetNumberOfObstacleCollisions();
	}

	public override void EvaluateFitness (){
		
		fitness = Mathf.Clamp(100 - numOfCollisionsWithObstacle, 0, 100);
	}

	/*Inputs:
	* 
	* - distance from ground
	* - dx velocity
	* - dy velocity
	* - rotational velocity
	* - number of points touching ground
	* - creature rotation
	* - distance from obstacle
	*/
	protected override void UpdateInputs (){
		
		// distance from ground
		inputs[0][0] = creature.DistanceFromGround() + 0.5f;
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
		// TODO: distance from obstacle
		inputs[0][6] = creature.GetDistanceFromObstacle();
	}
}
