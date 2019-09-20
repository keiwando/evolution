using System.IO;
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