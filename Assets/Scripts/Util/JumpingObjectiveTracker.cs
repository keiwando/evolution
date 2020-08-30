using UnityEngine;

namespace Keiwando.Evolution {

    // Jump as high as possible ()

    public class JumpingObjectiveTracker: ObjectiveTracker {

        /// <summary>
        /// The optimal distance a "perfect" creature could travel in 10 seconds.
        /// Quite arbitrarily chosen.
        /// </summary> 
	    private const float MAX_HEIGHT = 20f;

        private float maxHeightJumped = 0f;
        private float maxWeightedAverageHeight = 0f;

        void FixedUpdate() {

            float distanceFromGround = creature.DistanceFromGround();

            float maxHeight = creature.GetHighestPoint().y - creature.GetLowestPoint().y + distanceFromGround;
            this.maxHeightJumped = Mathf.Max(distanceFromGround, maxHeightJumped);
            this.maxWeightedAverageHeight = Mathf.Max((4 * distanceFromGround + maxHeight) / 5, maxWeightedAverageHeight);
        }

        public override float EvaluateFitness(float simulationTime) {
            return maxWeightedAverageHeight / MAX_HEIGHT;
        }


    }
}