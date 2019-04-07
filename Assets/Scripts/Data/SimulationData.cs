using System;
using System.Collections.Generic;

[Serializable]
public class SimulationData {

    public readonly int Version = 3;

    public SimulationSettings Settings;
    public CreatureDesign CreatureDesign;

    public List<ChromosomeData> BestCreatures;
    public string[] CurrentChromosomes;
}