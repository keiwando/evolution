using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSaver : MonoBehaviour {

	private const string SAVE_FOLDER = "CreatureSaves";
	private const string COMPONENT_SEPARATOR = "--%%--\n";
	private string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void WriteSaveFile(string name, List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {

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

		if (!name.EndsWith(".txt")) {
			name += ".txt";
		}
			
		var path = Path.Combine(Application.dataPath, SAVE_FOLDER);
		path = Path.Combine(path, name);

		File.WriteAllText(path, content);
	}

	public void LoadCreature(string name, CreatureBuilder builder) {

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
