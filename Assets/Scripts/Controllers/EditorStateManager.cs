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
        
        try {
            _editorSettings = EditorSettings.Decode(Settings.EditorSettings);
        } catch {
            Settings.EditorSettings = EditorSettings.Default.Encode().ToString(Formatting.None);
            _editorSettings = EditorSettings.Default;
        }
        
        try {
            _simulationSettings = SimulationSettings.Decode(Settings.SimulationSettings);
        } catch {
            Settings.SimulationSettings = SimulationSettings.Default.Encode().ToString(Formatting.None);
            _simulationSettings = SimulationSettings.Default;
        }
        
        try {
            _networkSettings = NeuralNetworkSettings.Decode(Settings.NetworkSettings);
        } catch {
            Settings.NetworkSettings = NeuralNetworkSettings.Default.Encode().ToString(Formatting.None);
            _networkSettings = NeuralNetworkSettings.Default;
        }
        
        try {
            _lastCreatureDesign = CreatureSerializer.ParseCreatureDesign(Settings.LastCreatureDesign);
        } catch {
            Settings.LastCreatureDesign = CreatureDesign.Empty.Encode().ToString(Formatting.None);
            _lastCreatureDesign = CreatureDesign.Empty;
        }
    }
}