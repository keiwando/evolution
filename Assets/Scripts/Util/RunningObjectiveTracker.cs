using UnityEngine;

namespace Keiwando.Evolution {

    /// Get as far to the right as possible.

    public class RunningObjectiveTracker: ObjectiveTracker {

        /// <summary>
        /// The optimal distance a "perfect" creature could travel in 10 seconds.
        /// Quite arbitrarily chosen.
        /// </summary> 
	    private const int MAX_DISTANCE = 55;

        public override float EvaluateFitness(float simulationTime) {
            return (creature.GetXPosition() - creature.InitialPosition.x) / (MAX_DISTANCE * simulationTime);
        }
    }
} 