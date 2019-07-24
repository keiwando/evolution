using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.JSON;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

    [Serializable]
    public class SimulationData {

        public int Version { get; private set; } = 3;

        public SimulationSettings Settings { get; set; }
        public NeuralNetworkSettings NetworkSettings { get; set; }
        public CreatureDesign CreatureDesign { get; set; }
        public SimulationSceneDescription SceneDescription { get; set; }

        public List<ChromosomeData> BestCreatures { get; set; }
        public string[] CurrentChromosomes { get; set; }

        public readonly int LastV2SimulatedGeneration;

        public SimulationData(
            SimulationSettings settings, 
            NeuralNetworkSettings networkSettings, 
            CreatureDesign design, 
            SimulationSceneDescription sceneDescription
        ) {
            this.Settings = settings;
            this.NetworkSettings = networkSettings;
            this.CreatureDesign = design;
            this.SceneDescription = sceneDescription;
            this.BestCreatures = new List<ChromosomeData>();
            this.CurrentChromosomes = new string[0];
            this.LastV2SimulatedGeneration = 0;
        }

        public SimulationData(
            SimulationSettings settings, 
            NeuralNetworkSettings networkSettings, 
            CreatureDesign design,
            SimulationSceneDescription sceneDescription,
            List<ChromosomeData> bestCreatures, 
            string[] currentChromosomes,
            int lastV2SimulatedGeneration = 0
        ): this(settings, networkSettings, design, sceneDescription) {
            this.BestCreatures = bestCreatures;
            this.CurrentChromosomes = currentChromosomes;
            this.LastV2SimulatedGeneration = lastV2SimulatedGeneration;
        }

        #region Encode & Decode

        private static class CodingKey {
            public const string Version = "version";
            public const string Settings = "simulationSettings";
            public const string NetworkSettings = "networkSettings";
            public const string CreatureDesign = "creatureDesign";
            public const string SceneDescription = "scene";
            public const string BestCreatures = "bestCreatures";
            public const string CurrentChromosomes = "currentChromosomes";
            public const string LastV2SimulatedGeneration = "lastV2SimulationGeneration";
        }

        public JObject Encode() {

            JObject json = new JObject();
            
            json[CodingKey.Version] = this.Version;
            json[CodingKey.Settings] = this.Settings.Encode();
            json[CodingKey.NetworkSettings] = this.NetworkSettings.Encode();
            json[CodingKey.CreatureDesign] = this.CreatureDesign.Encode();
            json[CodingKey.SceneDescription] = this.SceneDescription.Encode();
            json[CodingKey.BestCreatures] = JArray.From(this.BestCreatures);
            json[CodingKey.CurrentChromosomes] = this.CurrentChromosomes;
            json[CodingKey.LastV2SimulatedGeneration] = this.LastV2SimulatedGeneration;

            return json;
        }

        public static SimulationData Decode(string encoded) {
            return Decode(JObject.Parse(encoded));   
        }

        public static SimulationData Decode(JObject json) {

            return new SimulationData(
                json[CodingKey.Settings].Decode(SimulationSettings.Decode),
                json[CodingKey.NetworkSettings].Decode(NeuralNetworkSettings.Decode),
                json[CodingKey.CreatureDesign].Decode(CreatureDesign.Decode),
                json[CodingKey.SceneDescription].Decode(SimulationSceneDescription.Decode),
                json[CodingKey.BestCreatures].ToList(ChromosomeData.Decode),
                json[CodingKey.CurrentChromosomes].ToStringArray(),
                json[CodingKey.LastV2SimulatedGeneration].ToInt()
            );
        }

        #endregion
    }
}