
namespace Keiwando.Evolution {

    public class LegacySimulationOptions {

        /// <summary>
        /// For simulations up to save version 2:
        /// transform.rotation.z was used instead of transform.eulerAngles.z
        /// in order to calculate the bone rotations.
        /// </summary>
        public bool LegacyRotationCalculation = false;

        /// <summary>
        /// In the climbing task, the template creature's DistanceFromGround
        /// call always returned 0, so the drop height calculation ended up
        /// being incorrect
        /// </summary>
        public bool LegacyClimbingDropCalculation = false;
    }
}