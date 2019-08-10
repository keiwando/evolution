using System.Collections;
using UnityEngine;

namespace Keiwando.Evolution {

	public class JumpingBrain : Brain {

		public override int NumberOfInputs { get { return 6; } }

		private const float MAX_HEIGHT = 20f;

		// private float maxHeightJumped;
		// private float maxWeightedAverageHeight = 0f;

		// public override void EvaluateFitness (){

		// 	//fitness = Mathf.Clamp(maxHeightJumped / MAX_HEIGHT, 0f, 1f);
		// 	fitness = Mathf.Clamp(maxWeightedAverageHeight / MAX_HEIGHT, 0f, 1f);
		// }

		/*Inputs:
		* 
		* - distance from ground
		* - dx velocity
		* - dy velocity
		* - rotational velocity
		* - number of points touching ground
		* - Creature rotation
		*/
		protected override void UpdateInputs (){

			// distance from ground
			// Assert.IsNotNull(creature, "Creature is null");
			// Assert.IsNotNull(Network.Inputs, "Input array is null");
			Network.Inputs[0] = creature.DistanceFromGround();

			// maxHeightJumped = Mathf.Max(Network.Inputs[0], maxHeightJumped);
			// float maxHeight = creature.GetHighestPoint().y - creature.GetLowestPoint().y + Network.Inputs[0];

			//print(Network.Inputs[0] + " : " + maxHeight);

			// CalculateWeightedAverageHeight(Network.Inputs[0], maxHeight);
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

		// private void CalculateWeightedAverageHeight(float minHeight, float maxHeight) {

		// 	// (weights) minHeight : maxHeight => 4 : 1
		// 	maxWeightedAverageHeight = Mathf.Max((4 * minHeight + maxHeight) / 5, maxWeightedAverageHeight);
		// }
	}

}