using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class SimulationSceneContext: ISceneContext {

        private readonly SimulationData data;
        
        private readonly LayerMask backgroundLayer;
        private readonly LayerMask staticForegroundLayer;
        private readonly LayerMask dynamicForegroundLayer;

        public SimulationSceneContext(SimulationData data) {
            this.data = data;
            this.backgroundLayer = LayerMask.NameToLayer("SimulationBackground");
            this.staticForegroundLayer = LayerMask.NameToLayer("StaticForeground");
            this.dynamicForegroundLayer = LayerMask.NameToLayer("DynamicForeground");
        }

        public CreatureStats GetStatsForBestOfGeneration(int generation) {
            if (generation < 1 || generation > data.BestCreatures.Count) {
                return null;
            }
            return data.BestCreatures[generation - 1].Stats;
        }

        public float GetDistanceOfBest(int generation) {
            var stats = GetStatsForBestOfGeneration(generation);
            if (stats == null) return float.NaN;
            switch (this.data.Settings.Task) {
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

        public int GetCurrentGeneration() {
            return this.data.BestCreatures.Count + 1;
        }

        public bool AreDistanceMarkersEnabled() {
            return true;
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
    }
}