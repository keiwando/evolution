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
using Keiwando.Evolution.Scenes;
using System.Security.Cryptography.X509Certificates;

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
	private const ushort LATEST_SERIALIZATION_VERSION = 1;

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
	public static string SaveSimulation(SimulationData data, string copyMissingBestCreaturesDataFromFilePath = null) {

		string filename = GetSuggestedSimulationName(data);
		
		// Save without overwriting existing saves
		return SaveSimulationFile(filename, data, false, copyMissingBestCreaturesDataFromFilePath);
	}

	/// <summary>
	/// Saves the encoded simulation to a file with the specified name.
	/// </summary>
	/// <param name="name">The filename without an extension.</param>
	/// <param name="encodedData"></param>
	/// <returns>The filename of the save file without the extension</returns>
	public static string SaveSimulationFile(string name, SimulationData data, bool overwrite = false, string copyMissingBestCreaturesDataFromFilePath = null) { 

		name = EXTENSION_PATTERN.Replace(name, "");

		if (!overwrite) {
			name = GetAvailableSimulationName(name);
		}
		var path = PathToSimulationSave(name);

		CreateSaveFolder();

		using (var stream = File.Open(path, FileMode.Create))
		using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
			WriteSimulationData(data, writer, copyMissingBestCreaturesDataFromFilePath);
		}
		
		return name;
	}

	/// <summary>
	/// Loads a previously saved simulation data with the specified name
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static SimulationData LoadSimulationData(string name) {

		var path = PathToSimulationSave(name);
		if (!File.Exists(path)) {
			return null;
		}

		using (var stream = File.Open(path, FileMode.Open))
		using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8)) {
			SimulationData simulationData = DecodeSimulationData(reader);
			if (simulationData != null) {
				return simulationData;
			}
		}
		string textContents = File.ReadAllText(path);
		return ParseLegacySimulationData(textContents, name);
	}

	public static string SaveSimulationDataUpdatesIntoExistingFile(SimulationData data, string filePath) {
		// It is not guaranteed that we will be able to update the existing file. We have to 
		// write a separate full serialization file (and replace the old one with it), if the size of
		// intermediate data blocks has to change. We can only reuse the existing file when the only 
		// operations are replacing the same sized data blocks and appending to the end of the file.

		if (!TrySaveSimulationDataUpdatesToExistingFile(data, filePath)) {
			string tmpFilename = SaveSimulation(data, copyMissingBestCreaturesDataFromFilePath: filePath);
			string tmpPath = PathToSimulationSave(tmpFilename);
			string backupFileName = tmpPath + "_bu";
			File.Replace(sourceFileName: tmpPath, destinationFileName: filePath, destinationBackupFileName: backupFileName);
			if (File.Exists(backupFileName)) {
				File.Delete(backupFileName);
			}
			return filePath;
		} else {
			string desiredNewFileName = GetSuggestedSimulationName(data);
			if (PathToSimulationSave(desiredNewFileName) != filePath) {
				string newFileName = GetAvailableSimulationName(desiredNewFileName);
				string newFilePath = PathToSimulationSave(newFileName);
				File.Move(filePath, newFilePath);
				return newFilePath;
			} else {
				return filePath;
			}
		}
	}

	/// Returns whether the existing file could be updated with the new simulation data.
	private static bool TrySaveSimulationDataUpdatesToExistingFile(SimulationData data, string filePath) {

		using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
		using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8))
		using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
			try {

				if (
					reader.ReadChar() != 'E' ||
					reader.ReadChar() != 'V' ||
					reader.ReadChar() != 'O' ||
					reader.ReadChar() != 'L' ||
					reader.ReadChar() != 'S' ||
					reader.ReadChar() != 'I' ||
					reader.ReadChar() != 'M' ||
					reader.ReadChar() != 'L'
				) {
					return false;
				}

				ushort version = reader.ReadUInt16();
				if (version > LATEST_SERIALIZATION_VERSION) {
					Debug.Log($"Unknown SimulationData serialization version {version}");
					return false;
				}

				uint dataLengthBeforeChromosomes = reader.ReadBlockLength();
				long startByteOfChromosomes = reader.BaseStream.Position + (long)dataLengthBeforeChromosomes;

				int simulationDataVersion = (int)reader.ReadUInt16();
				long offsetBeforeSimulationSettings = reader.BaseStream.Position;
				SimulationSettings simulationSettings = SimulationSettings.Decode(reader);
				if (simulationSettings.PopulationSize != data.Settings.PopulationSize) {
					return false;
				}
				reader.BaseStream.Seek(offsetBeforeSimulationSettings, SeekOrigin.Begin);
				data.Settings.Encode(writer);

				reader.BaseStream.Seek(startByteOfChromosomes, SeekOrigin.Begin);

				uint populationDataLength = reader.ReadBlockLength();
				long byteAfterPopulationData = reader.BaseStream.Position + (long)populationDataLength;

				int chromosomeLength = reader.ReadInt32();
				float[][] currentChromosomes = new float[simulationSettings.PopulationSize][];

				byte[] byteData = new byte[chromosomeLength * sizeof(float)];
				for (int i = 0; i < data.Settings.PopulationSize; i++) {
					float[] chromosome = data.CurrentChromosomes[i];
					Buffer.BlockCopy(chromosome, 0, byteData, 0, byteData.Length);
					writer.Write(byteData);
				}

				reader.BaseStream.Seek(byteAfterPopulationData, SeekOrigin.Begin);

				int oldNumberOfBestCreatures = reader.ReadInt32();
				reader.BaseStream.Seek(-4, SeekOrigin.Current);
				writer.Write(data.BestCreatures.Count);

				writer.Seek(0, SeekOrigin.End);
				int newBestCreaturesCount = data.BestCreatures.Count - oldNumberOfBestCreatures;
				if (newBestCreaturesCount > 0) {
					WriteBestCreaturesChromosomeData(
						writer, 
						data, 
						startOffset: oldNumberOfBestCreatures,
						chromosomeLength: chromosomeLength,
						byteData: new byte[chromosomeLength * sizeof(float)]
					);
				}
			} catch {
				return false;
			}
		}

		return true;
	}

	private static void WriteSimulationData(SimulationData data, BinaryWriter writer, string copyMissingBestCreaturesDataFromFilePath = null) {

		// ### Header ###

		// Magic Bytes
		writer.Write((char)'E');
    writer.Write((char)'V');
    writer.Write((char)'O');
    writer.Write((char)'L');
    writer.Write((char)'S');
    writer.Write((char)'I');
    writer.Write((char)'M');
    writer.Write((char)'L');

		// Version 
		ushort version = LATEST_SERIALIZATION_VERSION;
		writer.Write(version);

		// ### Content ###
		
		long dataLengthBeforeChromosomesOffset = writer.Seek(0, SeekOrigin.Current);
		writer.WriteDummyBlockLength();
		
		ushort simulationDataVersion = (ushort)data.Version;
		writer.Write(simulationDataVersion);
		data.Settings.Encode(writer);
		data.NetworkSettings.Encode(writer);
		CreatureSerializer.WriteCreatureDesign(data.CreatureDesign, writer);
		data.SceneDescription.Encode(writer);
		writer.Write(data.LastV2SimulatedGeneration);

		writer.WriteBlockLengthToOffset(dataLengthBeforeChromosomesOffset);

		// Population Chromosomes

		long populationDataLengthOffset = writer.Seek(0, SeekOrigin.Current);
		writer.WriteDummyBlockLength();
		// We first write the population chromosomes so that in the normal case of the population size
		// not changing during the simulation, we can simply append the new best creatures chromosome
		// data to the end of the save file.
		int chromosomeLength = data.CurrentChromosomes.Length > 0 ? data.CurrentChromosomes[0].Length : 0;
		writer.Write(chromosomeLength);

		byte[] byteData = new byte[chromosomeLength * sizeof(float)];
		for (int i = 0; i < data.Settings.PopulationSize; i++) {
			float[] chromosome = data.CurrentChromosomes[i];
			Buffer.BlockCopy(chromosome, 0, byteData, 0, byteData.Length);
			writer.Write(byteData);
		}

		writer.WriteBlockLengthToOffset(populationDataLengthOffset);

		// Best Creatures Chromosomes

		writer.Write(data.BestCreatures.Count);

		bool hasMissingBestCreatureData = false;
		foreach (ChromosomeData? chromosomeData in data.BestCreatures) {
			if (chromosomeData == null) {
				hasMissingBestCreatureData = true;
				break;
			}
		}

		BinaryReader missingChromosomeDataReader = null;
		if (hasMissingBestCreatureData) {
			var stream = new FileStream(copyMissingBestCreaturesDataFromFilePath, FileMode.Open);
			missingChromosomeDataReader = new BinaryReader(stream, System.Text.Encoding.UTF8);
			SkipUntilBestCreaturesDataAndReturnChromosomeLength(missingChromosomeDataReader);
			missingChromosomeDataReader.ReadInt32(); // Number of entries
		}

		try {
			WriteBestCreaturesChromosomeData(
				writer: writer, 
				data: data, 
				startOffset: 0, 
				chromosomeLength: chromosomeLength, 
				byteData: byteData,
				missingChromosomeDataReader: missingChromosomeDataReader
			);
		} finally {
			if (missingChromosomeDataReader != null) {
				missingChromosomeDataReader.BaseStream.Close();
				missingChromosomeDataReader.Close();
			}
		}
	}

	private static void WriteBestCreaturesChromosomeData(BinaryWriter writer, 
																											 SimulationData data, 
																											 int startOffset, 
																											 int chromosomeLength,
																											 byte[] byteData,
																											 BinaryReader missingChromosomeDataReader = null) {
		byte[] dummyChromosomeData = null;
		CreatureStats dummyStats = null;
		for (int i = startOffset; i < data.BestCreatures.Count; i++) {
			ChromosomeData? chromosomeData = data.BestCreatures[i];
			bool canReadMissingChromosomeData = missingChromosomeDataReader != null && missingChromosomeDataReader.BaseStream.Position < missingChromosomeDataReader.BaseStream.Length;
			if (canReadMissingChromosomeData) {
				if (!chromosomeData.HasValue) {
					chromosomeData = LoadChromosomeData(missingChromosomeDataReader, chromosomeLength: chromosomeLength);
				} else {
					missingChromosomeDataReader.BaseStream.Seek(chromosomeLength * sizeof(float), SeekOrigin.Current);
					CreatureStats.Skip(missingChromosomeDataReader);
				}
			}
			if (!chromosomeData.HasValue) {
				Debug.LogError("Writing full simulation serialization file with incomplete best creatures chromosome data.");
				// This case shouldn't happen, but just in case it does, we still write placeholder data here to 
				// keep the simulation save format valid.
				if (dummyChromosomeData == null) {
					dummyChromosomeData = new byte[chromosomeLength * sizeof(float)];
					dummyStats = new CreatureStats();
				}
				writer.Write(dummyChromosomeData);
				dummyStats.Encode(writer);
			} else {
				Buffer.BlockCopy(chromosomeData.Value.Chromosome, 0, byteData, 0, byteData.Length);
				writer.Write(byteData);
				chromosomeData.Value.Stats.Encode(writer);
			}
		}
	}

	public static SimulationData DecodeSimulationData(BinaryReader reader) {
		try {

			if (
        reader.ReadChar() != 'E' ||
        reader.ReadChar() != 'V' ||
        reader.ReadChar() != 'O' ||
        reader.ReadChar() != 'L' ||
        reader.ReadChar() != 'S' ||
        reader.ReadChar() != 'I' ||
        reader.ReadChar() != 'M' ||
        reader.ReadChar() != 'L'
      ) {
        return null;
      }

			ushort version = reader.ReadUInt16();
			if (version > LATEST_SERIALIZATION_VERSION) {
				Debug.Log($"Unknown SimulationData serialization version {version}");
				return null;
			}

			uint dataLengthBeforeChromosomes = reader.ReadBlockLength();
			long startByteOfChromosomes = reader.BaseStream.Position + (long)dataLengthBeforeChromosomes;

			int simulationDataVersion = (int)reader.ReadUInt16();
			SimulationSettings simulationSettings = SimulationSettings.Decode(reader);
			NeuralNetworkSettings networkSettings = NeuralNetworkSettings.Decode(reader);
			CreatureDesign creatureDesign = CreatureSerializer.DecodeCreatureDesign(reader);
			if (creatureDesign == null) {
				return null;
			}
			SimulationSceneDescription sceneDescription = SimulationSceneDescription.Decode(reader);
			if (sceneDescription == null) {
				return null;
			}
			int lastV2SimulatedGeneration = reader.ReadInt32();

			reader.BaseStream.Seek(startByteOfChromosomes, SeekOrigin.Begin);

			uint populationDataLength = reader.ReadBlockLength();
			long byteAfterPopulationData = reader.BaseStream.Position + (long)populationDataLength;

			int chromosomeLength = reader.ReadInt32();
			float[][] currentChromosomes = new float[simulationSettings.PopulationSize][];

			for (int i = 0; i < simulationSettings.PopulationSize; i++) {
				byte[] byteData = reader.ReadBytes(chromosomeLength * sizeof(float));
				float[] chromosome = new float[chromosomeLength];
				Buffer.BlockCopy(byteData, 0, chromosome, 0, byteData.Length);
				currentChromosomes[i] = chromosome;
			}

			reader.BaseStream.Seek(byteAfterPopulationData, SeekOrigin.Begin);

			int numberOfBestCreaturesData = reader.ReadInt32();
			List<ChromosomeData?> bestCreatures = new List<ChromosomeData?>();

			for (int i = 0; i < numberOfBestCreaturesData; i++) {
				// We only need to actually load the latest best creature initially. The rest can be loaded
				// on demand.
				if (i < numberOfBestCreaturesData - 1) {
					reader.BaseStream.Seek(chromosomeLength * sizeof(float), SeekOrigin.Current);
					CreatureStats.Skip(reader);
					bestCreatures.Add(null);
				} else {
					ChromosomeData chromosomeData = LoadChromosomeData(reader, chromosomeLength: chromosomeLength);
					bestCreatures.Add(chromosomeData);
				}
			}

			return new SimulationData(
				settings: simulationSettings,
				networkSettings: networkSettings,
				design: creatureDesign,
				sceneDescription: sceneDescription,
				bestCreatures: bestCreatures,
				currentChromosomes: currentChromosomes,
				lastV2SimulatedGeneration: lastV2SimulatedGeneration
			);
		} catch {
			return null;
		}
	}

	private static ChromosomeData LoadChromosomeData(BinaryReader reader, int chromosomeLength) {
		byte[] byteData = reader.ReadBytes(chromosomeLength * sizeof(float));
		float[] chromosome = new float[chromosomeLength];
		Buffer.BlockCopy(byteData, 0, chromosome, 0, byteData.Length);
		CreatureStats stats = CreatureStats.Decode(reader);
		return new ChromosomeData(chromosome, stats);
	}

	public static int SkipUntilBestCreaturesDataAndReturnChromosomeLength(BinaryReader reader) {
		try {

			if (
        reader.ReadChar() != 'E' ||
        reader.ReadChar() != 'V' ||
        reader.ReadChar() != 'O' ||
        reader.ReadChar() != 'L' ||
        reader.ReadChar() != 'S' ||
        reader.ReadChar() != 'I' ||
        reader.ReadChar() != 'M' ||
        reader.ReadChar() != 'L'
      ) {
        return 0;
      }

			ushort version = reader.ReadUInt16();
			if (version > LATEST_SERIALIZATION_VERSION) {
				Debug.Log($"Unknown SimulationData serialization version {version}");
				return 0;
			}
			uint dataLengthBeforeChromosomes = reader.ReadBlockLength();
			long startByteOfChromosomes = reader.BaseStream.Position + (long)dataLengthBeforeChromosomes;
			reader.BaseStream.Seek(startByteOfChromosomes, SeekOrigin.Begin);

			uint populationDataLength = reader.ReadBlockLength();
			long byteAfterPopulationData = reader.BaseStream.Position + (long)populationDataLength;
			int chromosomeLength = reader.ReadInt32();
			reader.BaseStream.Seek(byteAfterPopulationData, SeekOrigin.Begin);

			return chromosomeLength;

		} catch {
			Debug.LogError("Invalid Simulation file!");
			return 0;
		}
	}

	public static void SkipBestCreatureEntries(BinaryReader reader, int count, int chromosomeLength) {
		int chromosomeByteLength = chromosomeLength * sizeof(float);
		for (int i = 0; i < count; i++) {
			reader.BaseStream.Seek(chromosomeByteLength, SeekOrigin.Current);
			CreatureStats.Skip(reader);
		}
	}

	public static void LoadBestCreatureData(string filepath, SimulationData simulationData, int generationIndex) {
		if (generationIndex < 0 || generationIndex >= simulationData.BestCreatures.Count) {
			return;
		}
		using (var stream = File.Open(filepath, FileMode.Open))
		using (var reader = new BinaryReader(stream)) {
			int chromosomeLength = SkipUntilBestCreaturesDataAndReturnChromosomeLength(reader);
			int chromosomeByteLength = chromosomeLength * sizeof(float);
			int bestCreatureEntriesCount = reader.ReadInt32();
			if (generationIndex >= bestCreatureEntriesCount) {
				Debug.LogError($"Not enough best creature entries in the simulation data file: {filepath}!.");
				return;
			}
			SkipBestCreatureEntries(reader, count: generationIndex, chromosomeLength: chromosomeLength);
			ChromosomeData chromosomeData = LoadChromosomeData(reader, chromosomeLength: chromosomeLength);
			simulationData.BestCreatures[generationIndex] = chromosomeData;
		}
	}

	/// <summary>
	/// Returns the SimulationData of a previously saved simulation.
	/// </summary>
	/// <param name="name">The name of the saved simulation without the file extension.</param>
	public static SimulationData ParseLegacySimulationData(string encoded, string filename = "Unnamed.evol") {

		// Distinguish between JSON and legacy custom encodings
		if (encoded.StartsWith("{")) {
			return SimulationData.Decode(encoded);
		}

		return LegacySimulationLoader.ParseSimulationData(filename, encoded);
	}

	public static string GetSuggestedSimulationName(SimulationData data) {
		string creatureName = data.CreatureDesign.Name;
		string dateString = System.DateTime.Now.ToString("MMM dd, yyyy");
		string objectiveString = ObjectiveUtil.StringRepresentation(data.Settings.Objective);
		int generation = data.BestCreatures.Count + 1;
		string filename = string.Format("{0} - {1} - {2} - Gen {3}", creatureName, objectiveString, dateString, generation);
		return filename;
	}

	public static string GetAvailableSimulationName(SimulationData simulationData) {
		string suggestedName = GetSuggestedSimulationName(simulationData);
		return GetAvailableSimulationName(suggestedName);
	}

	/// <summary>
	/// Returns a simulation save filename that is still available based on the
	/// specified suggested name. (Both without the extension)
	/// </summary>
	private static string GetAvailableSimulationName(string suggestedName) {
		
		var existingNames = GetEvolutionSaveFilenames().Select(n => n.ToLower());
		int counter = 2;
		var finalName = suggestedName;
		while (existingNames.Contains(finalName.ToLower() + FILE_EXTENSION)) {
			finalName = string.Format("{0} ({1})", suggestedName, counter);
			counter++;
		}
		return finalName;
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
		return GetEvolutionSaveFilenames().Select(n => n.ToLower()).Contains(name.ToLower() + FILE_EXTENSION);
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

	/// <summary>
	/// Copies the default simulation files from the resources folder into the savefile directory
	/// </summary>
	private static void CopyDefaultSimulations() {

		CreateSaveFolder();

		var names = new [] {
			// "FROGGER - RUNNING - Default - Gen(70)"
			"FROGGER - Running - Default - Gen 40"
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
