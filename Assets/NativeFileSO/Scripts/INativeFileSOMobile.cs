using System;
namespace Keiwando.NativeFileSO {

	public interface INativeFileSOMobile {
		
		void OpenFiles(SupportedFileType[] supportedTypes, bool canSelectMultiple);
		void SaveFile(FileToSave file);

		OpenedFile[] GetOpenedFiles();
	}
}
