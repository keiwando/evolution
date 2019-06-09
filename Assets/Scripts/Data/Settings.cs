using UnityEngine;

public class Settings {

	private const string DID_MIGRATE_CREATURE_SAVES_KEY = "DID_MIGRATE_CREATURE_SAVES_KEY";
	private const string DID_MIGRATE_SIMULATION_SAVES_KEY = "DID_MIGRATE_SIMULATION_SAVES_KEY";
	private const string SHOW_MUSCLE_CONTRACTION_KEY = "showMuscleContraction";
	private const string SHOW_MUSCLES_KEY = "SHOW_MUSCLES_KEY";
	private const string SHOW_ONE_AT_ATIME_KEY = "SHOW_ONE_AT_ATIME_KEY";
	private const string HIDDEN_CREATURE_OPACITY_KEY = "HIDDEN_CREATURE_OPACITY_KEY";
	private const string DONT_SHOW_V_2_SIMULATION_DEPRECATION_OVERLAY_AGAIN_KEY = "DONT_SHOW_V_2_SIMULATION_DEPRECATION_OVERLAY_AGAIN_KEY";
	private const string GRID_SIZE_KEY = "GRID_SIZE";
	private const string GRID_ENABLED_KEY = "GRID_ENABLED";
	private const string HELP_SCREEN_LANGUAGE_KEY = "HELP_SCREEN_LANGUAGE";
	private const string HELP_INDICATOR_SHOWN_KEY = "FIRST_TIME";
	private const string EDITOR_MODE_KEY = "EDITOR_MODE_KEY";
	private const string CREATURE_NAMES_KEY = "_CreatureNames";
	private const string SIMULATION_SETTINGS_KEY = "EVOLUTION_SETTINGS";
	private const string NETWORK_SETTINGS_KEY = "NEURAL NETWORK SETTINGS";
	private const string EDITOR_SETTINGS_KEY = "EDITOR_SETTINGS_KEY";
	private const string CURRENT_CREATURE_DESIGN_KEY = "CURRENT_CREATURE_DESIGN_KEY";

	public static bool DidMigrateCreatureSaves {
		get { return GetBool(DID_MIGRATE_CREATURE_SAVES_KEY, false); }
		set { SetBool(DID_MIGRATE_CREATURE_SAVES_KEY, value); }
	}

	public static bool DidMigrateSimulationSaves {
		get { return GetBool(DID_MIGRATE_SIMULATION_SAVES_KEY, false); }
		set { SetBool(DID_MIGRATE_SIMULATION_SAVES_KEY, value); }
	}

	public static bool ShowMuscleContraction {
		get { return GetBool(SHOW_MUSCLE_CONTRACTION_KEY, false); }
		set { SetBool(SHOW_MUSCLE_CONTRACTION_KEY, value); }
	}

	public static bool ShowMuscles {
		get { return GetBool(SHOW_MUSCLES_KEY, true); }
		set { SetBool(SHOW_MUSCLES_KEY, value); }
	}

	public static bool ShowOneAtATime {
		get { return GetBool(SHOW_ONE_AT_ATIME_KEY, false); }
		set { SetBool(SHOW_ONE_AT_ATIME_KEY, value); }
	}

	public static float HiddenCreatureOpacity {
		get { return PlayerPrefs.GetFloat(HIDDEN_CREATURE_OPACITY_KEY, 0.225f); }
		set { PlayerPrefs.SetFloat(HIDDEN_CREATURE_OPACITY_KEY, value); Save(); }
	}

	public static bool DontShowV2SimulationDeprecationOverlayAgain {
		get { return GetBool(DONT_SHOW_V_2_SIMULATION_DEPRECATION_OVERLAY_AGAIN_KEY, false); }
		set { SetBool(DONT_SHOW_V_2_SIMULATION_DEPRECATION_OVERLAY_AGAIN_KEY, value); }
	}

	public static float GridSize {
		get { return PlayerPrefs.GetFloat(GRID_SIZE_KEY, 1.0f); }
		set { PlayerPrefs.SetFloat(GRID_SIZE_KEY, value); Save(); }
	}

	public static bool GridEnabled {
		get { return GetBool(GRID_ENABLED_KEY, false); }
		set { SetBool(GRID_ENABLED_KEY, value); }
	}

	public static string HelpScreenLanguage {
		get { return PlayerPrefs.GetString(HELP_SCREEN_LANGUAGE_KEY, "LANGUAGE_ENGLISH"); }
		set { PlayerPrefs.SetString(HELP_SCREEN_LANGUAGE_KEY, value); Save(); }
	}

	public static bool HelpIndicatorShown {
		get { return GetBool(HELP_INDICATOR_SHOWN_KEY, true); }
		set { SetBool(HELP_INDICATOR_SHOWN_KEY, value); }
	}

	public static int EditorMode {
		get { return PlayerPrefs.GetInt(EDITOR_MODE_KEY, 0); }
		set { PlayerPrefs.SetInt(EDITOR_MODE_KEY, value); Save(); }
	}

	public static string CreatureNames {
		get { return PlayerPrefs.GetString(CREATURE_NAMES_KEY, ""); }
		set { PlayerPrefs.SetString(CREATURE_NAMES_KEY, value); Save(); }
	}

	public static string SimulationSettings {
		get { return PlayerPrefs.GetString(SIMULATION_SETTINGS_KEY, ""); }
		set { PlayerPrefs.SetString(SIMULATION_SETTINGS_KEY, value); Save(); }
	}

	public static string NetworkSettings {
		get { return PlayerPrefs.GetString(NETWORK_SETTINGS_KEY, ""); }
		set { PlayerPrefs.SetString(NETWORK_SETTINGS_KEY, value); Save(); }
	}

	public static string EditorSettings {
		get { return PlayerPrefs.GetString(EDITOR_SETTINGS_KEY, ""); }
		set { PlayerPrefs.SetString(EDITOR_SETTINGS_KEY, value); Save(); }
	}

	public static string CurrentCreatureDesign {
		get { return PlayerPrefs.GetString(CURRENT_CREATURE_DESIGN_KEY, ""); }
		set { PlayerPrefs.SetString(CURRENT_CREATURE_DESIGN_KEY, value); Save(); }
	}

	static Settings() {
		if (GetBool("ALREADY_INITIALIZED_e31cf645-7751-4a7b-ae0d-2ca38f6063b8")) { return; }
		Initialize();
		SetBool("ALREADY_INITIALIZED_e31cf645-7751-4a7b-ae0d-2ca38f6063b8", true);
		Save();
	}

	private static void Initialize() {
		DidMigrateCreatureSaves = false;
		DidMigrateSimulationSaves = false;
		ShowMuscleContraction = false;
		ShowMuscles = true;
		ShowOneAtATime = false;
		HiddenCreatureOpacity = 0.225f;
		DontShowV2SimulationDeprecationOverlayAgain = false;
		GridSize = 1.0f;
		GridEnabled = false;
		HelpScreenLanguage = "LANGUAGE_ENGLISH";
		EditorMode = 0;
	}

	private static bool GetBool(string key, bool defaultValue = false) {
		return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
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
	}
}
