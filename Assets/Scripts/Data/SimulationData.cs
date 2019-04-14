using System;
using System.Collections.Generic;

[Serializable]
public class SimulationData {

    public readonly int Version = 3;

    public SimulationSettings Settings { get; set; }
    public NeuralNetworkSettings NetworkSettings { get; set; }
    public CreatureDesign CreatureDesign { get; set; }

    public List<ChromosomeData> BestCreatures { get; set; }
    public string[] CurrentChromosomes { get; set; }

    public SimulationData(SimulationSettings settings, NeuralNetworkSettings networkSettings, CreatureDesign design) {
        this.Settings = settings;
        this.NetworkSettings = networkSettings;
        this.CreatureDesign = design;
        this.BestCreatures = new List<ChromosomeData>();
        this.CurrentChromosomes = new string[settings.populationSize];
    }

    public SimulationData(SimulationSettings settings, NeuralNetworkSettings networkSettings, CreatureDesign design,
                          List<ChromosomeData> bestCreatures, string[] currentChromosomes) 
                          : this(settings, networkSettings, design) {
        this.BestCreatures = bestCreatures;
        this.CurrentChromosomes = currentChromosomes;
    }
}