using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace Keiwando.Evolution {

	public enum BrainType {
		Universal = 0,
		LegacyRunningBrain = 1,
		LegacyJumpingBrain = 2,
		LegacyObstacleJumpBrain = 3,
		LegacyClimbingBrain = 4
	}

	[RequireComponent(typeof(Creature))]
	abstract public class Brain : MonoBehaviour {

		public FeedForwardNetwork Network { get; protected set; }

		abstract public int NumberOfInputs { get; }
		virtual public int NumberOfOutputs => numberOfUniqueMuscleIds;

		private int numberOfUniqueMuscleIds = 0;
		
		/// <summary>
		/// For each muscle, contains the index of the brain output that should be applied to this muscle.
		/// </summary>
		private int[] muscleToOutputIndex;

		private Muscle[] muscles;

		protected Creature creature;

		public virtual void Start() {
			this.creature = GetComponent<Creature>();
		}

    public static int GetNetworkInputCountForBrainType(BrainType brainType) {
      switch (brainType) {
        case BrainType.Universal: return UniversalBrain.NUMBER_OF_INPUTS;
        case BrainType.LegacyRunningBrain: return RunningBrain.NUMBER_OF_INPUTS;
        case BrainType.LegacyJumpingBrain: return JumpingBrain.NUMBER_OF_INPUTS;
        case BrainType.LegacyObstacleJumpBrain: return ObstacleJumpingBrain.NUMBER_OF_INPUTS;
        case BrainType.LegacyClimbingBrain: return ClimbingBrain.NUMBER_OF_INPUTS;
				default:
					Debug.LogError("Unhandled brain type.");
					return UniversalBrain.NUMBER_OF_INPUTS;
      }
    }

		public static int GetNetworkOutputCountForBrainType(BrainType brainType, UniqueMusclesContext uniqueMusclesContext) {
			switch (brainType) {
				case BrainType.Universal: return uniqueMusclesContext.numberOfUniqueMuscleIds + UniversalBrain.NUMBER_OF_ADDITIONAL_OUTPUTS;
				default: return uniqueMusclesContext.numberOfUniqueMuscleIds;
			}
		}

    public struct UniqueMusclesContext {
      public int numberOfUniqueMuscleIds;
		  /// For each muscle, contains the index of the brain output that should be applied to this muscle.
      public int[] muscleToOutputIndex;
    }

    public static UniqueMusclesContext CalculateUniqueMusclesContext(List<MuscleData> muscles) {
      int numberOfUniqueMuscleIds = 0;
      int[] muscleToOutputIndex = new int[muscles.Count];
			Dictionary<string, int> userIdToFirstCorrespondingMuscleIndex = null;
			for (int i = 0; i < muscles.Count; i++) {
				var muscle = muscles[i];
				int outputIndex = numberOfUniqueMuscleIds;
				if (muscle.userId == "") {
					numberOfUniqueMuscleIds++;
				} else {
					if (userIdToFirstCorrespondingMuscleIndex == null) {
						userIdToFirstCorrespondingMuscleIndex = new Dictionary<string, int>();
					}
					if (!userIdToFirstCorrespondingMuscleIndex.TryGetValue(muscle.userId, out outputIndex)) {
						outputIndex = numberOfUniqueMuscleIds++;
						userIdToFirstCorrespondingMuscleIndex.Add(muscle.userId, outputIndex);
					}
				}
				muscleToOutputIndex[i] = outputIndex;
			}
      return new UniqueMusclesContext {
        numberOfUniqueMuscleIds = numberOfUniqueMuscleIds,
        muscleToOutputIndex = muscleToOutputIndex
      };
    }

		public void Init(NeuralNetworkSettings settings, Muscle[] muscles, UniqueMusclesContext uniqueMusclesContext, float[] chromosome = null) {

			this.muscles = muscles;
			this.muscleToOutputIndex = new int[muscles.Length];
			this.numberOfUniqueMuscleIds = uniqueMusclesContext.numberOfUniqueMuscleIds;
      this.muscleToOutputIndex = uniqueMusclesContext.muscleToOutputIndex;

			this.Network = new FeedForwardNetwork(NumberOfInputs, NumberOfOutputs, settings, chromosome);
		}

		virtual public void FixedUpdate() {

			if (Network != null && creature.Alive && creature.recordingPlayer == null) {
				
				UpdateInputs();
				var outputs = Network.CalculateOutputs();
				ApplyOutputs(outputs);
			}
		}

		/// <summary>
		/// Load the Input values into the inputs vector.
		/// </summary>
		abstract protected void UpdateInputs();

		/// <summary>
		/// Takes the neural network outputs outputs and applies them to the 
		/// list of muscles. Calls the ApplyOutputToMuscle function for every output. 
		/// </summary>
		protected virtual void ApplyOutputs(float[] outputs) {

			for (int i = 0; i < muscles.Length; i++) {
				int outputIndex = this.muscleToOutputIndex[i];
				float output = float.IsNaN(outputs[outputIndex]) ? 0 : outputs[outputIndex];
				ApplyOutputToMuscle(output, muscles[i]);
			}
		}

		/// <summary>
		/// Interprets the output and calls the respective function on the muscle.
		/// </summary>
		protected virtual void ApplyOutputToMuscle(float output, Muscle muscle) {
			
			// maps the output of the sigmoid function from [0, 1] to a range of [-1, 1]
			float percent = 2 * output - 1f;

			if (percent < 0)
				muscle.muscleAction = Muscle.MuscleAction.CONTRACT;
			else
				muscle.muscleAction = Muscle.MuscleAction.EXPAND;

			muscle.SetContractionForce(Math.Abs(percent));
		}

		public string ToChromosomeString() {
			return Network.ToBinaryString();
		}


		private StringBuilder debugBuilder;
		private StringBuilder debugInputBuilder;
		protected virtual void DEBUG_PRINT_INPUTS() {

			// debugBuilder = new StringBuilder();
			if (debugInputBuilder == null) debugInputBuilder = new StringBuilder();
			var debugBuilder = debugInputBuilder;
			var inputs = Network.Inputs;

			debugBuilder.AppendLine("Distance from ground: " + inputs[0]);
			debugBuilder.AppendLine("Horiz vel: " + inputs[1]);
			debugBuilder.AppendLine("Vert vel: " + inputs[2]);
			debugBuilder.AppendLine("rot vel: " + inputs[3]);
			debugBuilder.AppendLine("points touchnig gr: " + inputs[4]);
			debugBuilder.AppendLine("rotation: " + inputs[5] + "\n");

			print(debugBuilder.ToString());
		}

		// protected virtual void DEBUG_PRINT_OUTPUTS() {

		// 	//var sBuilder = new StringBuilder();
		// 	if (debugBuilder == null) debugBuilder = new StringBuilder();

		// 	for (int i = 0; i < outputs[0].Length; i++) {
		// 		debugBuilder.AppendLine("Muscle " + (i+1) + " : " + outputs[0][i]);
		// 	}

		// 	print(debugBuilder.ToString());
		// }
	}
}
