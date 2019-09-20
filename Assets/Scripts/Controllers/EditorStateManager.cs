using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution;
using Keiwando.JSON;

public class EditorStateManager {

    public static EditorSettings EditorSettings {
        get => _editorSettings;
        set {
            _editorSettings = value; 
            Settings.EditorSettings = value.Encode().ToString(Formatting.None);
        } 
    } 

    public static SimulationSettings SimulationSettings {
        get => _simulationSettings;
        set {
            _simulationSettings = value;
            Settings.SimulationSettings = value.Encode().ToString(Formatting.None);;
        }
    }

    public static NeuralNetworkSettings NetworkSettings {
        get => _networkSettings;
        set {
            _networkSettings = value;
            Settings.NetworkSettings = value.Encode().ToString(Formatting.None);;
        }
    }

    public static CreatureDesign LastCreatureDesign {
        get => _lastCreatureDesign;
        set {
            _lastCreatureDesign = value;
            Settings.LastCreatureDesign = value.Encode().ToString(Formatting.None);;
        }
    }

    private static EditorSettings _editorSettings;
    private static SimulationSettings _simulationSettings;
    private static NeuralNetworkSettings _networkSettings;
    private static CreatureDesign _lastCreatureDesign;

    static EditorStateManager() {
        _editorSettings = EditorSettings.Decode(Settings.EditorSettings);
        _simulationSettings = SimulationSettings.Decode(Settings.SimulationSettings);
        _networkSettings = NeuralNetworkSettings.Decode(Settings.NetworkSettings);
        _lastCreatureDesign = CreatureSerializer.ParseCreatureDesign(Settings.LastCreatureDesign);
    }
}