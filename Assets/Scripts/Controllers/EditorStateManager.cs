using System;
using UnityEngine;

public class EditorStateManager {

    public static EditorState LoadFromSerialization() {

        var simulationSettings = LoadSimulationSettings();
        var networkSettings = LoadNetworkSettings();
        var creatureDesign = LoadCreatureDesign();

        return new EditorState () {
            SimulationSettings = simulationSettings,
            NeuralNetworkSettings = networkSettings,
            CreatureDesign = creatureDesign
        };
    }

    public static void Serialize(EditorState state) {
        // TODO: Implement
    }

    private static SimulationSettings LoadSimulationSettings() {

        // TODO: Load from PlayerPrefs
        return new SimulationSettings();
    }

    private static NeuralNetworkSettings LoadNetworkSettings() {
        // TODO: Load from PlayerPrefs
        return new NeuralNetworkSettings();
    }

    private static CreatureDesign LoadCreatureDesign() {
        // TODO: Load from PlayerPrefs
        return new CreatureDesign();
    }
}