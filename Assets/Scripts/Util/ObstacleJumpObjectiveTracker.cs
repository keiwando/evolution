using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

    // Jump as high as possible and minimize the number of joints that 
    // touched an obstacle

    public class ObstacleJumpObjectiveTracker: ObjectiveTracker {

        private const float MAX_HEIGHT = 20f;
        private const float MAX_COLLISION_DURATION_PER_JOINT = 1f;

        /// <summary>
        /// Stores the amount of time (ms) that each joint was colliding with the obstacle
        /// </summary>
        private Dictionary<Joint, float> collisionDurations = new Dictionary<Joint, float>();
        private HashSet<Joint> collidedJoints = new HashSet<Joint>();
        private float maxHeightJumped;

        void FixedUpdate() {
            this.maxHeightJumped = Mathf.Max(creature.DistanceFromGround(), maxHeightJumped);
            creature.AddObstacleCollidingJointsToSet(collidedJoints);
            foreach (var joint in collidedJoints) {
                var duration = collisionDurations.ContainsKey(joint) ? collisionDurations[joint] : 0f;
                duration += Time.fixedDeltaTime;
                collisionDurations[joint] = duration;
            }
            collidedJoints.Clear();
        }

        public override float EvaluateFitness(float simulationTime) {
            var heightFitness = Mathf.Clamp(maxHeightJumped / MAX_HEIGHT, 0f, 1f);
            var totalCollisionDuration = 0f;
            var collidedJointsCount = 0;
            foreach (var entry in collisionDurations) {
                totalCollisionDuration += entry.Value;
                collidedJointsCount++;
            }
            var averageCollisionDuration = totalCollisionDuration / creature.joints.Count;

            var collisionFitness = 1f - Mathf.Clamp(averageCollisionDuration / MAX_COLLISION_DURATION_PER_JOINT, 0f, 1f);
            return Math.Max(collisionFitness, 0.3f * heightFitness + 0.7f * collisionFitness);
        }
    }
}