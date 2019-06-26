using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution;

public class EditorStateManager {

    public static EditorSettings EditorSettings {
        get => _editorSettings;
        set {
            _editorSettings = value; 
            Settings.EditorSettings = value.Encode();
        } 
    } 

    public static SimulationSettings SimulationSettings {
        get => _simulationSettings;
        set {
            _simulationSettings = value;
            Settings.SimulationSettings = value.Encode();
        }
    }

    public static NeuralNetworkSettings NetworkSettings {
        get => _networkSettings;
        set {
            _networkSettings = value;
            Settings.NetworkSettings = value.Encode();
        }
    }

    // public static CreatureDesign CreatureDesign {
    //     get => _creatureDesign;
    //     set {
    //         _creatureDesign = value;
    //         Settings.CurrentCreatureDesign = value.Encode();
    //     }
    // }

    private static EditorSettings _editorSettings;
    private static SimulationSettings _simulationSettings;
    private static NeuralNetworkSettings _networkSettings;
    // private static CreatureDesign _creatureDesign;

    static EditorStateManager() {
        _editorSettings = EditorSettings.Decode(Settings.EditorSettings);
        _simulationSettings = SimulationSettings.Decode(Settings.SimulationSettings);
        _networkSettings = NeuralNetworkSettings.Decode(Settings.NetworkSettings);
        // _creatureDesign = CreatureSerializer.ParseCreatureDesign(Settings.CurrentCreatureDesign);
    }

    // public static EditorState Load() {

    //     return new EditorState(_creatureDesign, _simulationSettings, _networkSettings);
    // }

    // public static void Serialize(EditorState state) {
    //     Settings.SimulationSettings = state.SimulationSettings.Encode();
    //     Settings.NetworkSettings = state.NeuralNetworkSettings.Encode();
    //     Settings.CurrentCreatureDesign = state.CreatureDesign.Encode();
    // }
}