using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class BuildManager {

	private const string WIN_FOLDER_PATH   = "/../Builds/Windows/Evolution.exe";
	private const string MACOS_FOLDER_PATH = "/../Builds/Mac/Evolution.app";
	private const string LINUX_EXPORT_PATH = "/../Builds/Linux/Evolution.x86_64";
	private const string WEBGL_EXPORT_PATH = "/../Builds/WebGL/Evolution";

	[MenuItem("BuildTools/Build All Standalones")]
	public static void BuildAllStandalones() {

		BuildMacOS();
		BuildWindows();
		BuildLinux();
		BuildWebGL();
	}

	[MenuItem("BuildTools/Build Windows")]
	public static void BuildWindows() {

		var path = Application.dataPath + WIN_FOLDER_PATH;

		Build(BuildTarget.StandaloneWindows, BuildOptions.CompressWithLz4HC, path);
	}

	[MenuItem("BuildTools/Build macOS")]
	public static void BuildMacOS() {

		var path = Application.dataPath + MACOS_FOLDER_PATH;
		//UnityEngine.Debug.Log("Path = " + path);

		Build(BuildTarget.StandaloneOSX, BuildOptions.ShowBuiltPlayer | BuildOptions.CompressWithLz4HC, path);
	}

	[MenuItem("BuildTools/Build Linux")]
	public static void BuildLinux() {

		var path = Application.dataPath + LINUX_EXPORT_PATH;

		Build(BuildTarget.StandaloneLinux64, BuildOptions.CompressWithLz4HC, path);
	}

	[MenuItem("BuildTools/Build WebGL")]
	public static void BuildWebGL() {

		var path = Application.dataPath + WEBGL_EXPORT_PATH;

		Build(BuildTarget.WebGL, BuildOptions.CompressWithLz4HC, path);
	}

	private static void Build(BuildTarget target, BuildOptions options, string path) {

		var buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.locationPathName = path;
		buildPlayerOptions.scenes = new string[] {
			"Assets/Scenes/CreatureBuildingScene.unity", 
			"Assets/Scenes/EvolutionScene.unity"
		};
		buildPlayerOptions.options = options;
		buildPlayerOptions.target = target;
			
		BuildPipeline.BuildPlayer(buildPlayerOptions);
	}
}
