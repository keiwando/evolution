using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public abstract class BaseSceneContext {

        protected readonly SimulationData data;

        protected abstract string backgroundLayerName { get; }
        protected abstract string staticForegroundLayerName { get; }
        protected abstract string dynamicForegroundLayerName { get; }

        private readonly LayerMask backgroundLayer;
        private readonly LayerMask staticForegroundLayer;
        private readonly LayerMask dynamicForegroundLayer;

        public BaseSceneContext(SimulationData data) {
            this.data = data;
            this.backgroundLayer = LayerMask.NameToLayer(backgroundLayerName);
            this.staticForegroundLayer = LayerMask.NameToLayer(staticForegroundLayerName);
            this.dynamicForegroundLayer = LayerMask.NameToLayer(dynamicForegroundLayerName);
        }

        public CreatureStats GetStatsForBestOfGeneration(int generation) {
            if (generation < 1 || generation > data.BestCreatures.Count) {
                return null;
            }
            return data.BestCreatures[generation - 1].Stats;
        }

        public virtual float GetDistanceOfBest() {

            if (data.BestCreatures.Count == 0) return float.NaN;

            float bestDistance = 0f;

            for (int i = 0; i < data.BestCreatures.Count; i++) {
                float distance = 0f;
                var stats = data.BestCreatures[i].Stats;
                distance = GetDistanceForTask(stats, this.data.Settings.Task);
                if (distance > bestDistance) {
                    bestDistance = distance;
                }
            }
            
            return bestDistance;
        }

        public LayerMask GetBackgroundLayer() {
            return backgroundLayer;
        }

        public LayerMask GetDynamicForegroundLayer() {
            return dynamicForegroundLayer;
        }

        public LayerMask GetStaticForegroundLayer() {
            return staticForegroundLayer;
        }

        protected static float GetDistanceForTask(CreatureStats stats, EvolutionTask task) {
            if (stats == null) return float.NaN;
            switch (task) {
            case EvolutionTask.Running: 
                return stats.horizontalDistanceTravelled;
            case EvolutionTask.ObstacleJump: 
            case EvolutionTask.Climbing:
                return stats.verticalDistanceTravelled;
            case EvolutionTask.Jumping:
                return stats.maxJumpingHeight;
            }
            return stats.horizontalDistanceTravelled;
        }
    }
}