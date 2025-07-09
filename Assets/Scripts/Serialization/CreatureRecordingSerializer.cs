using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public class CreatureRecordingSerializer {

  private const string SAVE_FOLDER = "GalleryRecordings";
  public const string FILE_EXTENSION = ".evolutiongallery";

  public static readonly Regex EXTENSION_PATTERN = new Regex(FILE_EXTENSION);

  public static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

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
    ushort version = 1;
    writer.Write(version);
    
    // ### Content ###

    // Metadata
    int generation = recording.generation;
    writer.Write(generation);
    long dateValue = recording.date.ToBinary();
    writer.Write(dateValue);

    // Creature Design
    long creatureDesignStartOffset = writer.Seek(0, SeekOrigin.Current);
    WriteDummyBlockLength(writer);
    CreatureSerializer.WriteCreatureDesign(recording.creatureDesign, writer);
    WriteBlockLengthToOffset(creatureDesignStartOffset, writer);

    // Scene Description
    
  }

  private static void WriteBlockLengthToOffset(long offset, BinaryWriter writer) {
		long currentOffset = writer.Seek(0, SeekOrigin.Current);
		long blockLength = offset - (currentOffset + 8);
		writer.Seek((int)offset, SeekOrigin.Begin);
		writer.Write((uint)blockLength);
		writer.Seek((int)currentOffset, SeekOrigin.Begin);
	}

	private static void WriteDummyBlockLength(BinaryWriter writer) {
		writer.Write((uint)0);
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
