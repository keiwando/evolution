using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The EvolutionSaver provides function for saving and loading the state of a simulation in / from a file.
/// 
/// The Evolution Save files have the following format:
/// Filename: CreatureName - Date - Generation
/// 
/// Content: 
/// 
/// simulation task
/// -separator-
/// time per generation
/// -separator-
/// CreatureSaveData - created by the CreatureSaver class
/// -separator-
/// A list of the best chromosomes (for each chromosome:  chromosome : fitness)
/// -separator-
/// A list of the current chromosomes (The chromosomes of all creatures of the last simulating generation)
/// </summary>
public class EvolutionSaver {

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "EvolutionSaves";
	/// <summary>
	/// The separator to use in the save file between the file components such as the creature data,
	/// and the chromosome lists. 
	/// </summary>
	private const string COMPONENT_SEPARATOR = "--?%%%?--\n";
	/// <summary>
	/// Used for splitting the text file by the body component types.
	/// </summary>
	private static string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };

	//private static string CURRENT_SAVE_KEY = "_CurrentCreatureSave";
	//private static string CREATURE_NAMES_KEY = "_CreatureNames";

	private static string RESOURCE_PATH = Path.Combine(Application.dataPath, "Resources");

	/// <summary>
	/// Saves the given information about an evolution simulation of a creature in a file, so that
	/// it can be loaded and continued at the same generation again.
	/// The filename cannot contain dots (.)
	/// Throws: IllegalFilenameException
	/// </summary>
	public static void WriteSaveFile(string creatureName, Evolution.Task task, int timePerGen, int generationNumber, string creatureSaveData, List<ChromosomeInfo> bestChromosomes, List<string> currentChromosomes) {

		var date = System.DateTime.Now.ToString("yyyy-MM-dd");
		var taskName = Evolution.TaskToString(task);

		var filename = string.Format("{0} - {1} - {2} - Gen:{3}.txt", creatureName, taskName, date, generationNumber);

		var stringBuilder = new StringBuilder();

		// Add the task type
		stringBuilder.AppendLine(((int)task).ToString());
		stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the time per generation
		stringBuilder.AppendLine(timePerGen.ToString());
		stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the creature save data 
		stringBuilder.AppendLine(creatureSaveData);
		stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the list of best chromosomes
		foreach (var chromosome in bestChromosomes) {
			stringBuilder.AppendLine(chromosome.ToString());
		}
		stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the list of current chromosomes
		foreach (var chromosome in currentChromosomes) {
			stringBuilder.AppendLine(chromosome);
		}
		stringBuilder.Append(COMPONENT_SEPARATOR);

		var path = Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		File.WriteAllText(path, stringBuilder.ToString());
	}

	public static void LoadSimulationFromSaveFile(string filename, CreatureBuilder creatureBuilder, Evolution evolution) {

		if (!filename.EndsWith(".txt")) {
			filename += ".txt";
		}

		// check if the file exists
		var path = Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();

		var components = contents.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		var taskType = Evolution.TaskForNumber(int.Parse(components[0].Replace("\n", "")));

		var timePerGen = int.Parse(components[1].Replace("\n", ""));

		var creatureData = components[2];
		CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);

		var bestChromosomesData = new List<string>(components[3].Split('\n'));
		var bestChromosomes = new List<ChromosomeInfo>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {
				bestChromosomes.Add(ChromosomeInfo.FromString(chromosomeData));	
			}
		}

		var chromosomeComponents = components[4].Split('\n');
		var currentChromosomes = new List<string>();

		foreach (var chromosome in chromosomeComponents) {

			if (chromosome != "") {
				currentChromosomes.Add(chromosome);
			}
		}

		var currentGeneration = bestChromosomes.Count + 1;

		Evolution.task = taskType;

		creatureBuilder.ContinueEvolution(evolution, () => {

			evolution.ContinueEvolution(currentGeneration, timePerGen, bestChromosomes, currentChromosomes);
		});
	}

	/// <summary>
	/// Returns a list of filenames containing simulation save data. 
	/// </summary>
	/// <returns>The evolution save filenames.</returns>
	public static List<string> GetEvolutionSaveFilenames() {

		var info = new DirectoryInfo(Path.Combine(RESOURCE_PATH, SAVE_FOLDER));
		var fileInfo = info.GetFiles();
		var names = new HashSet<string>();

		foreach (FileInfo file in fileInfo) {

			if (file.Name.Contains(".txt")) {

				names.Add(file.Name.Split('.')[0]);
			}
		} 

		var filenames = new List<string>();
		foreach (string name in names) {
			filenames.Add(name);
		}

		return filenames;
	}
}
