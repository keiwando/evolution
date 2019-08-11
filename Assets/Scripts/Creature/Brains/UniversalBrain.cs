using UnityEngine;

namespace Keiwando.Evolution {

    public class UniversalBrain: Brain {

        public override int NumberOfInputs => 14;
        public override int NumberOfOutputs {
            get { return base.NumberOfOutputs + 1; }
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
        ///     - Up-Back
        ///     - Up-Front
        ///     - Custom
        /// - dX velocity
        /// - dY velocity
        /// - angular velocity
        /// - number of points touching ground
        /// - creature rotation
        /// - custom raycast angle
        /// </summary>
        protected override void UpdateInputs() {

            var basicInputs = creature.CalculateBasicBrainInputs();
            
            var center = new Vector3(creature.GetXPosition(), creature.GetYPosition());

            var forward = creature.RaycastDistance(center, new Vector3(1, 0));
            var downForward = creature.RaycastDistance(center, new Vector3(1, -1));
            var downBack = creature.RaycastDistance(center, new Vector3(-1, -1));
            var back = creature.RaycastDistance(center, new Vector3(-1, 0));
            var upBack = creature.RaycastDistance(center, new Vector3(-1, 1));
            var upFront = creature.RaycastDistance(center, new Vector3(1, 1));
            var custom = creature.RaycastDistance(center, new Vector3(Mathf.Cos(customRaycastAngle), Mathf.Sin(customRaycastAngle)));

            var inputs = Network.Inputs;
            inputs[0] = creature.DistanceFromGround();
            inputs[1] = forward;
            inputs[2] = downForward;
            inputs[3] = downBack;
            inputs[4] = back;
            inputs[5] = upBack;
            inputs[6] = upFront;
            inputs[7] = custom;

            inputs[8] = basicInputs.VelocityX;
            inputs[8] = basicInputs.VelocityY;
            inputs[10] = basicInputs.AngularVelocity;
            inputs[11] = basicInputs.PointsTouchingGroundCount;
            inputs[12] = basicInputs.Rotation;
            inputs[13] = customRaycastAngle;
        }

        protected override void ApplyOutputs(float[] outputs) {
            base.ApplyOutputs(outputs);

            customRaycastAngle = outputs[outputs.Length - 1];
        }
    }
}