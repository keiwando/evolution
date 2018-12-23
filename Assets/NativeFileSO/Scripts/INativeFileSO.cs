using System;

namespace Keiwando.NativeFileSO { 

	public interface INativeFileSO {

		void OpenFile(SupportedFileType[] supportedTypes, Action<bool, OpenedFile> onCompletion);
		void OpenFiles(SupportedFileType[] supportedTypes, Action<bool, OpenedFile[]> onCompletion);

		void SaveFile(FileToSave file);
	}
}


