using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution.Scenes;

public class CreatureRecordingSerializer {

  private const string SAVE_FOLDER = "GalleryRecordings";
  public const string FILE_EXTENSION = ".evolutiongallery";

  public static readonly Regex EXTENSION_PATTERN = new Regex(FILE_EXTENSION);

  public static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
  private const ushort LATEST_SERIALIZATION_VERSION = 1;

  public static void SaveCreatureRecordingFile(CreatureRecording recording) {

    string creatureName = recording.creatureDesign.Name;
		string dateString = System.DateTime.Now.ToString("MMM dd, yyyy");
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

    // Metadata
    int generation = recording.generation;
    writer.Write(generation);
    long dateValue = recording.date.ToBinary();
    writer.Write(dateValue);

    // Creature Design
    long creatureDesignStartOffset = writer.Seek(0, SeekOrigin.Current);
    writer.WriteDummyBlockLength();
    CreatureSerializer.WriteCreatureDesign(recording.creatureDesign, writer);
    writer.WriteBlockLengthToOffset(creatureDesignStartOffset);

    // Scene Description
    recording.sceneDescription.Encode(writer);

    recording.movementData.Encode(writer);
  }

  private static CreatureRecording DecodeCreatureRecording(BinaryReader reader) {

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

      int generation = reader.ReadInt32();
      long dateValue = reader.ReadInt64();
      DateTime date = DateTime.FromBinary(dateValue);

      uint creatureDesignDataLength = reader.ReadBlockLength();
      long byteAfterCreatureDesignData = reader.BaseStream.Position + (long)creatureDesignDataLength;
      CreatureDesign creatureDesign = CreatureSerializer.DecodeCreatureDesign(reader, creatureDesignDataLength);
      if (creatureDesign == null) {
        return null;
      }
      reader.BaseStream.Seek(byteAfterCreatureDesignData, SeekOrigin.Begin);

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
        generation: generation,
        sceneDescription: sceneDescription,
        movementData: movementData
      );
      recording.date = date;
      return recording;

    } catch {
      return null;
    }
  }

  private static DateTime? ReadDateOfCreatureRecordingFile(BinaryReader reader) {

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

  private static string PathToCreatureRecordingSave(string name) {
    return Path.Combine(RESOURCE_PATH, name + FILE_EXTENSION);
  }

  private static void CreateSaveFolder() {
    Directory.CreateDirectory(RESOURCE_PATH);
  }
}
