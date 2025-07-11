using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Keiwando.JSON;
using Keiwando.Evolution;

public class IllegalFilenameException: IOException {

	public override string Message {
		get { return "The filename is not valid.\n" + base.Message; }
	}
}

/// <summary>
/// Handles saving and loading of Creatures
/// </summary>
public class CreatureSerializer {

	public static event Action MigrationDidBegin;
	public static event Action MigrationDidEnd;

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

	/// <summary>
	/// Saves the given creature design data to a .creat file.
	/// </summary>
	/// <param name="creatureName">The name of the creature design. Will become the filename
	/// of the save file.</param>
	/// <param name="saveData">The design data to be stored.</param>
	/// <param name="overwrite">Whether an existing creature design with the same name should
	/// be overwritten or not. If not, then an available name is chosen for the new save.</param>
	/// <returns>The name under which the design has been saved.</returns>
	public static void SaveCreatureDesign(CreatureDesign design, bool overwrite = false) {
		
		var creatureName = design.Name;
		creatureName = EXTENSION_PATTERN.Replace(creatureName, "");

		if (!overwrite) {
			creatureName = GetAvailableCreatureName(creatureName);
		}

		design.Name = creatureName;
		var encoded = design.Encode().ToString(Formatting.None);
		var path = PathToCreatureDesign(creatureName);

		CreateSaveFolder();
		File.WriteAllText(path, encoded);
	}

	private enum TaggedBlockType: short {
		Joints = 0,
		Bones = 1,
		Muscles = 2
	}
	private const short TaggedBlockType_MAX_VALUE = 2;
	private const ushort SERIALIZATION_VERSION = 1;

	public static void WriteCreatureDesign(CreatureDesign design, BinaryWriter writer) {

		// ### Header ###

		// Magic Bytes
    writer.Write((char)'E');
    writer.Write((char)'V');
    writer.Write((char)'O');
    writer.Write((char)'L');
    writer.Write((char)'D');
    writer.Write((char)'S');
    writer.Write((char)'G');
    writer.Write((char)'N');

		// Version
		writer.Write(SERIALIZATION_VERSION);

		long dataLengthOffset = writer.Seek(0, SeekOrigin.Current);
		writer.WriteDummyBlockLength();

		// ### Content ###

		// Name
		writer.WriteUTF8String(design.Name);

		// Tagged blocks defining different aspects of the creature design

		// Joints
		{
			long lengthOffset = writer.Seek(0, SeekOrigin.Current);
			writer.WriteDummyBlockLength();

			writer.Write((short)TaggedBlockType.Joints);
			writer.Write((uint)design.Joints.Count);
			foreach (JointData joint in design.Joints) {
				WriteJointData(joint, writer);
			}

			writer.WriteBlockLengthToOffset(lengthOffset);
		}

		// Bones
		{
			long lengthOffset = writer.Seek(0, SeekOrigin.Current);
			writer.WriteDummyBlockLength();

			writer.Write((short)TaggedBlockType.Bones);
			writer.Write((uint)design.Bones.Count);
			foreach (BoneData bone in design.Bones) {
				WriteBoneData(bone, writer);
			}

			writer.WriteBlockLengthToOffset(lengthOffset);
		}

		// Muscles
		{
			long lengthOffset = writer.Seek(0, SeekOrigin.Current);
			writer.WriteDummyBlockLength();

			writer.Write((short)TaggedBlockType.Muscles);
			writer.Write((uint)design.Muscles.Count);
			foreach (MuscleData muscle in design.Muscles) {
				WriteMuscleData(muscle, writer);
			}

			writer.WriteBlockLengthToOffset(lengthOffset);
		}

		writer.WriteBlockLengthToOffset(dataLengthOffset);
	}

	public static CreatureDesign DecodeCreatureDesign(BinaryReader reader) {
		try {
			if (
				reader.ReadChar() != 'E' ||
				reader.ReadChar() != 'V' ||
				reader.ReadChar() != 'O' ||
				reader.ReadChar() != 'L' ||
				reader.ReadChar() != 'D' ||
				reader.ReadChar() != 'S' ||
				reader.ReadChar() != 'G' ||
				reader.ReadChar() != 'N'
			) {
				return null;
			}
			
			ushort version = reader.ReadUInt16();
			if (version > SERIALIZATION_VERSION) {
				Debug.Log($"Unknown serialization format {version} for Creature Design");
				return null;
			}

			uint dataLength = reader.ReadBlockLength();
			long expectedEndByte = reader.BaseStream.Position + dataLength;

			string name = reader.ReadUTF8String();
			List<JointData> joints = new List<JointData>();
			List<BoneData> bones = new List<BoneData>();
			List<MuscleData> muscles = new List<MuscleData>();

			while (reader.BaseStream.Position < expectedEndByte) {
				// Read tagged blocks
				uint blockLength = reader.ReadUInt32();
				long expectedBlockEndByte = reader.BaseStream.Position + (long)blockLength;
				short rawBlockType = reader.ReadInt16();
				if (rawBlockType < 0 || rawBlockType > TaggedBlockType_MAX_VALUE) {
					reader.BaseStream.Seek(expectedBlockEndByte, SeekOrigin.Begin);
					continue;
				}
				TaggedBlockType blockType = (TaggedBlockType)rawBlockType;
				switch (blockType) {
					case TaggedBlockType.Joints: {
						int jointCount = (int)reader.ReadUInt32();
						for (int i = 0; i < jointCount; i++) {
							JointData jointData = ReadJointData(reader);
							joints.Add(jointData);
						}
						break;
					}
					
					case TaggedBlockType.Bones: {
						int boneCount = (int)reader.ReadUInt32();
						for (int i = 0; i < boneCount; i++) {
							BoneData boneData = ReadBoneData(reader);
							bones.Add(boneData);
						}
						break;
					}

					case TaggedBlockType.Muscles: {
						int muscleCount = (int)reader.ReadUInt32();
						for (int i = 0; i < muscleCount; i++) {
							MuscleData muscleData = ReadMuscleData(reader);
							muscles.Add(muscleData);
						}
						break;
					}

					default:
						Debug.LogError("Unknown blockType. This block should have been skipped before the switch.");
						break;
				}
			}

			reader.BaseStream.Seek(expectedEndByte, SeekOrigin.Begin);

			return new CreatureDesign(
				name: name,
				joints: joints,
				bones: bones,
				muscles: muscles
			);
		} catch {
			return null;
		}
	}

	private static void WriteJointData(JointData jointData, BinaryWriter writer) {
		// Two bytes length so the reader can be forwards compatible and just jump
		// to the next joint without knowing all the properties
		long lengthOffset = writer.Seek(0, SeekOrigin.Current);
		writer.Write((ushort)0);
		// Two byte bitfield that defines which of the optional properties are serialized.
		ushort optPropertyFlags = 0;
		bool serializeWeight = jointData.weight != 1;
		bool serializeFitnessPenalty = jointData.fitnessPenaltyForTouchingGround != 0;
		if (serializeWeight) {
			optPropertyFlags |= (1 << 0);
		}
		if (serializeFitnessPenalty) {
			optPropertyFlags |= (1 << 1);
		}
		writer.Write(optPropertyFlags);
		writer.Write((int)jointData.id);
		writer.Write(jointData.position.x);
		writer.Write(jointData.position.y);
		if (serializeWeight) {
			writer.Write(jointData.weight);
		}
		if (serializeFitnessPenalty) {
			writer.Write(jointData.fitnessPenaltyForTouchingGround);
		}

		long nextOffset = writer.Seek(0, SeekOrigin.Current);
		writer.Seek((int)lengthOffset, SeekOrigin.Begin);
		writer.Write((ushort)(nextOffset - lengthOffset - 2));
		writer.Seek((int)nextOffset, SeekOrigin.Begin);
	}

	private static JointData ReadJointData(BinaryReader reader) {
		ushort dataLength = reader.ReadUInt16();
		long endByte = reader.BaseStream.Position + dataLength;

		ushort flags = reader.ReadUInt16();
		bool weightIsSerialized = (flags & (1 << 0)) != 0;
		bool fitnessPenaltyIsSerialized = (flags & (1 << 1)) != 0;
		int id = reader.ReadInt32();
		float positionX = reader.ReadSingle();
		float positionY = reader.ReadSingle();
		float weight = 1;
		if (weightIsSerialized) {
			weight = reader.ReadSingle();
		}
		float fitnessPenalty = 0;
		if (fitnessPenaltyIsSerialized) {
			fitnessPenalty = reader.ReadSingle();
		}

		reader.BaseStream.Seek(endByte, SeekOrigin.Begin);

		return new JointData(
			id: id,
			position: new Vector2(positionX, positionY),
			weight: weight,
			penalty: fitnessPenalty,
			isGooglyEye: false
		);
	}

	private static void WriteBoneData(BoneData boneData, BinaryWriter writer) {
		// Two bytes length so the reader can be forwards compatible and just jump
		// to the next bone without knowing all the properties
		long lengthOffset = writer.Seek(0, SeekOrigin.Current);
		writer.Write((ushort)0);	

		// Two byte bitfield that defines which optional properties are serialized and contains
		// boolean property values.
		ushort flags = 0;
		bool serializeWeight = boneData.weight != 1;
		if (serializeWeight) {
			flags |= (1 << 0);
		}
		if (boneData.isWing) {
			flags |= (1 << 1);
		}
		if (boneData.inverted) {
			flags |= (1 << 2);
		}
		if (boneData.legacy) {
			flags |= (1 << 3);
		}
		writer.Write(flags);
		writer.Write((int)boneData.id);
		writer.Write((int)boneData.startJointID);
		writer.Write((int)boneData.endJointID);
		if (serializeWeight) {
			writer.Write(boneData.weight);
		}

		long nextOffset = writer.Seek(0, SeekOrigin.Current);
		writer.Seek((int)lengthOffset, SeekOrigin.Begin);
		writer.Write((ushort)(nextOffset - lengthOffset - 2));
		writer.Seek((int)nextOffset, SeekOrigin.Begin);
	}

	private static BoneData ReadBoneData(BinaryReader reader) {
		ushort dataLength = reader.ReadUInt16();
		long endByte = reader.BaseStream.Position + dataLength;

		ushort flags = reader.ReadUInt16();
		bool weightIsSerialized = (flags & (1 << 0)) != 0;
		bool isWing = (flags & (1 << 1)) != 0;
		bool inverted = (flags & (1 << 2)) != 0;
		bool legacy = (flags & (1 << 3)) != 0;

		int id = reader.ReadInt32();
		int startJointID = reader.ReadInt32();
		int endJointID = reader.ReadInt32();
		float weight = 1;
		if (weightIsSerialized) {
			weight = reader.ReadSingle();
		}
	
		reader.BaseStream.Seek(endByte, SeekOrigin.Begin);

		return new BoneData(
			id: id,
			startJointID: startJointID,
			endJointID: endJointID,
			weight: weight,
			isWing: isWing,
			inverted: inverted,
			legacy: legacy
		);
	}

	private static void WriteMuscleData(MuscleData muscleData, BinaryWriter writer) {
		// Two bytes length so the reader can be forwards compatible and just jump
		// to the next muscle without knowing all the properties
		long lengthOffset = writer.Seek(0, SeekOrigin.Current);
		writer.Write((ushort)0);	

		// Two byte bitfield that defines which optional properties are serialized and contains
		// boolean property values.
		ushort flags = 0;
		bool serializeStrength = muscleData.strength != 1;
		bool serializeUserId = !string.IsNullOrEmpty(muscleData.userId);
		if (serializeStrength) {
			flags |= (1 << 0);
		}
		if (muscleData.canExpand) {
			flags |= (1 << 1);
		}
		if (serializeUserId) {
			flags |= (1 << 2);
		}
		writer.Write(flags);
		writer.Write((int)muscleData.id);
		writer.Write((int)muscleData.startBoneID);
		writer.Write((int)muscleData.endBoneID);
		if (serializeStrength) {
			writer.Write(muscleData.strength);
		}
		if (serializeUserId) {
			string userId = muscleData.userId;
			if (userId.Length > 10000) {
				userId = userId.Substring(0, 10000);
			}
			writer.WriteUTF8String(userId);
		}

		long nextOffset = writer.Seek(0, SeekOrigin.Current);
		writer.Seek((int)lengthOffset, SeekOrigin.Begin);
		writer.Write((ushort)(nextOffset - lengthOffset - 2));
		writer.Seek((int)nextOffset, SeekOrigin.Begin);
	}

	private static MuscleData ReadMuscleData(BinaryReader reader) {
		ushort dataLength = reader.ReadUInt16();
		long endByte = reader.BaseStream.Position + dataLength;

		ushort flags = reader.ReadUInt16();
		bool strengthIsSerialized = (flags & (1 << 0)) != 0;
		bool canExpand = (flags & (1 << 1)) != 0;
		bool userIdIsSerialized = (flags & (1 << 2)) != 0;

		int id = reader.ReadInt32();
		int startBoneID = reader.ReadInt32();
		int endBoneID = reader.ReadInt32();
		float strength = 1;
		if (strengthIsSerialized) {
			strength = reader.ReadSingle();
		}
		string userId = "";
		if (userIdIsSerialized) {
			userId = reader.ReadUTF8String();
		}

		reader.BaseStream.Seek(endByte, SeekOrigin.Begin);

		return new MuscleData(
			id: id,
			startBoneID: startBoneID,
			endBoneID: endBoneID,
			strength: strength,
			canExpand: canExpand,
			userId: userId
		);
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
			return CreatureDesign.Decode(encoded);
		}

		return LegacyCreatureParser.ParseCreatureDesign(name, encoded);
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
		if (!File.Exists(oldPath)) return;

		var creatureDesign = LoadCreatureDesign(oldName);
		creatureDesign.Name = newName;
		DeleteCreatureSave(oldName);
		SaveCreatureDesign(creatureDesign, true);
	}

	/// <summary>
	/// Returns true if a creature design save with the specified name already exists.
	/// </summary>
	/// <param name="name">The name of the creature design.</param>
	public static bool CreatureExists(string name) {
		return GetCreatureNames().Select(n => n.ToLower()).Contains(name.ToLower());
	}

	/// <summary>
	/// Loads the names of all the creature save files into the 
	/// creatureNames array.
	/// </summary>
	public static List<string> GetCreatureNames() {
		
		if (IsWebGL()) return GetDefaultCreatureNames();

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

		var existingNames = GetCreatureNames().Select(n => n.ToLower());
		int counter = 2;
		var finalName = suggestedName;
		while (existingNames.Contains(finalName.ToLower())) {
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
				var design = ParseCreatureDesign(creature.Value, creature.Key);
				SaveCreatureDesign(design, false);
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
		if (MigrationDidBegin != null) MigrationDidBegin();

		var creatureNames = GetCreatureNamesFromPlayerPrefs();
		foreach (var creatureName in creatureNames) {
			var saveData = PlayerPrefs.GetString(creatureName, "");
			if (!string.IsNullOrEmpty(saveData) && !CreatureExists(creatureName)) {
				var design = ParseCreatureDesign(saveData, creatureName);
				SaveCreatureDesign(design, false);
			}
		}

		Settings.DidMigrateCreatureSaves = true;
		if (MigrationDidEnd != null) MigrationDidEnd();
	}

	private static List<string> GetCreatureNamesFromPlayerPrefs() {
		return new List<string>(Settings.CreatureNames.Split('\n'));
	}

	#endregion
}
