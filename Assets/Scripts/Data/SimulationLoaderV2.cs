using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Evolution Save files have the following format (VERSION 2):
/// Filename: CreatureName - Date - Generation
/// 
/// Content: 
/// 
/// v save format version (v 2)
/// -separator-
/// Encoded Evolution Settings
/// -separator-
/// Encoded Neural Network Settings
/// -separator-
/// CreatureSaveData - created by the CreatureSaver class
/// -separator-
/// A list of the best chromosomes (for each chromosome:  chromosome : (CreatureStats encoded))
/// -separator-
/// A list of the current chromosomes (The chromosomes of all creatures of the last simulating generation)
/// </summary>
public class SimulationLoaderV2 {

    /// <summary>
	/// Loads the simulation from save file with the format version 2
	/// </summary>
	/// <param name="name">The name of the simulation save.</param>
	/// <param name="content">The Content of the save file.</param>
	public static void LoadSimulationFromSaveFile(string name, string content, SimulationSerializer.SplitOptions splitOptions, CreatureBuilder creatureBuilder, Evolution evolution) {

		var creatureName = name.Split('-')[0].Replace(" ", "");
		if (string.IsNullOrEmpty(creatureName))
			creatureName = "Unnamed";

		var components = content.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		var SimulationSettings = SimulationSettings.Decode(components[1]);
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

		evolution.Settings = SimulationSettings;

		creatureBuilder.ContinueEvolution(evolution, () => {

			CreatureSaver.SaveCurrentCreatureName(creatureName);
			evolution.ContinueEvolution(currentGeneration, SimulationSettings, networkSettings, bestChromosomes, currentChromosomes);
		});
	}
}