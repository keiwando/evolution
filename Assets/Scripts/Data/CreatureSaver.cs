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
public class CreatureSaver {

	public static readonly char[] INVALID_NAME_CHARACTERS = new char[]{ '\\', '/', '.' };
	private static readonly Regex INVALID_NAME_REGEX = 
					new Regex(string.Format("[{0}]", new string(INVALID_NAME_CHARACTERS)));

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "CreatureSaves";
	
	/// <summary>
	/// The separator to use in the save file between the different body component types. 
	/// </summary>
	private static  string COMPONENT_SEPARATOR = "--%%--\n"; // + System.Environment.NewLine;

	/// <summary>
	/// Used for splitting the text file by the body component types.
	/// </summary>
	private static string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };
	
	private static readonly Regex EXTENSION_PATTERN = new Regex(".creat");

	private static readonly string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	static CreatureSaver() {
		MigrateToFiles();
		CopyDefaultCreatures();
		ResetCurrentCreature();
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
	/// Saves the joints, bones and muscles of a creature with a given name to a file (/Playerprefs)
	/// The name cannot contain a dot (.)
	/// </summary>
	/// <exception cref="IllegalFilenameException"></exception>
	public static void WriteSaveFile(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		if ( INVALID_NAME_REGEX.IsMatch(name) || string.IsNullOrEmpty(name) ) 
			throw new IllegalFilenameException();

		var content = CreateSaveInfoFromCreature(joints, bones, muscles);
		name = EXTENSION_PATTERN.Replace(name, "");
		SaveCreatureDesign(name, content);
	}

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

		CreateSaveFolder();

		var info = new DirectoryInfo(RESOURCE_PATH);
		var fileInfo = info.GetFiles();

		var creatureNames = fileInfo.Where(f => f.Name.EndsWith(".creat"))
		.Select(f => EXTENSION_PATTERN.Replace(f.Name, "")).ToList();

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
	/// Loads a creature design with a specified name.
	/// </summary>
	public static void LoadCreature(string name, CreatureBuilder builder) {

		if (IsWebGL()) {
			LoadDefaultCreature(name, builder);
			return;
		}

		var contents = LoadSaveData(name);
		if (string.IsNullOrEmpty(contents)) return;

		LoadCreatureFromContents(contents, builder);
	}

	/// <summary>
	/// Loads a creature from the contents of a save file.
	/// </summary>
	public static void LoadCreatureFromContents(string contents, CreatureBuilder builder) {

		var components = contents.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		var jointStrings = components[0].Split('\n');
		var boneStrings = components[1].Split('\n');
		var muscleStrings = components[2].Split('\n');

		var joints = new List<Joint>();
		var bones = new List<Bone>();
		var muscles = new List<Muscle>();

		// create all the joints
		foreach (var data in jointStrings) {
			if (data.Length > 0) {
				joints.Add(Joint.CreateFromString(data));
			}
		}
		// create all the bones
		foreach (var data in boneStrings) {
			if (data.Length > 0) {
				bones.Add(Bone.CreateFromString(data, joints));
			}
		}
		// create all the muscles
		foreach (var data in muscleStrings) {
			if (data.Length > 1) {
				muscles.Add(Muscle.CreateFromString(data, bones));
			}
		}

		builder.SetBodyComponents(joints, bones, muscles);
	}

	/// <summary>
	/// Encodes the specified creature components into the contents of
	/// a creature design save file.
	/// </summary>
	public static string CreateSaveInfoFromCreature(List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		var content = new StringBuilder();
		// add joint data
		foreach (var joint in joints) {
			content.Append(joint.GetSaveString());
			content.Append('\n');
		}
		content.Append(COMPONENT_SEPARATOR);
		// add bone data
		foreach (var bone in bones) {
			content.Append(bone.GetSaveString());
			content.Append('\n');
		}
		content.Append(COMPONENT_SEPARATOR);
		// add muscle data
		foreach (var muscle in muscles) {
			content.Append(muscle.GetSaveString());
			content.Append('\n');
		}

		return content.ToString();
	}

	public static void LoadCurrentCreature(CreatureBuilder builder) {

		LoadCreatureFromContents(GetCurrentCreatureData(), builder);
	}

	public static void SaveCurrentCreature(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		var content = CreateSaveInfoFromCreature(joints, bones, muscles);

		SaveCurrentCreatureName(name);
		SaveCurrentCreatureDesign(content);
		return;
	}

	public static void SaveCurrentCreature(string name, string designData) {
		SaveCurrentCreatureName(name);
		SaveCurrentCreatureDesign(designData);
	}

	public static void SaveCurrentCreatureName(string name) {
		Settings.CurrentCreatureName = name;
	}

	public static string GetCurrentCreatureName() {
		return Settings.CurrentCreatureName;
	}

	public static void SaveCurrentCreatureDesign(string creatureData) {
		Settings.CurrentCreatureDesign = creatureData;
	}

	public static string GetCurrentCreatureData() {
		return Settings.CurrentCreatureDesign;
	}

	public static void ResetCurrentCreature() {
		SaveCurrentCreature("Unnamed", "");
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
		return Application.platform == RuntimePlatform.WebGLPlayer;
	}

	private static string LoadSaveData(string name) {
		
		var path = PathToCreatureDesign(name);
		if (File.Exists(path)) {
			return File.ReadAllText(path);
		} else {
			return "";
		}
	}

	private static void LoadDefaultCreature(string name, CreatureBuilder builder) {

		if (!DefaultCreatures.defaultCreatures.ContainsKey(name)) {
			Debug.Log("Creature not found!");
			return;
		}

		var contents = DefaultCreatures.defaultCreatures[name];
		LoadCreatureFromContents(contents, builder);
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
				SaveCreatureDesign(creatureName, saveData);
			}
		}

		Settings.DidMigrateCreatureSaves = true;
	}

	private static List<string> GetCreatureNamesFromPlayerPrefs() {
		return new List<string>(Settings.CreatureNames.Split('\n'));
	}
}
