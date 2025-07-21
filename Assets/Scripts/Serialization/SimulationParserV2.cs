using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution;
using Keiwando.Evolution.Scenes;

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
public class SimulationParserV2 {

    /// <summary>
	/// Loads the simulation from save file with the format version 2
	/// </summary>
	/// <param name="name">The name of the simulation save.</param>
	/// <param name="content">The Content of the save file.</param>
	public static SimulationData ParseSimulationData(string name, string content, LegacySimulationLoader.SplitOptions splitOptions) {

		var creatureName = name.Split('-')[0].Replace(" ", "");
		if (string.IsNullOrEmpty(creatureName))
			creatureName = "Unnamed";

		var components = content.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		var simulationSettings = SimulationSettings.Decode(components[1]);
		var networkSettings = NeuralNetworkSettings.Decode(components[2]);

		var creatureData = components[3];
		var creatureDesign = CreatureSerializer.ParseCreatureDesign(creatureData);
		creatureDesign.Name = creatureName;

		var bestChromosomesData = new List<string>(components[4].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeData?>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {
				var stats = ChromosomeStats.FromString(chromosomeData);
				var data = new StringChromosomeData(stats.chromosome, stats.stats);
				bestChromosomes.Add(data.ToChromosomeData());	
			}
		}

		var chromosomeComponents = components[5].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None);
		var currentChromosomes = new List<float[]>();

		foreach (var chromosome in chromosomeComponents) {

			if (chromosome != "") {
				currentChromosomes.Add(ConversionUtils.BinaryStringToFloatArray(chromosome));
			}
		}

		var sceneDescription = DefaultSimulationScenes.DefaultSceneForObjective(simulationSettings.Objective);
		sceneDescription.PhysicsConfiguration = ScenePhysicsConfiguration.Legacy;

		return new SimulationData(
			simulationSettings, networkSettings, creatureDesign, 
			sceneDescription, bestChromosomes, currentChromosomes.ToArray(),
			bestChromosomes.Count
		);
	}
}