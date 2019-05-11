using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleJumpingBrain : Brain {

	public override int NumberOfInputs { get { return 7; } }


	//private long numOfCollisionsWithObstacle = 0;
	private HashSet<Joint> collidedJoints;

	private const float MAX_HEIGHT = 20f;

	private float maxHeightJumped;

	// Use this for initialization
	void Start () {

		collidedJoints = new HashSet<Joint>();
	}
	
	// Update is called once per frame
	public override void FixedUpdate () {
		base.FixedUpdate();

		//numOfCollisionsWithObstacle += creature.GetNumberOfObstacleCollisions();
		Creature.AddObstacleCollidingJointsToSet(collidedJoints);
	}

	public override void EvaluateFitness (){

		//print(string.Format("Number of obstacle collisions: {0}", numOfCollisionsWithObstacle));
		var heightFitness = Mathf.Clamp(maxHeightJumped / MAX_HEIGHT, 0f, 1f);
		//var collisionFitness = Mathf.Clamp(100f - (numOfCollisionsWithObstacle * 12) / GetComponent<Creature>().joints.Count, 0f, 100f) / 100f;
		var collisionFitness = 1f - Mathf.Clamp((float) collidedJoints.Count / Creature.joints.Count, 0f, 1f);

		fitness = 0.5f * (heightFitness + collisionFitness);
		//print(string.Format("HeightFitness: {0}%, CollisionFitness: {1}%, Total fitness: {2}%", heightFitness * 100f, collisionFitness * 100f, fitness * 100f));
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
		Network.Inputs[0] = Creature.DistanceFromGround();
		maxHeightJumped = Mathf.Max(Network.Inputs[0], maxHeightJumped);
		// horizontal velocity
		Vector3 velocity = Creature.GetVelocity();
		Network.Inputs[1] = velocity.x;
		// vertical velocity
		Network.Inputs[2] = velocity.y;
		// rotational velocity
		Network.Inputs[3] = Creature.GetAngularVelocity().z;
		// number of points touching ground
		Network.Inputs[4] = Creature.GetNumberOfPointsTouchingGround();
		// Creature rotation
		Network.Inputs[5] = Creature.GetRotation();
		// distance from obstacle
		Network.Inputs[6] = Creature.GetDistanceFromObstacle();
	}
}
