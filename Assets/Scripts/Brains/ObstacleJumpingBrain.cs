using System.Collections;
using UnityEngine;

public class ObstacleJumpingBrain : Brain {

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

	private float MAX_HEIGHT = 20f;

	private float maxHeightJumped;

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

		//print(string.Format("Number of obstacle collisions: {0}", numOfCollisionsWithObstacle));
		var heightFitness = Mathf.Clamp(maxHeightJumped / MAX_HEIGHT, 0f, 1f);
		var collisionFitness = Mathf.Clamp(100f - (numOfCollisionsWithObstacle * 12) / GetComponent<Creature>().joints.Count, 0f, 100f) / 100f;

		fitness = 0.5f * (heightFitness + collisionFitness);
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
		inputs[0][0] = creature.DistanceFromGround();
		maxHeightJumped = Mathf.Max(inputs[0][0], maxHeightJumped);
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
