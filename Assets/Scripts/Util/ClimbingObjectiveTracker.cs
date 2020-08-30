using UnityEngine;

namespace Keiwando.Evolution {

    // Be as high up as possible at the end of the simulation run

    public class ClimbingObjectiveTracker: ObjectiveTracker {

        private const float MAX_HEIGHT = 100f;

        public override float EvaluateFitness(float simulationTime) {
            var maxHeight = MAX_HEIGHT * simulationTime / 10f;
            var verticalDistanceFromSpawn = creature.GetYPosition() - creature.InitialPosition.y;
            
            return (verticalDistanceFromSpawn / maxHeight) + 0.5f;
        }
    }
}