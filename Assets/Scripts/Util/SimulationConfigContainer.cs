using UnityEngine;
using System;

namespace Keiwando.Evolution {

    public struct SimulationOptions {
        public Action<Evolution.Solution[]> onEvaluatedSolutions;
        public string loadedFromSimulationFilePath;
    }

    public struct SimulationConfig {
        public SimulationData SimulationData;
        public SimulationOptions Options;
    }

    public class SimulationConfigContainer: MonoBehaviour {

        public SimulationConfig Config;
    }
}