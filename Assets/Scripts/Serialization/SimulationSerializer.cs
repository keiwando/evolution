using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Keiwando.Evolution;
using Keiwando.JSON;

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

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "EvolutionSaves";

	public const string FILE_EXTENSION = ".evol";

	public static readonly Regex EXTENSION_PATTERN = new Regex(FILE_EXTENSION);

	private static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	static SimulationSerializer() {

		#if UNITY_WEBGL
		return;
		#endif

		MigrateSimulationSaves();
		CopyDefaultSimulations();
	}

	/// <summary>
	/// Saves the specified simulation data to a file
	/// </summary>
	/// <returns>The filename of the save file without the extensions</returns>
	public static string SaveSimulation(SimulationData data) {

		string contents = data.Encode().ToString(Formatting.None);
		string creatureName = data.CreatureDesign.Name;
		string dateString = System.DateTime.Now.ToString("MMM dd, yyyy");
		string taskString = EvolutionTaskUtil.StringRepresentation(data.Settings.Task);
		int generation = data.BestCreatures.Count + 1;
		string filename = string.Format("{0} - {1} - {2} - Gen {3}", creatureName, taskString, dateString, generation);

		// Save without overwriting existing saves
		return SaveSimulationFile(filename, contents, false);
	}

	/// <summary>
	/// Saves the encoded simulation to a file with the specified name.
	/// </summary>
	/// <param name="name">The filename without an extension.</param>
	/// <param name="encodedData"></param>
	/// <returns>The filename of the save file without the extension</returns>
	public static string SaveSimulationFile(string name, string encodedData, bool overwrite = false) { 

		name = EXTENSION_PATTERN.Replace(name, "");

		if (!overwrite) {
			name = GetAvailableSimulationName(name);
		}
		var path = PathToSimulationSave(name);

		CreateSaveFolder();
		File.WriteAllText(path, encodedData);

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
	/// Loads a previously saved simulation data with the specified name
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static SimulationData LoadSimulationData(string name) {

		var contents = LoadSaveData(name);

		if (string.IsNullOrEmpty(contents))
			throw new Exception("Invalid Simulation Data file contents - empty!");

		return ParseSimulationData(contents, name);
	}

	/// <summary>
	/// Returns the SimulationData of a previously saved simulation.
	/// </summary>
	/// <param name="name">The name of the saved simulation without the file extension.</param>
	public static SimulationData ParseSimulationData(string encoded, string filename = "Unnamed.evol") {

		// Distinguish between JSON and legacy custom encodings
		if (encoded.StartsWith("{")) {
			return SimulationData.Decode(encoded);
		}

		return LegacySimulationLoader.ParseSimulationData(filename, encoded);
	}

	/// <summary>
	/// Returns a list of filenames containing simulation save data. 
	/// </summary>
	/// <returns>The evolution save filenames.</returns>
	public static List<string> GetEvolutionSaveFilenames() {

		return FileUtil.GetFilenamesInDirectory(RESOURCE_PATH, FILE_EXTENSION);
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
		return GetEvolutionSaveFilenames().Contains(name + FILE_EXTENSION);
	}

	/// <summary>
	/// Deletes the save file with the specified name. This can not be undone!
	/// </summary>
	/// <param name="filename">The name of the evolution save file to be deleted. 
	/// (Without an extension)</param>
	public static void DeleteSaveFile(string name) {

		var path = PathToSimulationSave(name);
		if (File.Exists(path))
			File.Delete(path);
	}

	private static string LoadSaveData(string name) {
		
		var path = PathToSimulationSave(name);
		if (File.Exists(path)) {
			return File.ReadAllText(path);
		} else {
			return "";
		}
	}

	/// <summary>
	/// Copies the default simulation files from the resources folder into the savefile directory
	/// </summary>
	private static void CopyDefaultSimulations() {

		CreateSaveFolder();

		var names = new [] {
			"FROGGER - RUNNING - Default - Gen(70)"	
		};

		foreach (var name in names) {

			var savePath = GetSavePathForFile(name + ".evol");

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
		return Path.Combine(RESOURCE_PATH, string.Format("{0}{1}", name, FILE_EXTENSION));
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

	#region Migration

	/// <summary>
	/// Updates the extension for all existing save files from .txt to .evol
	/// </summary>
	private static void MigrateSimulationSaves() {
		if (Settings.DidMigrateSimulationSaves) return;
		Debug.Log("Beginning simulation save migration.");

		var filenames = FileUtil.GetFilenamesInDirectory(RESOURCE_PATH, ".txt");
		var txtReplace = new Regex(".txt");
		foreach (var filename in filenames) {
			var newName = txtReplace.Replace(filename, ".evol");
			var oldPath = GetSavePathForFile(filename);
			var newPath = GetSavePathForFile(newName);

			if (File.Exists(oldPath) && !File.Exists(newPath))
				File.Move(oldPath, newPath);
		}

		Settings.DidMigrateSimulationSaves = true;
	}

	#endregion
}
