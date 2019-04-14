using System;

[Serializable]
public struct ChromosomeData {

    public readonly string Chromosome;
    public readonly CreatureStats Stats;

    public ChromosomeData(string chromosome, CreatureStats stats) {
        this.Chromosome = chromosome;
        this.Stats = stats;
    } 
}