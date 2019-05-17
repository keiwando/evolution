using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution.Scenes;

[Serializable]
public class SimulationData {

    public readonly int Version = 3;

    public SimulationSettings Settings { get; set; }
    public NeuralNetworkSettings NetworkSettings { get; set; }
    public CreatureDesign CreatureDesign { get; set; }
    public SimulationScene SceneDescription { get; set; }

    public List<ChromosomeData> BestCreatures { get; set; }
    public string[] CurrentChromosomes { get; set; }

    public SimulationData(
        SimulationSettings settings, 
        NeuralNetworkSettings networkSettings, 
        CreatureDesign design, 
        SimulationScene sceneDescription
    ) {
        this.Settings = settings;
        this.NetworkSettings = networkSettings;
        this.CreatureDesign = design;
        this.SceneDescription = sceneDescription;
        this.BestCreatures = new List<ChromosomeData>();
        this.CurrentChromosomes = new string[0];
    }

    public SimulationData(
        SimulationSettings settings, 
        NeuralNetworkSettings networkSettings, 
        CreatureDesign design,
        SimulationScene sceneDescription,
        List<ChromosomeData> bestCreatures, 
        string[] currentChromosomes
    ): this(settings, networkSettings, design, sceneDescription) {
        this.BestCreatures = bestCreatures;
        this.CurrentChromosomes = currentChromosomes;
    }

    public string Encode() {
        return JsonUtility.ToJson(this);
    }
}