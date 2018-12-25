using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The SimulationSerializer provides function for saving and loading the state of a simulation in / from a file.
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
public class SimulationSerializer {

	public class SplitOptions {

		private string COMPONENT_SEPARATOR_BASE = "--?%%%?--";
		public string COMPONENT_SEPARATOR {
			get { return COMPONENT_SEPARATOR_BASE + NEWLINE; }
		}

		public string[] SPLIT_ARRAY;
		public string[] NEWLINE_SPLIT;
		public string NEWLINE {
			get { return newline; }
		} 
		private string newline = Environment.NewLine;

		public SplitOptions(){
			
			SPLIT_ARRAY = new string[] { COMPONENT_SEPARATOR };
			NEWLINE_SPLIT = new string[] { NEWLINE };
		}

		public SplitOptions(string newline) {

			this.newline = newline;
			this.SPLIT_ARRAY = new string[] { COMPONENT_SEPARATOR };
			this.NEWLINE_SPLIT = new string[] { NEWLINE };
		}
	}

	public static readonly char[] INVALID_NAME_CHARACTERS = new char[]{ '\\', '/', '.' };

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

	private static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	static SimulationSerializer() {
		// TODO: Migrate file extensions
	}

	/// <summary>
	/// Saves the given information about an evolution simulation of a creature in a file, so that
	/// it can be loaded and continued at the same generation again.
	/// </summary>
	/// <remarks>
	/// The file is always written in the latest save version format.
	/// </remarks>
	/// <exception cref="IllegalFilenameException">Thrown if the filename contains 
	/// illegal characters</exception>
	/// <returns>The filename of the saved file.</returns>
	public static string WriteSaveFile(string creatureName, EvolutionSettings settings, NeuralNetworkSettings networkSettings, int generationNumber, string creatureSaveData, List<ChromosomeStats> bestChromosomes, List<string> currentChromosomes) {

		var splitOptions = new SplitOptions();

		var date = System.DateTime.Now.ToString("yyyy-MM-dd");

		var filename = string.Format("{0} - {1} - {2} - Gen({3}).txt", creatureName, settings.task, date, generationNumber);    // MAYBE IMPORTANT FOR THE FUTURE: Changed from Gen:i to Gen(i)

		var stringBuilder = new StringBuilder();

		// Add the version number
		stringBuilder.AppendLine(string.Format("v {0}", version.ToString()));
		stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

		// Add the encoded evolution settings
		stringBuilder.AppendLine(settings.Encode());
		stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

		// Add the encoded neural network settings
		stringBuilder.AppendLine(networkSettings.Encode());
		stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

		// Add the creature save data 
		stringBuilder.AppendLine(creatureSaveData);
		stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

		// Add the list of best chromosomes
		foreach (var chromosome in bestChromosomes) {
			stringBuilder.AppendLine(chromosome.ToString());
		}
		stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

		// Add the list of current chromosomes
		foreach (var chromosome in currentChromosomes) {
			stringBuilder.AppendLine(chromosome);
		}
		stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

		SaveSimulationFile(filename, stringBuilder.ToString());

		return filename;
	}

	public static void SaveSimulationFile(string filename, string contents) { 

		var path = GetAvailableFilePath(filename);

		CreateSaveFolder();
		File.WriteAllText(path, contents);
	}

	private static string GetAvailableFilePath(string filename) {

		var path = RESOURCE_PATH;
		path = Path.Combine(path, filename);
		var extension = Path.GetExtension(filename);

		int counter = 2;
		while (System.IO.File.Exists(path)) {

			var pattern = String.Format(@"( \d+)?{0}", extension);

			filename = Regex.Replace(filename, pattern, string.Format(" {0}{1}", counter, extension));
			path = Path.Combine(RESOURCE_PATH, filename);
			counter++;
		}

		return path;
	}

	public static void LoadSimulationFromSaveFile(string filename, CreatureBuilder creatureBuilder, Evolution evolution) {

		if (!filename.EndsWith(".txt")) {
			filename += ".txt";
		}

		// check if the file exists
		var path = RESOURCE_PATH;
		path = Path.Combine(path, filename);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();

		var splitOptions = new SplitOptions();

		// Determine the line endings
		if (contents.Contains("\r\n")) {
			// Windows style endings
			splitOptions = new SplitOptions("\r\n");
		} else {
			splitOptions = new SplitOptions("\n");
		}
			
		var components = contents.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		// determine the version of the save file.
		// if the first line doesn't start with a v the version is 1

		if (components[0].ToUpper()[0] == 'V') {

			var version = int.Parse(components[0].Split(' ')[1]);

			if (version == 2) {
				SimulationLoaderV2.LoadSimulationFromSaveFile(filename, contents, splitOptions, creatureBuilder, evolution);
			} else {
				throw new System.Exception("Unknown Save file format!");
			}
			
		} else {
			// V1
			SimulationLoaderV1.LoadSimulationFromSaveFile(filename, contents, splitOptions, creatureBuilder, evolution);
		}
	}
	

	/// <summary>
	/// Returns a list of filenames containing simulation save data. 
	/// </summary>
	/// <returns>The evolution save filenames.</returns>
	public static List<string> GetEvolutionSaveFilenames() {

		CreateSaveFolder();

		var info = new DirectoryInfo(RESOURCE_PATH);
		var fileInfo = info.GetFiles();

		var filesList = new List<FileInfo>(fileInfo);
		filesList.Sort((f1,f2) => f2.LastAccessTime.CompareTo(f1.LastAccessTime)); // Sort descending

		return filesList.Select(x => x.Name).ToList();
	}

	private static void CreateSaveFolder() {
		Directory.CreateDirectory(RESOURCE_PATH);
		CopyDefaultSimulations();
	}

	/// <summary>
	/// Copies the default simulation files from the resources folder into the savefile directory
	/// </summary>
	private static void CopyDefaultSimulations() {

		var names = new [] {
			"FROGGER - RUNNING - Default - Gen(70)"	
		};

		foreach (var name in names) {

			var savePath = Path.Combine(RESOURCE_PATH, name + ".txt");

			if (!System.IO.File.Exists(savePath)) {
				var loadPath = Path.Combine("DefaultSaves", name);
				var resFile = Resources.Load(loadPath) as TextAsset;

				File.WriteAllText(savePath, resFile.text);
			}
		}
	}

	/// <summary>
	/// Deletes the save file with the specified name. This can not be undone!
	/// </summary>
	/// <param name="filename">The filename of the evolution save file to be deleted.</param>
	public static void DeleteSaveFile(string filename) {

		var path = RESOURCE_PATH; //Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		File.Delete(path);
		Debug.Log(filename);
	}

	public static string GetSavePathForFile(string filename) { 
		return Path.Combine(RESOURCE_PATH, filename);
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
