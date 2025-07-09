using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {
    
    public static class SimulationSceneBuilder {

        public static void Build(SimulationSceneDescription scene, ISceneContext context) {
            for (int i = 0; i < scene.Structures.Length; i++) {
                IStructure structure = scene.Structures[i];
                if (structure != null) {
                   structure.GetBuilder().Build(context);
                };
            }
        }
    }
}