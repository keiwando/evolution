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
	/// The separator to use in the save file between the different body component types. 
	/// </summary>
	private const string COMPONENT_SEPARATOR = "--%%--\n";
	/// <summary>
	/// Used for splitting the text file by the body component types.
	/// </summary>
	private static string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };

	// Use this for initialization
	public CreatureSaver () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Loads the names of all the creature save-files into the creatureNames array.
	/// </summary>
	public static List<string> GetCreatureNames() {
		
		//Debug.Log("Loading Creature Names...");
		
		var info = new DirectoryInfo(Path.Combine(Application.dataPath, SAVE_FOLDER));
		var fileInfo = info.GetFiles();
		var names = new HashSet<string>();

		foreach (FileInfo file in fileInfo) {
			names.Add(file.Name.Split('.')[0]);
		} 
			
		var creatureNames = new List<string>();
		foreach (string name in names) {
			creatureNames.Add(name);
		}

		creatureNames.Sort();

		return creatureNames;
	}

	/// <summary>
	/// Saves the joints, bones and muscles of a creature with a given name to a file.
	/// The name cannot contain a dot (.)
	/// Throws: IllegalFilenameException
	/// </summary>
	public static void WriteSaveFile(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

		if ( name.Contains(".") || name == "" ) throw new IllegalFilenameException();

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

		var filename = name + ".txt";
		/*if (!filename.EndsWith(".txt")) {
			filename += ".txt";
		}*/
			
		var path = Path.Combine(Application.dataPath, SAVE_FOLDER);
		path = Path.Combine(path, filename);

		File.WriteAllText(path, content);

		//creatureNames.Add(name);
	}

	public static void LoadCreature(string name, CreatureBuilder builder) {

		if (!name.EndsWith(".txt")) {
			name += ".txt";
		}

		var path = Path.Combine(Application.dataPath, SAVE_FOLDER);
		path = Path.Combine(path, name);

		var reader = new StreamReader(path);
		var contents = reader.ReadToEnd();
		reader.Close();

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
}
