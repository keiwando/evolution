
namespace Keiwando.NativeFileSO.Samples {

	public class SaveTest {

		public static void Main() {

			string path = "path/to/existing/fileToSave.txt";
			string newFilename = "ExportedFile.txt";

			FileToSave file = new FileToSave(path, newFilename, SupportedFileType.PlainText);

			// Allows the user to choose a save location and saves the 
			// file to that location
			NativeFileSO.shared.SaveFile(file);
		}
	}
}
