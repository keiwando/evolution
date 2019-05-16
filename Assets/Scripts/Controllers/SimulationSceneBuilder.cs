using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {
    
    public static class SimulationSceneBuilder {

        public static void Build(SimulationScene scene) {
            for (int i = 0; i < scene.Structures.Length; i++) {

                scene.Structures[i].GetBuilder().Build();
            }
        }
    }
}