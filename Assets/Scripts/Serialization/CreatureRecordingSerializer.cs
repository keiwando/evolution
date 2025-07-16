using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution.Scenes;
using Keiwando.NFSO;

public class CreatureRecordingSerializer {

  private const string SAVE_FOLDER = "GalleryRecordings";
  public const string FILE_EXTENSION = ".evolutiongallery";

  public static readonly Regex EXTENSION_PATTERN = new Regex(FILE_EXTENSION);

  public static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
  private const ushort LATEST_SERIALIZATION_VERSION = 1;

  public static void SaveCreatureRecordingFile(CreatureRecording recording) {

    string creatureName = recording.creatureDesign.Name;
		string dateString = recording.date.ToString("MMM dd, yyyy");
		string filename = string.Format("{0} - {1} - Gen {2}", creatureName, dateString, recording.generation);

    SaveCreatureRecordingFile(filename, recording, false);
 
  }

  public static void SaveCreatureRecordingFile(string name, CreatureRecording recording, bool overwrite = false) {

    name = EXTENSION_PATTERN.Replace(name, "");

    if (!overwrite) {
      name = GetAvailableCreatureRecordingName(name);
    }
    var path = PathToCreatureRecordingSave(name);

    CreateSaveFolder();

    using (var stream = File.Open(path, FileMode.Create))
    using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
      WriteCreatureRecording(recording, writer);
    }
  }

  public static CreatureRecording LoadCreatureRecording(string name) {

    var path = PathToCreatureRecordingSave(name);
    if (!File.Exists(path)) {
      return null;
    }

    using (var stream = File.Open(path, FileMode.Open))
    using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8)) {
      return DecodeCreatureRecording(reader);
    }
  }

  public static DateTime? LoadCreatureRecordingDate(string name) {

    var path = PathToCreatureRecordingSave(name);
    if (!File.Exists(path)) {
      return null;
    }

    using (var stream = File.Open(path, FileMode.Open))
    using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8)) {
      return ReadDateOfCreatureRecordingFile(reader);
    }
  }

  public static void DeleteCreatureRecording(string name) {
    var path = CreatureRecordingSerializer.PathToCreatureRecordingSave(name);
    if (File.Exists(path)) {
      File.Delete(path);
    }
  }

  private static void WriteCreatureRecording(CreatureRecording recording, BinaryWriter writer) {

    // ### Header ###

    // Magic Bytes
    writer.Write((char)'E');
    writer.Write((char)'V');
    writer.Write((char)'O');
    writer.Write((char)'L');
    writer.Write((char)'G');
    writer.Write((char)'A');
    writer.Write((char)'L');
    writer.Write((char)'L');

    // Version
    ushort version = LATEST_SERIALIZATION_VERSION;
    writer.Write(version);
    
    // ### Content ###

    long metadataBlockLengthOffset = writer.Seek(0, SeekOrigin.Current);
    writer.WriteDummyBlockLength();
    // Metadata
    int generation = recording.generation;
    writer.Write(generation);
    long dateValue = recording.date.ToBinary();
    writer.Write(dateValue);
    byte task = (byte)recording.task;
    writer.Write(task);
    recording.stats.Encode(writer);
    writer.Write(recording.networkInputCount);
    writer.Write(recording.networkOutputCount);
    recording.networkSettings.Encode(writer);

    writer.WriteBlockLengthToOffset(metadataBlockLengthOffset);

    CreatureSerializer.WriteCreatureDesign(recording.creatureDesign, writer);
    recording.sceneDescription.Encode(writer);
    recording.movementData.Encode(writer);
  }

  public static CreatureRecording DecodeCreatureRecording(BinaryReader reader) {

    try {
      if (
        reader.ReadChar() != 'E' ||
        reader.ReadChar() != 'V' ||
        reader.ReadChar() != 'O' ||
        reader.ReadChar() != 'L' ||
        reader.ReadChar() != 'G' ||
        reader.ReadChar() != 'A' ||
        reader.ReadChar() != 'L' ||
        reader.ReadChar() != 'L'
      ) {
        return null;
      }

      ushort version = reader.ReadUInt16();
      if (version > LATEST_SERIALIZATION_VERSION) {
        Debug.Log($"Unknown CreatureRecording serialization version {version}");
        return null;
      }

      uint metadataBlockLengthOffset = reader.ReadBlockLength();
      long byteAfterMetadata = reader.BaseStream.Position + (long)metadataBlockLengthOffset;
      
      int generation = reader.ReadInt32();
      long dateValue = reader.ReadInt64();
      DateTime date = DateTime.FromBinary(dateValue);
      byte rawObjective = reader.ReadByte();
      Objective task = Objective.Running;
      if (rawObjective <= (byte)ObjectiveUtil.LAST_OBJECTIVE) {
        task = (Objective)rawObjective;
      }
      CreatureStats creatureStats = CreatureStats.Decode(reader);
      int networkInputCount = reader.ReadInt32();
      int networkOutputCount = reader.ReadInt32();
      NeuralNetworkSettings settings = NeuralNetworkSettings.Decode(reader);

      reader.BaseStream.Seek(byteAfterMetadata, SeekOrigin.Begin);

      CreatureDesign creatureDesign = CreatureSerializer.DecodeCreatureDesign(reader);
      if (creatureDesign == null) {
        return null;
      }

      SimulationSceneDescription sceneDescription = SimulationSceneDescription.Decode(reader);
      if (sceneDescription == null) {
        return null;
      }

      CreatureRecordingMovementData movementData = CreatureRecordingMovementData.Decode(reader);
      if (movementData == null) {
        return null;
      }

      CreatureRecording recording = new CreatureRecording(
        creatureDesign: creatureDesign,
        sceneDescription: sceneDescription,
        movementData: movementData,
        task: task,
        generation: generation,
        stats: creatureStats,
        networkInputCount: networkInputCount,
        networkOutputCount: networkOutputCount,
        networkSettings: settings
      );
      recording.date = date;
      return recording;

    } catch {
      return null;
    }
  }

  public static DateTime? ReadDateOfCreatureRecordingFile(BinaryReader reader) {

   try {
      if (
        reader.ReadChar() != 'E' ||
        reader.ReadChar() != 'V' ||
        reader.ReadChar() != 'O' ||
        reader.ReadChar() != 'L' ||
        reader.ReadChar() != 'G' ||
        reader.ReadChar() != 'A' ||
        reader.ReadChar() != 'L' ||
        reader.ReadChar() != 'L'
      ) {
        return null;
      }

      ushort version = reader.ReadUInt16();
      if (version > LATEST_SERIALIZATION_VERSION) {
        Debug.Log($"Unknown CreatureRecording serialization version {version}");
        return null;
      }

      reader.ReadBlockLength();
      int generation = reader.ReadInt32();
      long dateValue = reader.ReadInt64();
      DateTime date = DateTime.FromBinary(dateValue);
      return date;
    } catch {
      return null;
    } 
  }

  private static string GetAvailableCreatureRecordingName(string suggestedName) {

    var existingNames = GetCreatureRecordingFilenames().Select(n => n.ToLower());
    int counter = 2;
		var finalName = suggestedName;
		while (existingNames.Contains(finalName.ToLower() + FILE_EXTENSION)) {
			finalName = string.Format("{0} ({1})", suggestedName, counter);
			counter++;
		}
		return finalName;
  }

  public static List<string> GetCreatureRecordingFilenames() {
    return FileUtil.GetFilenamesInDirectory(RESOURCE_PATH, FILE_EXTENSION);
  }

  public static string PathToCreatureRecordingSave(string name) {
    return Path.Combine(RESOURCE_PATH, name + FILE_EXTENSION);
  }

  private static void CreateSaveFolder() {
    Directory.CreateDirectory(RESOURCE_PATH);
  }
}
