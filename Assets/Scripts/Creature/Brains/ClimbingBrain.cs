using System.Collections;
using UnityEngine;

namespace Keiwando.Evolution {

	public class ClimbingBrain : Brain {

		public override int NumberOfInputs => 6;

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