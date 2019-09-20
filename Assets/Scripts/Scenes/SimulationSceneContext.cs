using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class SimulationSceneContext: BaseSceneContext, ISceneContext {
        
        protected override string backgroundLayerName => "SimulationBackground";
        protected override string staticForegroundLayerName => "StaticForeground";
        protected override string dynamicForegroundLayerName => "DynamicForeground";

        public SimulationSceneContext(SimulationData data): base(data) {}
    }
}