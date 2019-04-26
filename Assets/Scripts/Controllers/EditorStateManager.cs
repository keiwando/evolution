using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorStateManager {

    public static EditorState Load() {

        return new EditorState(LoadCreatureDesign(), LoadSimulationSettings(), LoadNetworkSettings());
    }

    public static void Serialize(EditorState state) {
        Settings.SimulationSettings = state.SimulationSettings.Encode();
        Settings.NetworkSettings = state.NeuralNetworkSettings.Encode();
        Settings.CurrentCreatureDesign = state.CreatureDesign.Encode();
    }

    private static SimulationSettings LoadSimulationSettings() {

        return SimulationSettings.Decode(Settings.SimulationSettings);
    }

    private static NeuralNetworkSettings LoadNetworkSettings() {
        return NeuralNetworkSettings.Decode(Settings.NetworkSettings);
    }

    private static CreatureDesign LoadCreatureDesign() {
        return CreatureSerializer.ParseCreatureDesign(Settings.CurrentCreatureDesign);
    }
}