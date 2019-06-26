// TODO: Remove
// using UnityEngine;
// using System;

// [Serializable]
// public struct EditorState {

//     public CreatureDesign CreatureDesign;
//     public SimulationSettings SimulationSettings;
//     public NeuralNetworkSettings NeuralNetworkSettings;
    
//     public static EditorState Default = new EditorState(new CreatureDesign(), SimulationSettings.Default, NeuralNetworkSettings.Default);

//     public EditorState(CreatureDesign design, SimulationSettings simulationSettings, NeuralNetworkSettings networkSettings) {
//         this.CreatureDesign = design;
//         this.SimulationSettings = simulationSettings;
//         this.NeuralNetworkSettings = networkSettings;
//     }

//     #region Encode & Decode

//     public string Encode() {
//         return JsonUtility.ToJson(this);
//     }

//     public static EditorState Decode(string encoded) {

//         if (string.IsNullOrEmpty(encoded))
//             return Default;
            
//         return (EditorState)JsonUtility.FromJson(encoded, typeof(EditorState));
//     }

//     #endregion
// }