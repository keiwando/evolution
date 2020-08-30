
namespace Keiwando.Evolution {

    public interface IObjectiveTracker {

        /// <summary>
        /// Returns the unclamped fitness value of the tracked creature.
        /// </summary>
        /// <param name="simulationTime">The total amount of time that this creature was simulated for.</param>
        /// <returns></returns>
        float EvaluateFitness(float simulationTime);
    }
}