using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class JumpingBrain : Brain {

	protected override int NUMBER_OF_INPUTS {
		get {
			return 6;
		}
	}

	/*protected override int[] IntermediateLayerSizes {
		get {
			return new int[]{ 10 };
		}
	}*/


	// Use this for initialization
	void Start () {
		if(IntermediateLayerSizes.Length != NUMBER_OF_LAYERS - 2) {
			Debug.LogError("IntermediateLayerSizes has too many or not enough elements.");
		}
	}

	//private const float MAX_HEIGHT = 20f;
	private const float MAX_HEIGHT = 20f;

	private float maxHeightJumped;
	private float maxWeightedAverageHeight;

	// Update is called once per frame
	void Update () {
		base.Update();

	}

	public override void EvaluateFitness (){

		//fitness = Mathf.Clamp(maxHeightJumped / MAX_HEIGHT, 0f, 1f);
		fitness = Mathf.Clamp(maxWeightedAverageHeight / MAX_HEIGHT, 0f, 1f);
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
		Assert.IsNotNull(creature, "Creature is null");
		Assert.IsNotNull(inputs, "Input array is null");
		inputs[0][0] = creature.DistanceFromGround();

		maxHeightJumped = Mathf.Max(inputs[0][0], maxHeightJumped);
		float maxHeight = creature.GetHighestPoint().y - creature.GetLowestPoint().y + inputs[0][0];

		CalculateWeightedAverageHeight(inputs[0][0], maxHeight);
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

	private void CalculateWeightedAverageHeight(float minHeight, float maxHeight) {

		// (weights) minHeight : maxHeight => 3 : 1
		maxWeightedAverageHeight = Mathf.Max((3 * minHeight + maxHeight) / (4 * MAX_HEIGHT), maxWeightedAverageHeight);
	}
}
