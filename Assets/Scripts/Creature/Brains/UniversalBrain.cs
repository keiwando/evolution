using UnityEngine;

namespace Keiwando.Evolution {

    public class UniversalBrain: Brain {

        private const float MAX_RAYCAST_DISTANCE = 20f;
        private const float ANGLE_SMOOTHING_WEIGHT = 0.1f;
        public const int NUMBER_OF_INPUTS = 11;
        public const int NUMBER_OF_ADDITIONAL_OUTPUTS = 1;

        public override int NumberOfInputs => NUMBER_OF_INPUTS;
        public override int NumberOfOutputs {
            get { return base.NumberOfOutputs + NUMBER_OF_ADDITIONAL_OUTPUTS; }
        }

        private float customRaycastAngle = 0f;

        /// <summary>
        /// Inputs:
        /// 
        /// - Raycast Distances:
        ///     - Distance to ground
        ///     - Forward
        ///     - Down-Forward
        ///     - Down-Back
        ///     - Back
        ///     - Custom
        /// - dX velocity
        /// - dY velocity
        /// - angular velocity
        /// - number of points touching ground
        /// - creature rotation
        /// </summary>
        protected override void UpdateInputs() {

            var basicInputs = creature.CalculateBasicBrainInputs();
            
            var center = new Vector3(creature.GetXPosition(), creature.GetYPosition());

            var forward = creature.RaycastDistance(center, new Vector3(1, 0), MAX_RAYCAST_DISTANCE);
            var downForward = creature.RaycastDistance(center, new Vector3(1, -1), MAX_RAYCAST_DISTANCE);
            var downBack = creature.RaycastDistance(center, new Vector3(-1, -1), MAX_RAYCAST_DISTANCE);
            var back = creature.RaycastDistance(center, new Vector3(-1, 0), MAX_RAYCAST_DISTANCE);

            var direction = new Vector3(Mathf.Cos(customRaycastAngle), Mathf.Sin(customRaycastAngle));
            // Debug.DrawRay(center, direction.normalized * MAX_RAYCAST_DISTANCE, Color.magenta);
            var custom = creature.RaycastDistance(center, direction, MAX_RAYCAST_DISTANCE);

            var inputs = Network.Inputs;
            inputs[0] = creature.DistanceFromGround();
            inputs[1] = forward;
            inputs[2] = downForward;
            inputs[3] = downBack;
            inputs[4] = back;
            inputs[5] = custom;

            inputs[6] = basicInputs.VelocityX;
            inputs[7] = basicInputs.VelocityY;
            inputs[8] = basicInputs.AngularVelocity;
            inputs[9] = basicInputs.PointsTouchingGroundCount;
            inputs[10] = basicInputs.Rotation;
        }

        protected override void ApplyOutputs(float[] outputs) {
            base.ApplyOutputs(outputs);

            var newRaycastAngle = outputs[outputs.Length - 1] * 2 * Mathf.PI;
            customRaycastAngle = ANGLE_SMOOTHING_WEIGHT * newRaycastAngle + (1f - ANGLE_SMOOTHING_WEIGHT) * customRaycastAngle;
        }
    }
}