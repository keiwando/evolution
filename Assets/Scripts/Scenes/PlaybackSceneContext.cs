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

    public class GalleryPlaybackSceneContext: ISceneContext {

        protected string backgroundLayerName => "PlaybackBackground";
        protected string staticForegroundLayerName => "PlaybackStaticForeground";
        protected string dynamicForegroundLayerName => "PlaybackDynamicForeground";

        private CreatureStats stats;
        private Objective task;

        private readonly LayerMask backgroundLayer;
        private readonly LayerMask staticForegroundLayer;
        private readonly LayerMask dynamicForegroundLayer;

        public GalleryPlaybackSceneContext(CreatureStats stats, Objective task) {
            this.stats = stats;
            this.task = task;
            this.backgroundLayer = LayerMask.NameToLayer(backgroundLayerName);
            this.staticForegroundLayer = LayerMask.NameToLayer(staticForegroundLayerName);
            this.dynamicForegroundLayer = LayerMask.NameToLayer(dynamicForegroundLayerName);
        }
        
        public LayerMask GetBackgroundLayer() { return backgroundLayer; }
        public LayerMask GetStaticForegroundLayer() { return staticForegroundLayer; }
        public LayerMask GetDynamicForegroundLayer() { return dynamicForegroundLayer; }

        public CreatureStats GetStatsForBestOfGeneration(int generation) {
            return stats;
        }

        public float GetDistanceOfBest() {
            return BaseSceneContext.GetDistanceForObjective(stats, task);
        }
    }
}