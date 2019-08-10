using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

    // Jump as high as possible and minimize the number of joints that 
    // touched an obstacle

    public class ObstacleJumpObjectiveTracker: ObjectiveTracker {

        private const float MAX_HEIGHT = 20f;

        private HashSet<Joint> collidedJoints = new HashSet<Joint>();
        private float maxHeightJumped;

        void FixedUpdate() {
            this.maxHeightJumped = Mathf.Max(creature.DistanceFromGround(), maxHeightJumped);
            creature.AddObstacleCollidingJointsToSet(collidedJoints);
        }

        public override float EvaluateFitness(float simulationTime) {
            var heightFitness = Mathf.Clamp(maxHeightJumped / MAX_HEIGHT, 0f, 1f);
            var collisionFitness = 1f - Mathf.Clamp((float) collidedJoints.Count / creature.joints.Count, 0f, 1f);
            return 0.5f * (heightFitness + collisionFitness);
        }
    }
}