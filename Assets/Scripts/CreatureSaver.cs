using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllegalFilenameException: IOException {

	public override string Message {
		get {
			return "The filename is not valid.\n" + base.Message;
		}
	}
}

/// <summary>
/// Handles saving and loading of Creatures
/// </summary>
public class CreatureSaver {

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "CreatureSaves";
	/// <summary>
	/// The name of the folder that holds the save file for the currently evolving creature.
	/// </summary>
	private const string CURRENT_SAVE_FOLDER = "CurrentCreatureSave";
	/// <summary>
	/// The separator to use in the save file between the different body component types. 
	/// </summary>
	private const string COMPONENT_SEPARATOR = "--%%--\n";
	/// <summary>
	/// Used for splitting the text file by the body component types.
	/// </summary>
	private static string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };

	private static string CURRENT_SAVE_KEY = "_CurrentCreatureSave";
	private static string CURRENT_CREATURE_NAME_KEY = "_CurrentCreatureName";
	private static string CREATURE_NAMES_KEY = "_CreatureNames";


	private static string RESOURCE_PATH = Path.Combine(Application.dataPath, "Resources");

	// Use this for initialization
	public CreatureSaver () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Loads the default creatures into the PlayerPrefs
	public static void SetupPlayerPrefs() {

		//PlayerPrefs.DeleteAll();
		// Debug
		//PrintCreatureData("FROGGER");

		var added = new List<string>();

		foreach (var name in DefaultCreatures.defaultCreatures.Keys) {
			
			if(!PlayerPrefs.HasKey(name)) {
				
				PlayerPrefs.SetString(name, DefaultCreatures.defaultCreatures[name]);
				added.Add(name);
			}
		}

		AddCreatureNamesToPP(added);
	}

	/// <summary>
	/// Loads the names of all the creature save-files (/Playerprefs) into the creatureNames array.
	/// </summary>
	public static List<string> GetCreatureNames() {
		
		//Debug.Log("Loading Creature Names...");

		if (IsWebGL()) {
			return GetCreatureNamesWebGL();
		}

		var names = GetCreatureNamesFromPlayerPrefs();
		names.Sort();
		return names;


		// OLD CODE! UNREACHABLE!
		/*
		var info = new DirectoryInfo(Path.Combine(RESOURCE_PATH, SAVE_FOLDER));
		var fileInfo = info.GetFiles();
		var names = new HashSet<string>();

		foreach (FileInfo file in fileInfo) {

			if (file.Name.Contains(".txt")) {

				names.Add(file.Name.Split('.')[0]);
			}
		} 
			
		var creatureNames = new List<string>();
		foreach (string name in names) {
			creatureNames.Add(name);
		}

		creatureNames.Sort();

		return creatureNames;*/
	}

	private static List<string> GetCreatureNamesWebGL() {

		var names = new List<string>();
		foreach (var name in DefaultCreatures.defaultCreatures.Keys) {
			names.Add(name);
		} 

		names.Sort();
		return names;
	}

	private static List<string> GetCreatureNamesFromPlayerPrefs() {

		return new List<string>(PlayerPrefs.GetString(CREATURE_NAMES_KEY,"").Split('\n'));
	}

	private static void AddCreatureNamesToPP(List<string> names) {

		if (names.Count == 0) return;

		var namesInPP = GetCreatureNamesFromPlayerPrefs();
		namesInPP.AddRange(names);
		namesInPP.RemoveAll(t => t == "");

		var namesString = string.Join("\n", namesInPP.ToArray());


		PlayerPrefs.SetString(CREATURE_NAMES_KEY, namesString);
	}

	/// <summary>
	/// Saves the joints, bones and muscles of a creature with a given name to a file (/Playerprefs)
	/// The name cannot contain a dot (.)
	/// Throws: IllegalFilenameException
	/// </summary>
	public static void WriteSaveFile(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		if ( name.Contains(".") || name.Contains("_") || name == "" ) throw new IllegalFilenameException();

		var content = CreateSaveInfoFromCreature(joints, bones, muscles);

		var filename = name.ToUpper();// + ".txt";

		PlayerPrefs.SetString(filename, content);

		var names = GetCreatureNamesFromPlayerPrefs();
		if (!names.Contains(filename)) {

			names.Add(filename);
			var nameString = string.Join("\n",names.ToArray());
			PlayerPrefs.SetString(CREATURE_NAMES_KEY, nameString);
		}
		/*if (!filename.EndsWith(".txt")) {
			filename += ".txt";
		}*/
		/*	
		var path = Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		File.WriteAllText(path, content);*/
	}

	/// <summary>
	/// Loads a creature with a given name from one of the user saved or default creature files.
	/// </summary>
	public static void LoadCreature(string name, CreatureBuilder builder) {

		if (IsWebGL()) {
			LoadCreatureWebGL(name, builder);
			return;
		}

		/*if (!name.EndsWith(".txt")) {
			name += ".txt";
		}*/

		var contents = PlayerPrefs.GetString(name);

		/*
		var path = Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, name);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();*/

		LoadCreatureFromContents(contents, builder);
	}

	private static void LoadCreatureWebGL(string name, CreatureBuilder builder) {

		if (!DefaultCreatures.defaultCreatures.ContainsKey(name)) {
			Debug.Log("Creature not found!");
			return;
		}

		var contents = DefaultCreatures.defaultCreatures[name];

		LoadCreatureFromContents(contents, builder);
	}

	/// <summary>
	/// Loads a creature from the contents of a save file.
	/// </summary>
	public static void LoadCreatureFromContents(string contents, CreatureBuilder builder) {

		BodyComponent.ResetID();
		var components = contents.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		var jointStrings = components[0].Split('\n');
		var boneStrings = components[1].Split('\n');
		var muscleStrings = components[2].Split('\n');

		var joints = new List<Joint>();
		var bones = new List<Bone>();
		var muscles = new List<Muscle>();

		// create all the joints
		foreach (var data in jointStrings) {
			if (data != "") {
				joints.Add(Joint.CreateFromString(data));
			}
		}
		// create all the bones
		foreach (var data in boneStrings) {
			if (data != "") {
				bones.Add(Bone.CreateFromString(data, joints));
			}
		}
		// create all the muscles
		foreach (var data in muscleStrings) {
			if (data != "") {
				muscles.Add(Muscle.CreateFromString(data, bones));
			}
		}

		builder.SetBodyComponents(joints, bones, muscles);
	}
		
	public static void LoadCurrentCreature(CreatureBuilder builder) {

		//if (IsWebGL()) {
		LoadCurrentCreaturePP(builder);
		return;
		//}
		/*
		var name = "CurrentCreature.txt";

		var path = Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, CURRENT_SAVE_FOLDER);
		path = Path.Combine(path, name);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();

		LoadCreatureFromContents(contents, builder);*/
	}

	// PP == PlayerPrefs
	private static void LoadCurrentCreaturePP(CreatureBuilder builder) {
		var contents = PlayerPrefs.GetString(CURRENT_SAVE_KEY, "");
		if (contents == "") return;

		LoadCreatureFromContents(contents, builder);
	}

	/// <summary>
	/// Returns the save data of the current creature.
	/// </summary>
	/// <returns>The current creature data.</returns>
	public static string GetCurrentCreatureData() {

		return PlayerPrefs.GetString(CURRENT_SAVE_KEY, "");
	}

	private static string CreateSaveInfoFromCreature(List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		var content = "";
		// add joint data
		foreach (var joint in joints) {
			content += joint.GetSaveString() + "\n";
		}
		content += COMPONENT_SEPARATOR;
		// add bone data
		foreach (var bone in bones) {
			content += bone.GetSaveString() + "\n";
		}
		content += COMPONENT_SEPARATOR;
		// add muscle data
		foreach (var muscle in muscles) {
			content += muscle.GetSaveString() + "\n";
		}

		return content;
	}

	public static void SaveCurrentCreature(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		var content = CreateSaveInfoFromCreature(joints, bones, muscles);

		//if (IsWebGL()) {
		SaveCurrentCreaturePP(content);

		SaveCurrentCreatureName(name);
		return;
		//}
		/*
		var filename = "CurrentCreature.txt";

		var path = Path.Combine(RESOURCE_PATH, SAVE_FOLDER);
		path = Path.Combine(path, CURRENT_SAVE_FOLDER);
		path = Path.Combine(path, filename);

		File.WriteAllText(path, content);*/
	}

	public static void SaveCurrentCreatureName(string name) {
		PlayerPrefs.SetString(CURRENT_CREATURE_NAME_KEY, name);
	}

	public static string GetCurrentCreatureName() {
		return PlayerPrefs.GetString(CURRENT_CREATURE_NAME_KEY, "Creature");
	}

	private static void SaveCurrentCreaturePP(string content) {
		PlayerPrefs.SetString(CURRENT_SAVE_KEY, content);
	}

	private static bool IsWebGL() {
		return Application.platform == RuntimePlatform.WebGLPlayer;
	} 

	/// <summary>
	/// Prints the creature data stored in the Playerprefs for the specified creature. Used for debugging
	/// and manually adding stored creatures to the default creatures that come with the app.
	/// </summary>
	/// <param name="name">Name.</param>
	private static void PrintCreatureData(string name) {
		var data = PlayerPrefs.GetString(name.ToUpper(), "");
		Debug.Log(name + " data: \n" + data);
	}
}
