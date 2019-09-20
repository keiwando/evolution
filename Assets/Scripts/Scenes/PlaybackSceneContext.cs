using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class PlaybackSceneContext: BaseSceneContext, ISceneContext {

        protected override string backgroundLayerName => "PlaybackBackground";
        protected override string staticForegroundLayerName => "PlaybackStaticForeground";
        protected override string dynamicForegroundLayerName => "PlaybackDynamicForeground";

        private readonly BestCreaturesController controller;

        public PlaybackSceneContext(SimulationData data, BestCreaturesController controller): base(data) {
            this.controller = controller;
        }

        public override float GetDistanceOfBest() {
            var currentGeneration = GetCurrentGeneration();
            if (currentGeneration == 0) return float.NaN;

            var stats = GetStatsForBestOfGeneration(currentGeneration);
            return BaseSceneContext.GetDistanceForObjective(stats, data.Settings.Objective);
        }
        
        private int GetCurrentGeneration() {
            return this.controller.CurrentGeneration;
        }
    }
}