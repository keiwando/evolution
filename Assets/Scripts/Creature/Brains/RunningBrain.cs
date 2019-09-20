namespace Keiwando.Evolution {

	public class RunningBrain : Brain {

		public const int NUMBER_OF_INPUTS = 6;
		public override int NumberOfInputs => NUMBER_OF_INPUTS;

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