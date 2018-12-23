using System;
using System.IO;

namespace Keiwando.NativeFileSO.Samples { 

	public class FileWriter {

		private static readonly string NAME = "NativeFileSOTest.txt";

		public static void WriteTestFile(string path) {
			
			var fullPath = Path.Combine(path, NAME);
			var contents = "Native File SO Test file";

			File.WriteAllText(fullPath, contents);
		}

		public static void DeleteTestFile(string path) { 
			
			var fullPath = Path.Combine(path, NAME);
			File.Delete(fullPath);
		}
	}
}