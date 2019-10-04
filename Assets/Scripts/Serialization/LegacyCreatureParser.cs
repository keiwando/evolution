using UnityEngine;
using System.Collections.Generic;

public static class LegacyCreatureParser {

    /// <summary>
	/// The separator to use in the save file between the different body component types. 
	/// </summary>
	private static string COMPONENT_SEPARATOR = "--%%--\n"; // + System.Environment.NewLine;

    /// <summary>
	/// Used for splitting the text file by the body component types.
	/// </summary>
	private static string[] SPLIT_ARRAY = new string[]{ COMPONENT_SEPARATOR };

    public static CreatureDesign ParseCreatureDesign(string name, string contents) {

		var components = contents.Split(SPLIT_ARRAY, System.StringSplitOptions.None);

		var jointStrings = components[0].Split('\n');
		var boneStrings = components[1].Split('\n');
		var muscleStrings = components[2].Split('\n');

		var joints = new List<JointData>();
		var bones = new List<BoneData>();
		var muscles = new List<MuscleData>();

		// create all the joints
		foreach (var data in jointStrings) {
			if (data.Length > 0) {
				joints.Add(ParseJointData(data));
			}
		}
		// create all the bones
		foreach (var data in boneStrings) {
			if (data.Length > 0) {
				bones.Add(ParseBoneData(data));
			}
		}
		// create all the muscles
		foreach (var data in muscleStrings) {
			if (data.Length > 1) {
				muscles.Add(ParseMuscleData(data));
			}
		}

		return new CreatureDesign(name, joints, bones, muscles);
    }

	private static JointData ParseJointData(string encoded) {

		var parts = encoded.Split('%');
		// Format: ID - pos.x - pos.y - pos.z
		var x = ParseFloat(parts[1]);
		var y = ParseFloat(parts[2]);
		var z = ParseFloat(parts[3]);

		var id = int.Parse(parts[0]);

		return new JointData(id, new Vector3(x, y, z), 1f);
	}

	private static BoneData ParseBoneData(string encoded) {

		// Format: ID - startJointID - endJointID
		var parts = encoded.Split('%');
		var boneID = int.Parse(parts[0]);
		var jointID1 = int.Parse(parts[1]);
		var jointID2 = int.Parse(parts[2]);

		return new BoneData(boneID, jointID1, jointID2, 1f, true);		
	}

	private static MuscleData ParseMuscleData(string encoded) {

		// Format: ID - startBoneID - endBoneID
		var parts = encoded.Split('%');
		var muscleID = int.Parse(parts[0]);
		var startID = int.Parse(parts[1]);
		var endID = int.Parse(parts[2]);

		return new MuscleData(muscleID, startID, endID, Muscle.Defaults.MaxForce, true);
	}

	private static float ParseFloat(string encoded) {

		var culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
		var style = System.Globalization.NumberStyles.Float;

		var result = 0f;
		try {
			result = float.Parse(encoded, style, culture);
		} catch {
			float.TryParse(encoded, out result);
		}
		return result;
	}
}