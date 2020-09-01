using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SettingsGen {

	/// <summary>
	/// The path to the settings file to be parsed relative to the root of
	/// the assets directory.
	/// </summary>
	private const string INPUT_PATH = "Scripts/Data/settings.txt";
	/// <summary>
	/// The path to the directory that the generated .cs file should be written
	/// to.
	/// </summary>
	/// <remarks> An existing file with the same name will be overwritten!</remarks>
	private const string OUTPUT_DIR = "Scripts/Data";
	/// <summary>
	/// The name of the generated C# class.
	/// </summary>
	private const string OUTPUT_CLASS_NAME = "Settings";

	private static readonly Regex PARSING_PATTERN = 
		new Regex(@"(bool|string|int|float) ([\w\d_]+);?( ?= ?([^; ]+);?)?( ?\(([^)]+)\);?)?");

	private static readonly Regex UPPERCACSE_PATTERN = new Regex("[A-Z]+|[0-9]+");

	private const string IMPORTS = "using UnityEngine;\nusing System.Collections.Generic;";
	private const string HELPERS = @" public static void Reset() {
		Store.DeleteAll();
		Initialize();
	}

	public static ISettingsStore Store = new PlayerPrefsSettingsStore();
	
	public interface ISettingsStore {
		void SetBool(string key, bool b);
		void SetString(string key, string value);
		void SetInt(string key, int value);
		void SetFloat(string key, float value);

		bool GetBool(string key, bool defaultValue = false);
		string GetString(string key, string defaultValue = """");
		int GetInt(string key, int defaultValue = 0);
		float GetFloat(string key, float defaultValue = 0f);

		void DeleteAll();
	}

	public class PlayerPrefsSettingsStore : ISettingsStore {

		public void SetBool(string key, bool b) {
			PlayerPrefs.SetInt(key, b ? 1 : 0);
			PlayerPrefs.Save();
		}

		public void SetString(string key, string value) {
			PlayerPrefs.SetString(key, value);
			PlayerPrefs.Save();
		}

		public void SetInt(string key, int value) {
			PlayerPrefs.SetInt(key, value);
			PlayerPrefs.Save();
		}

		public void SetFloat(string key, float value) {
			PlayerPrefs.SetFloat(key, value);
			PlayerPrefs.Save();
		}

		public bool GetBool(string key, bool defaultValue = false) {
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
		}

		public string GetString(string key, string defaultValue = """") {
			return PlayerPrefs.GetString(key, defaultValue);
		}

		public int GetInt(string key, int defaultValue = 0) {
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public float GetFloat(string key, float defaultValue = 0f) {
			return PlayerPrefs.GetFloat(key, defaultValue);
		}

		public void DeleteAll() {
			PlayerPrefs.DeleteAll();
		}
	}

	public class DictionaryStore : ISettingsStore {

		private Dictionary<string, bool> bools = new Dictionary<string, bool>();
		private Dictionary<string, string> strings = new Dictionary<string, string>();
		private Dictionary<string, int> ints = new Dictionary<string, int>();
		private Dictionary<string, float> floats = new Dictionary<string, float>();

		public void SetBool(string key, bool b) {
			bools[key] = b;
		}

		public void SetString(string key, string value) {
			strings[key] = value;
		}

		public void SetInt(string key, int value) {
			ints[key] = value;
		}

		public void SetFloat(string key, float value) {
			floats[key] = value;
		}

		public bool GetBool(string key, bool defaultValue = false) {
			bool value;
			return bools.TryGetValue(key, out value) ? value : defaultValue;
		}

		public string GetString(string key, string defaultValue = """") {
			string value;
			return strings.TryGetValue(key, out value) ? value : defaultValue;
		}

		public int GetInt(string key, int defaultValue = 0) {
			int value;
			return ints.TryGetValue(key, out value) ? value : defaultValue;
		}

		public float GetFloat(string key, float defaultValue = 0f) {
			float value;
			return floats.TryGetValue(key, out value) ? value : defaultValue;
		}

		public void DeleteAll() {
			bools.Clear();
			strings.Clear();
			ints.Clear();
			floats.Clear();
		}
	}
	";

	[MenuItem("Tools/SettingsGen/Generate")]
	public static void Generate() {

		var inputPath = Path.Combine(Application.dataPath, INPUT_PATH);
		var outputPath = Path.Combine(Application.dataPath, OUTPUT_DIR);
		outputPath = Path.Combine(outputPath, string.Format("{0}.cs", OUTPUT_CLASS_NAME));

		var keyDeclarations = new List<string>();
		var properties = new List<string>();
		var initializations = new List<string>();

		var lines = new List<string>(File.ReadAllLines(inputPath));
		foreach (var line in lines) {
			if ("".Equals(line)) { continue; }

			var match = PARSING_PATTERN.Match(line);
			if (match.Success) {

				var groups = match.Groups;
				var type = groups[1].Value;
				var propName = groups[2].Value;
				var init = groups[4].Success ? groups[4].Value : null;
				var forcedKey = groups[6].Success ? groups[6].Value : null;

				properties.Add(ToProperty(propName, type, init));
				keyDeclarations.Add(ToKeyMember(propName, forcedKey));

				if (init != null) {
					initializations.Add(string.Format("{0} = {1};", propName, init));
				}

			} else {
				Debug.Log(string.Format("Invalid line: {0}", line));
			}
		}

		var builder = new StringBuilder();
		builder.AppendLine(IMPORTS);
		builder.AppendLine("");
		builder.AppendLine(string.Format("public class {0} {{", OUTPUT_CLASS_NAME));
		builder.AppendLine("");
		foreach (var keyInit in keyDeclarations) {
			builder.AppendLine(keyInit);
		}
		builder.AppendLine("");
		foreach (var prop in properties) {
			builder.AppendLine(prop);
			builder.AppendLine("");
		}
		builder.AppendLine(CreateInitializer(initializations));
		builder.AppendLine(HELPERS);
		builder.AppendLine("}");

		var fileInfo = new FileInfo(outputPath);
		if (!fileInfo.Exists)
			Directory.CreateDirectory(fileInfo.Directory.FullName);
		
		File.WriteAllText(outputPath, builder.ToString());
	}

	private static string CreateInitializer(List<string> initializations) {

		// Decrease the potential of this initialization flag being used as a settings
		// key
		// Use a constant random string that doesn't get regenerated each time
		var uuid = "e31cf645-7751-4a7b-ae0d-2ca38f6063b8"; //System.Guid.NewGuid().ToString();
		var firstTimeKey = string.Format("ALREADY_INITIALIZED_{0}", uuid);
		var builder = new StringBuilder();
		builder.AppendFormat("\tstatic {0}() {{\n", OUTPUT_CLASS_NAME);
		builder.AppendFormat("\t\tif (Store.GetBool(\"{0}\")) {{ return; }}\n", firstTimeKey);
		builder.AppendFormat("\t\tInitialize();\n");
		builder.AppendFormat("\t\tStore.SetBool(\"{0}\", true);\n", firstTimeKey);
		builder.AppendLine("\t}");
		builder.AppendLine("");
		builder.AppendLine("\tprivate static void Initialize() {");
		foreach (var init in initializations) {
			builder.AppendLine(string.Format("\t\t{0}", init));
		}
		builder.AppendLine("\t}");
		return builder.ToString();
	}

	private static string ToKeyMember(string pascalCaseKey, string forcedKey) {

		var identifier = ToKey(pascalCaseKey);
		var key = forcedKey == null ? identifier : forcedKey;
		return string.Format("\tprivate const string {0} = \"{1}\";", identifier, key);
	} 

	private static string ToProperty(string propName, string type, string defaultValue = null) {

		var key = ToKey(propName);

		switch (type) { 
		case "bool": return GenerateBoolProperty(propName, key, defaultValue);
		case "string": return GenerateStringProperty(propName, key, defaultValue);
		case "int": return GenerateIntProperty(propName, key, defaultValue);
		case "float": return GenerateFloatProperty(propName, key, defaultValue);
		default: throw new System.Exception(string.Format("Unrecognized type: {0}", type));
		}
	}

	private static string GenerateBoolProperty(string propName, string key, string defaultVal) {
		var defaultValue = defaultVal == null ? "true" : defaultVal;
		return string.Format("\tpublic static bool {0} {{\n" +
		                     "\t\tget {{ return Store.GetBool({1}, {2}); }}\n" +
		                     "\t\tset {{ Store.SetBool({1}, value); }}\n" +
		                     "\t}}", propName, key, defaultValue);
	}

	private static string GenerateStringProperty(string propName, string key, string defaultVal) {
		var defaultValue = defaultVal == null ? "\"\"" : defaultVal; 
		return string.Format("\tpublic static string {0} {{\n" +
		                     "\t\tget {{ return Store.GetString({1}, {2}); }}\n" +
		                     "\t\tset {{ Store.SetString({1}, value); }}\n" +
		                     "\t}}", propName, key, defaultValue);
	}

	private static string GenerateIntProperty(string propName, string key, string defaultVal) {
		var defaultValue = defaultVal == null ? "-1" : defaultVal;
		return string.Format("\tpublic static int {0} {{\n" +
		                     "\t\tget {{ return Store.GetInt({1}, {2}); }}\n" +
		                     "\t\tset {{ Store.SetInt({1}, value); }}\n" +
		                     "\t}}", propName, key, defaultValue);
	}

	private static string GenerateFloatProperty(string propName, string key, string defaultVal) {
		var defaultValue = defaultVal == null ? "-1" : defaultVal;
		return string.Format("\tpublic static float {0} {{\n" +
		                     "\t\tget {{ return Store.GetFloat({1}, {2}); }}\n" +
		                     "\t\tset {{ Store.SetFloat({1}, value); }}\n" +
		                     "\t}}", propName, key, defaultValue);
	}

	private static string ToKey(string pascalCaseProperty) {
		return string.Format("{0}_KEY", ToUpperCase(pascalCaseProperty));
	}

	private static string ToUpperCase(string pascalCase) {

		return UPPERCACSE_PATTERN.Replace(pascalCase, "_$0", int.MaxValue, 2).ToUpper();
	}
}
