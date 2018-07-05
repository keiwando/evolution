using UnityEngine;
using System.Collections;

public class RunningBrain : Brain {

	public override int NUMBER_OF_INPUTS {
		get {
			return 6;
		}
	}

	/*protected override int[] IntermediateLayerSizes {
		get {
			return new int[]{ 10 };
		}
	}*/

	private int MAX_DISTANCE = 55;	// The optimal distance a "perfect" creature could travel in the simulation time.
	//private int MAX_SPEED = 60;
	private float averageSpeed = 0;

	// Use this for initialization
	void Start () {

		//TestMatrixConversion();

		if(IntermediateLayerSizes.Length != NUMBER_OF_LAYERS - 2) {
			Debug.LogError("IntermediateLayerSizes has too many or not enough elements.");
		}
	}
	
	// Update is called once per frame
	public override void FixedUpdate ()
	{
		base.FixedUpdate();
		averageSpeed = (averageSpeed + creature.GetVelocity().x) / 2; 
	}

	/*protected override void ApplyOutputToMuscle (float output, Muscle muscle)
	{
		//print(output);
		muscle.SetContractionForce(output);
	}*/

	public override void EvaluateFitness ()
	{
		// The fitness for the running task is made up of the distance travelled to the
		// right at the end of the time and the average weighted speed of the creature.
		//fitness = (creature.GetXPosition() + 0.5f * Mathf.Abs(averageSpeed)) / ((MAX_DISTANCE * SimulationTime) + MAX_SPEED);
		fitness = Mathf.Clamp(creature.GetXPosition() / (MAX_DISTANCE * SimulationTime), 0f, 1f);
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
	protected override void UpdateInputs () {

//		var basicInputs = creature.CalculateBasicBrainInputs();
//
//		inputs[0][0] = basicInputs.DistanceFromFloor;
//		// horizontal velocity
//		inputs[0][1] = basicInputs.VelocityX;
//		// vertical velocity
//		inputs[0][2] = basicInputs.VelocityY;
//		// rotational velocity
//		inputs[0][3] = basicInputs.AngularVelocity;
//		// number of points touching ground
//		inputs[0][4] = basicInputs.PointsTouchingGroundCount;
//		// creature rotation
//		inputs[0][5] = basicInputs.Rotation;

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

	/// <summary>
	/// Prints all of the input values.
	/// </summary>
	protected virtual void DEBUG_PRINT() {

		print("Distance from ground: " + inputs[0][0]);
		print("Horiz vel: " + inputs[0][1]);
		print("Vert vel: " + inputs[0][2]);
		print("rot vel: " + inputs[0][3]);
		print("points touchnig gr: " + inputs[0][4]);
		print("rotation: " + inputs[0][5] + "\n");
	}

}
