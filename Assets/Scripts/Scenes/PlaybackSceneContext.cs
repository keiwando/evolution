using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class PlaybackSceneContext: ISceneContext {

        private readonly SimulationData data;
        private readonly BestCreaturesController controller;

        private readonly LayerMask backgroundLayer;
        private readonly LayerMask staticForegroundLayer;
        private readonly LayerMask dynamicForegroundLayer;

        public PlaybackSceneContext(SimulationData data, BestCreaturesController controller) {
            this.data = data;
            this.controller = controller;
            this.backgroundLayer = LayerMask.NameToLayer("PlaybackBackground");
            this.staticForegroundLayer = LayerMask.NameToLayer("PlaybackStaticForeground");
            this.dynamicForegroundLayer = LayerMask.NameToLayer("PlaybackDynamicForeground");
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
            case EvolutionTask.Jumping:
            case EvolutionTask.ObstacleJump: 
            case EvolutionTask.Climbing:
                return stats.verticalDistanceTravelled;
            }
            return stats.horizontalDistanceTravelled;
        }

        public int GetCurrentGeneration() {
            return this.controller.CurrentGeneration;
        }

        public bool AreDistanceMarkersEnabled() {
            return false;
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