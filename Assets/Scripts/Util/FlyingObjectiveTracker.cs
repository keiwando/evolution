using UnityEngine;

namespace Keiwando.Evolution {

    public class FlyingObjectiveTracker: ObjectiveTracker {

        /// <summary>
        /// The optimal height a "perfect" creature could reach in 10 seconds.
        /// Quite arbitrarily chosen.
        /// </summary> 
	    private const float MAX_HEIGHT = 40f;

        private float maxHeightJumped = 0f;
        private int totalFrameCount = 0;
        private int framesSpentNotTouchingGround = 0;

        void FixedUpdate() {

            float distanceFromGround = creature.DistanceFromGround();
            bool noJointsAreTouchingGround = creature.GetNumberOfPointsTouchingGround() == 0;

            float safeDistanceFromGround = Mathf.Max(0f, distanceFromGround);
            this.maxHeightJumped = Mathf.Max(safeDistanceFromGround, this.maxHeightJumped);
            this.totalFrameCount += 1;
            if (noJointsAreTouchingGround) {
                this.framesSpentNotTouchingGround += 1;
            }
        }

        public override float EvaluateFitness(float simulationTime) {
            float heightFitness = System.Math.Min(1.0f, (simulationTime / 10.0f) * maxHeightJumped / MAX_HEIGHT);
            float liftOffFitness = (float)framesSpentNotTouchingGround / (float)totalFrameCount;
            return (heightFitness + liftOffFitness) / 2.0f;
        }
    }
}