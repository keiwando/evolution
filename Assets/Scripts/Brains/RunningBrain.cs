using UnityEngine;
using System.Collections;

public class RunningBrain : Brain {

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

	private int MAX_DISTANCE = 5000;	// The optimal distance a "perfect" creature could travel in the simulation time.
	//private int MAX_SPEED = 100; 
	private float averageSpeed = 0;

	// Use this for initialization
	void Start () {

		//testMatrixConversion();

		if(IntermediateLayerSizes.Length != NUMBER_OF_LAYERS - 2) {
			Debug.LogError("IntermediateLayerSizes has too many or too few elements.");
		}
	}
	
	// Update is called once per frame
	public override void Update ()
	{
		base.Update();
		averageSpeed = (averageSpeed + creature.getVelocity().x) / 2; 
	}

	/*protected override void ApplyOutputToMuscle (float output, Muscle muscle)
	{
		//print(output);
		muscle.setContractionForce(output);
	}*/

	public override void evaluateFitness ()
	{
		// The fitness for the running task is made up of the distance travelled to the
		// right at the end of the time and the average weighted speed of the creature.
		// TODO: Add average speed into calculation.
		fitness = ( creature.getXPosition() ) / MAX_DISTANCE;

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
	protected override void updateInputs ()
	{
		// distance from ground
		inputs[0][0] = creature.distanceFromGround() + 0.5f;
		// horizontal velocity
		Vector3 velocity = creature.getVelocity();
		inputs[0][1] = velocity.x;
		// vertical velocity
		inputs[0][2] = velocity.y;
		// rotational velocity
		inputs[0][3] = creature.getAngularVelocity().z;
		// number of points touching ground
		inputs[0][4] = creature.getNumberOfPointsTouchingGround();
		// creature rotation
		inputs[0][5] = creature.getRotation();
	}

}
