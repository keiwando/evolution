using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class FileUtil {

    public static readonly char[] INVALID_FILENAME_CHARACTERS = new char[] {
        '\\', '/', '<', '>', ':', '"', '|', '?', '*',   
    };

    public static string Sanitize(string filename) {

        var sanitized = string.Join("_", filename.Trim().Split(INVALID_FILENAME_CHARACTERS));

        if (sanitized == "") {
            return "_";
        } else {
            return sanitized;
        }
    }

    public static List<string> GetFilenamesInDirectory(string directory, string extension = "") {

        Directory.CreateDirectory(directory);

        var info = new DirectoryInfo(directory);
        var fileInfo = info.GetFiles();

        return fileInfo.Where(f => f.Name.EndsWith(extension))
               .OrderByDescending(f => f.LastAccessTime)
               .Select(f => f.Name)
               .ToList();
    }

    public static string GetAvailableName(string suggested, string directory) {

        var existingNames = GetFilenamesInDirectory(directory);
        var extension = Path.GetExtension(suggested);
        var name = Sanitize(Path.GetFileNameWithoutExtension(suggested));
        var counter = 1;
        var availableName = string.Format("{0}{1}", name, extension);
        while (existingNames.Contains(availableName)) {
            counter += 1;
            availableName = string.Format("{0} {1}{2}", name, counter, extension);
        }
        return availableName;
    }
}

public static class StreamUtil {

    public static bool CanRead(this BinaryReader binaryReader, long byteCount) {
        var bs = binaryReader.BaseStream;
        return bs.Position + byteCount <= bs.Length;
    }
	
    public static void WriteDummyBlockLength(this BinaryWriter writer) {
		writer.Write((uint)0);
	}
	
    public static void WriteBlockLengthToOffset(this BinaryWriter writer, long offset) {
		long currentOffset = writer.Seek(0, SeekOrigin.Current);
		long blockLength = currentOffset - (offset + 4);
		writer.Seek((int)offset, SeekOrigin.Begin);
		writer.Write((uint)blockLength);
		writer.Seek((int)currentOffset, SeekOrigin.Begin);
	}

    public static uint ReadBlockLength(this BinaryReader reader) {
        return reader.ReadUInt32();
    }

    public static void WriteUTF8String(this BinaryWriter writer, string str) {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        uint length = (uint)bytes.Length;
        writer.Write(length);
        writer.Write(bytes);
    }

    public static string ReadUTF8String(this BinaryReader reader) {
        uint length = reader.ReadUInt32();
        byte[] bytes = reader.ReadBytes((int)length);
        return Encoding.UTF8.GetString(bytes);
    }
}