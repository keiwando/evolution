using UnityEngine;

namespace Keiwando.Evolution {

    [RequireComponent(typeof(Creature))]
    public abstract class ObjectiveTracker: MonoBehaviour, IObjectiveTracker {

        protected Creature creature;

        public virtual void Start() {
            this.creature = GetComponent<Creature>();
        }

        public abstract float EvaluateFitness(float simulationTime);
    }
}