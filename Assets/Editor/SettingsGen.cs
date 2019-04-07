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
	private const string INPUT_PATH = "Scripts/Settings/settings.txt";
	/// <summary>
	/// The path to the directory that the generated .cs file should be written
	/// to.
	/// </summary>
	/// <remarks> An existing file with the same name will be overwritten!</remarks>
	private const string OUTPUT_DIR = "Scripts/Settings";
	/// <summary>
	/// The name of the generated C# class.
	/// </summary>
	private const string OUTPUT_CLASS_NAME = "Settings";

	private static readonly Regex PARSING_PATTERN = 
		new Regex(@"(bool|string|int|float) ([\w\d_]+);?( ?= ?([^;]+);?)?( ?\(([^)]+)\);?)?");

	private static readonly Regex UPPERCACSE_PATTERN = new Regex("[A-Z]+|[0-9]+");

	private const string IMPORTS = "using UnityEngine;";
	private const string HELPERS = @"	private static bool GetBool(string key) {
		return PlayerPrefs.GetInt(key, 0) == 1;
	}

	private static void SetBool(string key, bool b) {
		PlayerPrefs.SetInt(key, b ? 1 : 0);
		PlayerPrefs.Save();
	}
	
	public static void Save() {
		PlayerPrefs.Save();
	}
	
	public static void Reset() {
		PlayerPrefs.DeleteAll();
		Initialize();
	}";

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

				properties.Add(ToProperty(propName, type));
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
		builder.AppendFormat("\t\tif (GetBool(\"{0}\")) {{ return; }}\n", firstTimeKey);
		builder.AppendFormat("\t\tInitialize();\n");
		builder.AppendFormat("\t\tSetBool(\"{0}\", true);\n", firstTimeKey);
		builder.AppendLine("\t\tSave();");
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

	private static string ToProperty(string propName, string type) {

		var key = ToKey(propName);

		switch (type) { 
		case "bool": return GenerateBoolProperty(propName, key);
		case "string": return GenerateStringProperty(propName, key);
		case "int": return GenerateIntProperty(propName, key);
		case "float": return GenerateFloatProperty(propName, key);
		default: throw new System.Exception(string.Format("Unrecognized type: {0}", type));
		}
	}

	private static string GenerateBoolProperty(string propName, string key) {
		return string.Format("\tpublic static bool {0} {{\n" +
		                     "\t\tget {{ return GetBool({1}); }}\n" +
		                     "\t\tset {{ SetBool({1}, value); }}\n" +
		                     "\t}}", propName, key);
	}

	private static string GenerateStringProperty(string propName, string key) {
		return string.Format("\tpublic static string {0} {{\n" +
		                     "\t\tget {{ return PlayerPrefs.GetString({1}, \"\"); }}\n" +
		                     "\t\tset {{ PlayerPrefs.SetString({1}, value); Save(); }}\n" +
		                     "\t}}", propName, key);
	}

	private static string GenerateIntProperty(string propName, string key) {
		return string.Format("\tpublic static int {0} {{\n" +
		                     "\t\tget {{ return PlayerPrefs.GetInt({1}, -1); }}\n" +
		                     "\t\tset {{ PlayerPrefs.SetInt({1}, value); Save(); }}\n" +
		                     "\t}}", propName, key);
	}

	private static string GenerateFloatProperty(string propName, string key) {
		return string.Format("\tpublic static float {0} {{\n" +
		                     "\t\tget {{ return PlayerPrefs.GetFloat({1}, -1); }}\n" +
		                     "\t\tset {{ PlayerPrefs.SetFloat({1}, value); Save(); }}\n" +
		                     "\t}}", propName, key);
	}

	private static string ToKey(string pascalCaseProperty) {
		return string.Format("{0}_KEY", ToUpperCase(pascalCaseProperty));
	}

	private static string ToUpperCase(string pascalCase) {

		return UPPERCACSE_PATTERN.Replace(pascalCase, "_$0", int.MaxValue, 2).ToUpper();
	}
}
