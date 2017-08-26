using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The EvolutionSaver provides function for saving and loading the state of a simulation in / from a file.
/// 
/// The Evolution Save files have the following format (VERSION 1):
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
/// 
/// -------------------------------------------------------------------------------------------------------
/// 
/// /// The Evolution Save files have the following format (VERSION 2):
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
public class EvolutionSaver {

	/// <summary>
	/// The current save file format version. This number has nothing to do with the Application.version.
	/// It should be the first line of every savefile prepended by a v so that it can be immediately identified how to interpret
	/// the rest of the file.
	/// The first savefiles do not contain a version number (1), but instead immediately start with the simulation task as an int, 
	/// which is how they can be differentiated from the rest (They don't start with a v).
	/// </summary>
	private static int version = 2;

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "EvolutionSaves";

	/// <summary>
	/// The separator to use in the save file between the file components such as the creature data,
	/// and the chromosome lists. 
	/// </summary>
	private static readonly string COMPONENT_SEPARATOR = "--?%%%?--" + Environment.NewLine;
	/// <summary>
	/// Used for splitting the text file by the body component types.
	/// </summary>
	private static string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };
	private static string[] NEWLINE_SPLIT = new string[] { Environment.NewLine };

	//private static string CURRENT_SAVE_KEY = "_CurrentCreatureSave";
	//private static string CREATURE_NAMES_KEY = "_CreatureNames";

	private static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	/// <summary>
	/// Saves the given information about an evolution simulation of a creature in a file, so that
	/// it can be loaded and continued at the same generation again.
	/// The filename cannot contain dots (.)
	/// Throws: IllegalFilenameException
	/// 
	/// Returns: The filename of the saved file.
	/// </summary>
	//public static string WriteSaveFile(string creatureName, Evolution.Task task, int timePerGen, int generationNumber, string creatureSaveData, List<ChromosomeInfo> bestChromosomes, List<string> currentChromosomes) {
	public static string WriteSaveFile(string creatureName, EvolutionSettings settings, NeuralNetworkSettings networkSettings, int generationNumber, string creatureSaveData, List<ChromosomeStats> bestChromosomes, List<string> currentChromosomes) {

		var date = System.DateTime.Now.ToString("yyyy-MM-dd");
		//var taskName = Evolution.TaskToString(task);

		var filename = string.Format("{0} - {1} - {2} - Gen({3}).txt", creatureName, settings.task, date, generationNumber);    // MAYBE IMPORTANT FOR THE FUTURE: Changed from Gen:i to Gen(i)

		var stringBuilder = new StringBuilder();

		// Add the task type
		//stringBuilder.AppendLine(((int)task).ToString());

		// Add the version number
		stringBuilder.AppendLine(string.Format("v {0}", version.ToString()));
		stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the time per generation
		//stringBuilder.AppendLine(settings.simulationTime.ToString());
		//stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the encoded evolution settings
		stringBuilder.AppendLine(settings.Encode());
		stringBuilder.Append(COMPONENT_SEPARATOR);

		// Add the encoded neural network settings
		stringBuilder.AppendLine(networkSettings.Encode());
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

		var path = RESOURCE_PATH; //Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		CreateSaveFolder();
		File.WriteAllText(path, stringBuilder.ToString());

		return filename;
	}

	public static void LoadSimulationFromSaveFile(string filename, CreatureBuilder creatureBuilder, Evolution evolution) {

		if (!filename.EndsWith(".txt")) {
			filename += ".txt";
		}

		//var creatureName = filename.Split('-')[0].Replace(" ", "");

		// check if the file exists
		var path = RESOURCE_PATH; //Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();

		var components = contents.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		// determine the version of the save file.
		// if the first line doesn't start with a v the version is 1

		if (components[0].ToUpper()[0] == 'V') {

			var version = int.Parse(components[0].Split(' ')[1]);

			if (version == 2) {

				LoadSimulationFromSaveFileV2(filename, contents, creatureBuilder, evolution);

			} else {
				throw new System.Exception("Unknown Save file format!");
			}
			
		} else {
			// V1
			LoadSimulationFromSaveFileV1(filename, contents, creatureBuilder, evolution);
		}

		/*
		try {
			var version = int.Parse(components[0]);

			if (version == 2) {

				LoadSimulationFromSaveFileV2(filename, contents, creatureBuilder, evolution);
			
			} else {
				throw new System.Exception("Unknown Save file format!");
			}

		} catch {
			LoadSimulationFromSaveFileV1(filename, contents, creatureBuilder, evolution);
		}*/

		/*var taskType = Evolution.TaskForNumber(int.Parse(components[0].Replace(Environment.NewLine, "")));

		var timePerGen = int.Parse(components[1].Replace(Environment.NewLine, ""));

		var creatureData = components[2];
		CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);

		var bestChromosomesData = new List<string>(components[3].Split(NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeInfo>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {
				bestChromosomes.Add(ChromosomeInfo.FromString(chromosomeData));	
			}
		}

		var chromosomeComponents = components[4].Split(NEWLINE_SPLIT, StringSplitOptions.None);
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

		//evolution.Settings.task = taskType;
		//evolution.Settings = settings;

		creatureBuilder.ContinueEvolution(evolution, () => {

			CreatureSaver.SaveCurrentCreatureName(creatureName);
			//evolution.ContinueEvolution(currentGeneration, timePerGen, bestChromosomes, currentChromosomes);
			evolution.ContinueEvolution(currentGeneration, settings, networkSettings, bestChromosomes, currentGeneration);
		});*/
	}

	/// <summary>
	/// Loads the simulation from save file of format version 1.
	/// </summary>
	/// <param name="filename">The Filename has to end on .txt .</param>
	/// <param name="content">The Content of the save file.</param>
	private static void LoadSimulationFromSaveFileV1(string filename, string content, CreatureBuilder creatureBuilder, Evolution evolution) { 

		var creatureName = filename.Split('-')[0].Replace(" ", "");

		// check if the file exists
		/*var path = RESOURCE_PATH; //Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();*/

		//var components = contents.Split(SPLIT_ARRAY, System.StringSplitOptions.None);
		var components = content.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		var taskType = Evolution.TaskForNumber(int.Parse(components[0].Replace(Environment.NewLine, "")));

		var timePerGen = int.Parse(components[1].Replace(Environment.NewLine, ""));

		var creatureData = components[2];
		CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);

		var bestChromosomesData = new List<string>(components[3].Split(NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeStats>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {

				var chromosomeInfo = ChromosomeInfo.FromString(chromosomeData);
				var chromosomeStats = new ChromosomeStats(chromosomeInfo.chromosome, new CreatureStats());
				chromosomeStats.stats.fitness = chromosomeInfo.fitness;

				bestChromosomes.Add(chromosomeStats);	
			}
		}

		var chromosomeComponents = components[4].Split(NEWLINE_SPLIT, StringSplitOptions.None);
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

		//evolution.Settings.task = taskType;
		evolution.Settings = settings;

		creatureBuilder.ContinueEvolution(evolution, () => {

			CreatureSaver.SaveCurrentCreatureName(creatureName);
			//evolution.ContinueEvolution(currentGeneration, timePerGen, bestChromosomes, currentChromosomes);
			evolution.ContinueEvolution(currentGeneration, settings, networkSettings, bestChromosomes, currentChromosomes);
		});
	}

	/// <summary>
	/// Loads the simulation from save file with the format version 2
	/// </summary>
	/// <param name="filename">The Filename has to end on .txt .</param>
	/// <param name="content">The Content of the save file.</param>
	private static void LoadSimulationFromSaveFileV2(string filename, string content, CreatureBuilder creatureBuilder, Evolution evolution) {

		var creatureName = filename.Split('-')[0].Replace(" ", "");

		var components = content.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		var evolutionSettings = EvolutionSettings.Decode(components[1]);
		var networkSettings = NeuralNetworkSettings.Decode(components[2]);

		var creatureData = components[3];
		CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);

		var bestChromosomesData = new List<string>(components[4].Split(NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeStats>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {
				bestChromosomes.Add(ChromosomeStats.FromString(chromosomeData));	
			}
		}

		var chromosomeComponents = components[5].Split(NEWLINE_SPLIT, StringSplitOptions.None);
		var currentChromosomes = new List<string>();

		foreach (var chromosome in chromosomeComponents) {

			if (chromosome != "") {
				currentChromosomes.Add(chromosome);
			}
		}

		var currentGeneration = bestChromosomes.Count + 1;

		//evolution.Settings.task = taskType;
		evolution.Settings = evolutionSettings;

		creatureBuilder.ContinueEvolution(evolution, () => {

			CreatureSaver.SaveCurrentCreatureName(creatureName);
			//evolution.ContinueEvolution(currentGeneration, timePerGen, bestChromosomes, currentChromosomes);
			evolution.ContinueEvolution(currentGeneration, evolutionSettings, networkSettings, bestChromosomes, currentChromosomes);
		});
	}

	/// <summary>
	/// Returns a list of filenames containing simulation save data. 
	/// </summary>
	/// <returns>The evolution save filenames.</returns>
	public static List<string> GetEvolutionSaveFilenames() {

		CreateSaveFolder();

		//var info = new DirectoryInfo(Path.Combine(RESOURCE_PATH, SAVE_FOLDER));
		var info = new DirectoryInfo(RESOURCE_PATH);
		var fileInfo = info.GetFiles();
		var names = new HashSet<string>();

		var filesList = new List<FileInfo>(fileInfo);
		filesList.Sort((f1,f2) => f2.CreationTime.CompareTo(f1.CreationTime)); // Sort descending

		foreach (FileInfo file in filesList) {

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

	private static void CreateSaveFolder() {
		Directory.CreateDirectory(RESOURCE_PATH);
	}

	/// <summary>
	/// Deletes the save file with the specified name. This can not be undone!
	/// </summary>
	/// <param name="filename">The filename of the evolution save file to be deleted.</param>
	public static void DeleteSaveFile(string filename) {

		var path = RESOURCE_PATH; //Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		File.Delete(path);
	}

	/// <summary>
	/// Saves a test .txt file to the evolution saves directory. Used for debugging an issue
	/// with saving files on Windows.
	/// </summary>
	private static void TestSave() {

		var filename = "Test.txt";
		var content = "Hello World";

		var savepath = Path.Combine(RESOURCE_PATH, filename);

		CreateSaveFolder();
		File.WriteAllText(savepath, content);
	}
}
