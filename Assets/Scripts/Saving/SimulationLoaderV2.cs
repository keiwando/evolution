using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationLoaderV2 {

    /// <summary>
	/// Loads the simulation from save file with the format version 2
	/// </summary>
	/// <param name="filename">The Filename has to end on .txt .</param>
	/// <param name="content">The Content of the save file.</param>
	public static void LoadSimulationFromSaveFile(string filename, string content, SimulationSerializer.SplitOptions splitOptions, CreatureBuilder creatureBuilder, Evolution evolution) {

		var creatureName = filename.Split('-')[0].Replace(" ", "");

		var components = content.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		var evolutionSettings = EvolutionSettings.Decode(components[1]);
		var networkSettings = NeuralNetworkSettings.Decode(components[2]);

		var creatureData = components[3];
		CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);

		var bestChromosomesData = new List<string>(components[4].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeStats>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {
				bestChromosomes.Add(ChromosomeStats.FromString(chromosomeData));	
			}
		}

		var chromosomeComponents = components[5].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None);
		var currentChromosomes = new List<string>();

		foreach (var chromosome in chromosomeComponents) {

			if (chromosome != "") {
				currentChromosomes.Add(chromosome);
			}
		}

		var currentGeneration = bestChromosomes.Count + 1;

		evolution.Settings = evolutionSettings;

		creatureBuilder.ContinueEvolution(evolution, () => {

			CreatureSaver.SaveCurrentCreatureName(creatureName);
			evolution.ContinueEvolution(currentGeneration, evolutionSettings, networkSettings, bestChromosomes, currentChromosomes);
		});
	}
}