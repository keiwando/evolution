using UnityEngine;
using System.Collections;

namespace Keiwando.Evolution {

	public class RunningBrain : Brain {

		public override int NumberOfInputs { get { return 6; } }

		// private float averageSpeed = 0;
		
		// Update is called once per frame
		// public override void FixedUpdate () {
		// 	base.FixedUpdate();
		// 	//averageSpeed = (averageSpeed + creature.GetVelocity().x) / 2;
		// 	var velX = Network.Inputs[1];
		// 	averageSpeed = (averageSpeed + velX) / 2; 
		// }

		/*protected override void ApplyOutputToMuscle (float output, Muscle muscle)
		{
			//print(output);
			muscle.SetContractionForce(output);
		}*/

		// public override void EvaluateFitness ()
		// {
		// 	// The fitness for the running objective is made up of the distance travelled to the
		// 	// right at the end of the time and the average weighted speed of the creature.
		// 	//fitness = (creature.GetXPosition() + 0.5f * Mathf.Abs(averageSpeed)) / ((MAX_DISTANCE * SimulationTime) + MAX_SPEED);
		// 	fitness = Mathf.Clamp(Creature.GetXPosition() / (55 * SimulationTime), 0f, 1f);
		// }

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

			var basicInputs = creature.CalculateBasicBrainInputs();

			Network.Inputs[0] = basicInputs.DistanceFromFloor;
			// horizontal velocity
			Network.Inputs[1] = basicInputs.VelocityX;
			// vertical velocity
			Network.Inputs[2] = basicInputs.VelocityY;
			// rotational velocity
			Network.Inputs[3] = basicInputs.AngularVelocity;
			// number of points touching ground
			Network.Inputs[4] = basicInputs.PointsTouchingGroundCount;
			// creature rotation
			Network.Inputs[5] = basicInputs.Rotation;

			// distance from ground
	//		inputs[0][0] = creature.DistanceFromGround();
	//		// horizontal velocity
	//		Vector3 velocity = creature.GetVelocity();
	//		inputs[0][1] = velocity.x;
	//		// vertical velocity
	//		inputs[0][2] = velocity.y;
	//		// rotational velocity
	//		inputs[0][3] = creature.GetAngularVelocity().z;
	//		// number of points touching ground
	//		inputs[0][4] = creature.GetNumberOfPointsTouchingGround();
	//		// creature rotation
	//		inputs[0][5] = creature.GetRotation();
		}

		/// <summary>
		/// Prints all of the input values.
		/// </summary>
		protected virtual void DEBUG_PRINT() {

			print("Distance from ground: " + Network.Inputs[0]);
			print("Horiz vel: " + Network.Inputs[1]);
			print("Vert vel: " + Network.Inputs[2]);
			print("rot vel: " + Network.Inputs[3]);
			print("points touchnig gr: " + Network.Inputs[4]);
			print("rotation: " + Network.Inputs[5] + "\n");
		}

	}

}