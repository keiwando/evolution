using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution;

public class EditorStateManager {

    public EditorSettings EditorSettings => _editorSettings;
    public SimulationSettings SimulationSettings => _simulationSettings;
    public NeuralNetworkSettings NetworkSettings => _neuralNetworkSettings;

    private static EditorSettings _editorSettings;
    private static SimulationSettings _simulationSettings;
    private static NeuralNetworkSettings _neuralNetworkSettings;
    private static CreatureDesign _creatureDesign;

    static EditorStateManager() {
        _editorSettings = LoadEditorSettings();
        _simulationSettings = LoadSimulationSettings();
        _neuralNetworkSettings = LoadNetworkSettings();
        _creatureDesign = LoadCreatureDesign();
    }

    public static EditorState Load() {

        return new EditorState(LoadCreatureDesign(), LoadSimulationSettings(), LoadNetworkSettings());
    }

    public static void Serialize(EditorState state) {
        Settings.SimulationSettings = state.SimulationSettings.Encode();
        Settings.NetworkSettings = state.NeuralNetworkSettings.Encode();
        Settings.CurrentCreatureDesign = state.CreatureDesign.Encode();
    }

    private static EditorSettings LoadEditorSettings() {
        // TODO: Replace with actual Settings key
        return EditorSettings.Decode("");
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