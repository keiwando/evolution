using System.Collections;
using UnityEngine;

namespace Keiwando.Evolution {

	public class ClimbingBrain : Brain {

		public override int NumberOfInputs { get { return 6; } }

		// private const float MAX_HEIGHT = 100f;


		// Update is called once per frame
		// public override void FixedUpdate () {
		// 	base.FixedUpdate();
		// }

		// public override void EvaluateFitness (){

		// 	var maxHeight = MAX_HEIGHT * SimulationTime / 10f;
		// 	// The fitness for the climbing objective is made up of the final distance from the ground.
		// 	fitness = Mathf.Clamp((Creature.DistanceFromFlatFloor() / maxHeight) + 0.5f, 0f, 1f);
		// 	//print(string.Format("Climbing fitness: {0}",fitness));
		// 	//print(string.Format("Distance from floor: {0}", creature.DistanceFromFlatFloor()));
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
		protected override void UpdateInputs (){

			// distance from ground
			Network.Inputs[0] = creature.DistanceFromGround();
			// horizontal velocity
			Vector3 velocity = creature.GetVelocity();
			Network.Inputs[1] = velocity.x;
			// vertical velocity
			Network.Inputs[2] = velocity.y;
			// rotational velocity
			Network.Inputs[3] = creature.GetAngularVelocity().z;
			// number of points touching ground
			Network.Inputs[4] = creature.GetNumberOfPointsTouchingGround();
			// Creature rotation
			Network.Inputs[5] = creature.GetRotation();
		}
	}

}