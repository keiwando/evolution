using System;
namespace Keiwando.NativeFileSO {

	public interface INativeFileSODesktop: INativeFileSO {

		void OpenFiles(SupportedFileType[] fileTypes, 
		               bool canSelectMultiple,
					   string title, 
		               string directory, 
					   Action<bool, OpenedFile[]> onCompletion);

		OpenedFile[] OpenFilesSync(SupportedFileType[] fileTypes, 
		                           bool canSelectMultiple, 
								   string title, 
		                           string directory);

		void SelectOpenPaths(SupportedFileType[] fileTypes, 
		                     bool canSelectMultiple,
							 string title, 
		                     string directory,
							 Action<bool, string[]> onCompletion);

		string[] SelectOpenPathsSync(SupportedFileType[] fileTypes, 
		                             bool canSelectMultiple,
									 string title, 
		                             string directory);

		void SaveFile(FileToSave file, string title, string directory);

		void SelectSavePath(FileToSave file,
							string title, 
		                    string directory,
							Action<bool, string> onCompletion);

		string SelectSavePathSync(FileToSave file,
								  string title, 
		                          string directory);
	}
}
