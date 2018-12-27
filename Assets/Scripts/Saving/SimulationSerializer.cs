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
/// The Evolution Save files have the following format (VERSION 2):
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

	private static readonly Regex EXTENSION_PATTERN = new Regex(".evol");

	private static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	static SimulationSerializer() {
		MigrateSimulationSaves();
		CopyDefaultSimulations();
	}

	/// <summary>
	/// Saves the encoded simulation to a file with the specified name.
	/// </summary>
	/// <param name="name">The filename without an extension.</param>
	/// <param name="encodedData"></param>
	public static void SaveSimulationFile(string name, string encodedData, bool overwrite = false) { 

		name = EXTENSION_PATTERN.Replace(name, "");

		if (!overwrite) {
			name = GetAvailableSimulationName(name);
		}
		var path = PathToSimulationSave(name);

		CreateSaveFolder();
		File.WriteAllText(path, encodedData);
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
	/// <returns>The name of the saved file.</returns>
	public static string WriteSaveFile(string creatureName, 
									   EvolutionSettings settings, 
									   NeuralNetworkSettings networkSettings, 
									   int generationNumber, 
									   string creatureSaveData, 
									   List<ChromosomeStats> bestChromosomes, 
									   List<string> currentChromosomes) {

		if (string.IsNullOrEmpty(creatureName)) 
			throw new IllegalFilenameException();

		var splitOptions = new SplitOptions();

		var date = System.DateTime.Now.ToString("yyyy-MM-dd");

		var name = string.Format("{0} - {1} - {2} - Gen({3})", creatureName, settings.task, date, generationNumber);
		name = GetAvailableSimulationName(name);

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

		SaveSimulationFile(name, stringBuilder.ToString());

		return name;
	}

	/// <summary>
	/// Returns a simulation save filename that is still available based on the
	/// specified suggested name. (Both without extensions)
	/// </summary>
	private static string GetAvailableSimulationName(string suggestedName) {
		
		var existingNames = GetEvolutionSaveFilenames();
		int counter = 2;
		var finalName = suggestedName;
		while (existingNames.Contains(finalName)) {
			finalName = string.Format("{0} ({1})", suggestedName, counter);
			counter++;
		}
		return finalName;
	}

	/// <summary>
	/// Loads a previously saved simulation from an existing file and continues the
	/// evolution process.
	/// </summary>
	/// <param name="name">The name of the saved simulation without the file extension.</param>
	public static void LoadSimulationFromSaveFile(string name, CreatureBuilder creatureBuilder, Evolution evolution) {

		var path = PathToSimulationSave(name);
		var contents = File.ReadAllText(path);

		var lineEndings = contents.Contains("\r\n") ? "\r\n" : "\n";
		var splitOptions = new SplitOptions(lineEndings);
			
		var components = contents.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		// determine the version of the save file.
		// if the first line doesn't start with a v the version is 1

		if (components[0].ToUpper()[0] != 'V') {
			// V1
			SimulationLoaderV1.LoadSimulationFromSaveFile(name, contents, splitOptions, creatureBuilder, evolution);
			return;
		}

		var version = int.Parse(components[0].Split(' ')[1]);

		switch (version) {
			case 2: SimulationLoaderV2.LoadSimulationFromSaveFile(name, contents, splitOptions, creatureBuilder, evolution); break;
			default: throw new System.Exception("Unknown Save file format!");
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
		
		var files = fileInfo.Where(f => f.Name.EndsWith(".evol")).ToList();

		files.Sort((f1,f2) => f2.LastAccessTime.CompareTo(f1.LastAccessTime)); // Sort descending

		return files.Select(f => EXTENSION_PATTERN.Replace(f.Name, "")).ToList();
	}

	/// <summary>
	/// Renames the creature design with the specified name (Without extension).
	/// Existing files are overwritten.
	/// </summary>
	public static void RenameSimulationSave(string oldName, string newName) {
		var oldPath = PathToSimulationSave(oldName);
		var newPath = PathToSimulationSave(newName);

		if (File.Exists(oldPath))
			File.Move(oldPath, newPath);
	}

	/// <summary>
	/// Returns true if a simulation save with the specified name (without extension) 
	/// already exists.
	/// </summary>
	/// <param name="name"></param>
	public static bool SimulationSaveExists(string name) {
		return GetEvolutionSaveFilenames().Contains(name);
	}

	/// <summary>
	/// Deletes the save file with the specified name. This can not be undone!
	/// </summary>
	/// <param name="filename">The name of the evolution save file to be deleted. 
	/// (Without an extension)</param>
	public static void DeleteSaveFile(string name) {

		var path = PathToSimulationSave(name);
		File.Delete(path);
	}

	/// <summary>
	/// Copies the default simulation files from the resources folder into the savefile directory
	/// </summary>
	private static void CopyDefaultSimulations() {

		CreateSaveFolder();

		var names = new [] {
			"FROGGER - RUNNING - Default - Gen(70).evol"	
		};

		foreach (var name in names) {

			var savePath = GetSavePathForFile(name);

			if (!System.IO.File.Exists(savePath)) {
				var loadPath = Path.Combine("DefaultSaves", name);
				var resFile = Resources.Load(loadPath) as TextAsset;

				File.WriteAllText(savePath, resFile.text);
			}
		}
	}

	/// <summary>
	/// Returns the path to a simulation save file with the specified name 
	/// (without an extension).
	/// </summary>
	public static string PathToSimulationSave(string name) {
		return Path.Combine(RESOURCE_PATH, string.Format("{0}.evol", name));
	}

	/// <summary>
	/// Returns the path to a simulation save file with the specified filename
	/// (including the extension).
	/// </summary>
	public static string GetSavePathForFile(string filename) { 
		return Path.Combine(RESOURCE_PATH, filename);
	}

	/// <summary>
	/// Creates the save location for the creature saves if it doesn't exist already.
	/// </summary>
	private static void CreateSaveFolder() {
		Directory.CreateDirectory(RESOURCE_PATH);
	}

	/// <summary>
	/// Updates the extension for all existing save files from .txt to .evol
	/// </summary>
	private static void MigrateSimulationSaves() {
		if (Settings.DidMigrateSimulationSaves) return;
		Debug.Log("Beginning simulation save migration.");

		var filenames = new DirectoryInfo(RESOURCE_PATH).GetFiles().Select(f => f.Name);
		var txtReplace = new Regex(".txt");
		foreach (var filename in filenames) {
			var newName = txtReplace.Replace(filename, ".evol");
			var oldPath = GetSavePathForFile(filename);
			var newPath = GetSavePathForFile(newName);

			if (File.Exists(oldPath))
				File.Move(oldPath, newPath);
		}

		Settings.DidMigrateSimulationSaves = true;
	}
}
