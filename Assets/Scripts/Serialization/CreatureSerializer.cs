using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class IllegalFilenameException: IOException {

	public override string Message {
		get { return "The filename is not valid.\n" + base.Message; }
	}
}

/// <summary>
/// Handles saving and loading of Creatures
/// </summary>
public class CreatureSerializer {

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "CreatureSaves";

	public const string FILE_EXTENSION = ".creat";
	
	public static readonly Regex EXTENSION_PATTERN = new Regex(string.Format("{0}$", FILE_EXTENSION));

	private static readonly string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	static CreatureSerializer() {
		MigrateToFiles();
		CopyDefaultCreatures();
	}

	public static void SaveCreatureDesign(CreatureDesign design) {
		// TODO: Implement
	}

	/// <summary>
	/// Saves the given creature design data to a .creat file.
	/// </summary>
	/// <param name="creatureName">The name of the creature design. Will become the filename
	/// of the save file.</param>
	/// <param name="saveData">The design data to be stored.</param>
	/// <param name="overwrite">Whether an existing creature design with the same name should
	/// be overwritten or not. If not, then an available name is chosen for the new save.</param>
	public static void SaveCreatureDesign(string creatureName, string saveData, bool overwrite = false) { 

		creatureName = EXTENSION_PATTERN.Replace(creatureName, "");

		if (!overwrite) {
			creatureName = GetAvailableCreatureName(creatureName);
		}
		var path = PathToCreatureDesign(creatureName);

		CreateSaveFolder();
		File.WriteAllText(path, saveData);
	}

	/// <summary>
	/// Loads a creature design with a specified name.
	/// </summary>
	public static CreatureDesign LoadCreatureDesign(string name) {

		#if UNITY_WEBGL
		return GetDefaultCreatureDesign(name);
		#endif

		var contents = LoadSaveData(name);
		
		if (string.IsNullOrEmpty(contents)) 
			return new CreatureDesign();

		return ParseCreatureDesign(contents, name);
	}

	public static CreatureDesign ParseCreatureDesign(string encoded, string name = "") {

		if (string.IsNullOrEmpty(encoded)) 
			return new CreatureDesign();

		// Distinguish between JSON and legacy custom encodings
		if (encoded.StartsWith("{")) {
			return (CreatureDesign)JsonUtility.FromJson(encoded, typeof(CreatureDesign));
		}

		return LegacyCreatureParser.ParseCreatureDesign(name, encoded);
	}

	/// <summary>
	/// Saves the joints, bones and muscles of a creature with a given name to a file (/Playerprefs)
	/// The name cannot contain a dot (.)
	/// </summary>
	/// <exception cref="IllegalFilenameException"></exception>
	// public static void WriteSaveFile(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

	// 	if ( INVALID_NAME_REGEX.IsMatch(name) || string.IsNullOrEmpty(name) ) 
	// 		throw new IllegalFilenameException();

	// 	var content = CreateSaveInfoFromCreature(joints, bones, muscles);
	// 	name = EXTENSION_PATTERN.Replace(name, "");
	// 	SaveCreatureDesign(name, content);
	// }

	/// <summary>
	/// Returns the path to the save location for the creature design with the specified name.
	/// Does not guarantee an existing file.
	/// </summary>
	/// <param name="name">The creature design name (without a file extension)</param>
	public static string PathToCreatureDesign(string name) {
		return Path.Combine(RESOURCE_PATH, string.Format("{0}.creat", name));
	}

	/// <summary>
	/// Renames the creature design with the specified name. 
	/// Existing files are overwritten.
	/// </summary>
	public static void RenameCreatureDesign(string oldName, string newName) {
		var oldPath = PathToCreatureDesign(oldName);
		var newPath = PathToCreatureDesign(newName);

		if (File.Exists(oldPath))
			File.Move(oldPath, newPath);
	}

	/// <summary>
	/// Returns true if a creature design save with the specified name already exists.
	/// </summary>
	/// <param name="name">The name of the creature design.</param>
	public static bool CreatureExists(string name) {
		return GetCreatureNames().Contains(name);
	}

	/// <summary>
	/// Loads the names of all the creature save files into the 
	/// creatureNames array.
	/// </summary>
	public static List<string> GetCreatureNames() {
		
		if (IsWebGL()) return GetDefaultCreatureNames();

		// CreateSaveFolder();

		// var info = new DirectoryInfo(RESOURCE_PATH);
		// var fileInfo = info.GetFiles();

		// var creatureNames = fileInfo.Where(f => f.Name.EndsWith(".creat"))
		// .Select(f => EXTENSION_PATTERN.Replace(f.Name, "")).ToList();
		var creatureNames = FileUtil.GetFilenamesInDirectory(RESOURCE_PATH, FILE_EXTENSION)
			.Select(filename => EXTENSION_PATTERN.Replace(filename, "")).ToList();
		
		creatureNames.Sort();

		return creatureNames;
	}

	/// <summary>
	/// Deletes the saved creature design data for the specified name.
	/// </summary>
	public static void DeleteCreatureSave(string name) {

		var path = PathToCreatureDesign(name);
		if (File.Exists(path))
			File.Delete(path);
	}

	

	/// <summary>
	/// Returns a creature design name that is still available based on the 
	/// specified suggested name.
	/// </summary>
	private static string GetAvailableCreatureName(string suggestedName) {

		var existingNames = GetCreatureNames();
		int counter = 2;
		var finalName = suggestedName;
		while (existingNames.Contains(finalName)) {
			finalName = string.Format("{0} ({1})", suggestedName, counter);
			counter++;
		}
		return finalName;
	}

	private static bool IsWebGL() {
		#if UNITY_WEBGL
		return true;
		#else
		return false;
		#endif
		// return Application.platform == RuntimePlatform.WebGLPlayer;
	}

	private static string LoadSaveData(string name) {
		
		var path = PathToCreatureDesign(name);
		if (File.Exists(path)) {
			return File.ReadAllText(path);
		} else {
			return "";
		}
	}

	private static CreatureDesign GetDefaultCreatureDesign(string name) {

		if (!DefaultCreatures.defaultCreatures.ContainsKey(name)) {
			Debug.Log("Creature not found!");
			return new CreatureDesign();
		}

		var contents = DefaultCreatures.defaultCreatures[name];
		return ParseCreatureDesign(contents, name);
	}

	/// <summary>
	/// Creates the save location for the creature saves if it doesn't exist already.
	/// </summary>
	private static void CreateSaveFolder() {
		Directory.CreateDirectory(RESOURCE_PATH);
	}

	/// <summary>
	/// Writes the default creature designs to save files.
	/// </summary>
	private static void CopyDefaultCreatures() {
		if (IsWebGL()) return;

		foreach (var creature in DefaultCreatures.defaultCreatures) {
			if (!CreatureExists(creature.Key)) {
				SaveCreatureDesign(creature.Key, creature.Value);
			}
		}
	}

	/// <summary>
	/// Returns the default creature names since creatures cannot be saved
	/// in the WebGL version.
	/// </summary>
	/// <returns></returns>
	private static List<string> GetDefaultCreatureNames() {

		var names = DefaultCreatures.defaultCreatures.Keys.ToList();
		names.Sort();
		return names;
	}

	#region Migration

	/// <summary>
	/// Migrates all existing creature design saves from the PlayerPrefs (an awful
	/// way of storing them) to use actual files.
	/// </summary>
	/// <remarks>The creature data remains in the PlayerPrefs in case of issues to enable
	/// potential future recovery. The PlayerPrefs should not be used to store any
	/// new creature designs!</remarks>
	private static void MigrateToFiles() {

		if (Settings.DidMigrateCreatureSaves) return;
		if (IsWebGL()) return;
		Debug.Log("Beginning creature save data migration.");

		var creatureNames = GetCreatureNamesFromPlayerPrefs();
		foreach (var creatureName in creatureNames) {
			var saveData = PlayerPrefs.GetString(creatureName, "");
			if (!string.IsNullOrEmpty(saveData)) {
				SaveCreatureDesign(creatureName, saveData, false);
			}
		}

		Settings.DidMigrateCreatureSaves = true;
	}

	private static List<string> GetCreatureNamesFromPlayerPrefs() {
		return new List<string>(Settings.CreatureNames.Split('\n'));
	}

	#endregion
}
