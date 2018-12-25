using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationLoaderV1 {

    /// <summary>
	/// Loads the simulation from save file of format version 1.
	/// </summary>
	/// <param name="filename">The Filename has to end on .txt .</param>
	/// <param name="content">The Content of the save file.</param>
	public static void LoadSimulationFromSaveFile(string filename, string content, SimulationSerializer.SplitOptions splitOptions, CreatureBuilder creatureBuilder, Evolution evolution) { 

		var creatureName = filename.Split('-')[0].Replace(" ", "");

		var components = content.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		var taskType = EvolutionTaskUtil.TaskForNumber(int.Parse(components[0].Replace(Environment.NewLine, "")));

		var timePerGen = int.Parse(components[1].Replace(Environment.NewLine, ""));

		var creatureData = components[2];
		CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);

		var bestChromosomesData = new List<string>(components[3].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeStats>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {

				var chromosomeInfo = ChromosomeInfo.FromString(chromosomeData);
				var chromosomeStats = new ChromosomeStats(chromosomeInfo.chromosome, new CreatureStats());
				chromosomeStats.stats.fitness = chromosomeInfo.fitness;

				bestChromosomes.Add(chromosomeStats);	
			}
		}

		var chromosomeComponents = components[4].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None);
		var currentChromosomes = new List<string>();

		foreach (var chromosome in chromosomeComponents) {

			if (chromosome != "") {
				currentChromosomes.Add(chromosome);
			}
		}

		var currentGeneration = bestChromosomes.Count + 1;

		var settings = new EvolutionSettings();
		settings.task = taskType;
		settings.simulationTime = timePerGen;
		settings.populationSize = currentChromosomes.Count;

		var networkSettings = new NeuralNetworkSettings();

		evolution.Settings = settings;

		creatureBuilder.ContinueEvolution(evolution, () => {

			CreatureSaver.SaveCurrentCreatureName(creatureName);
			evolution.ContinueEvolution(currentGeneration, settings, networkSettings, bestChromosomes, currentChromosomes);
		});
	}
}