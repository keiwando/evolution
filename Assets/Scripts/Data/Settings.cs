using UnityEngine;
using System.Collections.Generic;

public class Settings {

	private const string DID_MIGRATE_CREATURE_SAVES_KEY = "DID_MIGRATE_CREATURE_SAVES_KEY";
	private const string DID_MIGRATE_SIMULATION_SAVES_KEY = "DID_MIGRATE_SIMULATION_SAVES_KEY";
	private const string SHOW_MUSCLE_CONTRACTION_KEY = "showMuscleContraction";
	private const string SHOW_MUSCLES_KEY = "SHOW_MUSCLES_KEY";
	private const string SHOW_ONE_AT_ATIME_KEY = "SHOW_ONE_AT_ATIME_KEY";
	private const string HIDDEN_CREATURE_OPACITY_KEY = "HIDDEN_CREATURE_OPACITY_KEY";
	private const string DEFAULT_GRID_VISIBILITY_KEY = "DEFAULT_GRID_VISIBILITY_KEY";
	private const string FLYING_GRID_VISIBILITY_KEY = "FLYING_GRID_VISIBILITY_KEY";
	private const string DONT_SHOW_EXIT_CONFIRMATION_OVERLAY_AGAIN_KEY = "DONT_SHOW_EXIT_CONFIRMATION_OVERLAY_AGAIN_KEY";
	private const string GRID_SIZE_KEY = "GRID_SIZE";
	private const string GRID_ENABLED_KEY = "GRID_ENABLED";
	private const string AUTO_SAVE_ENABLED_KEY = "AUTO_SAVE_ENABLED_KEY";
	private const string AUTO_SAVE_DISTANCE_KEY = "AUTO_SAVE_DISTANCE_KEY";
	private const string HELP_SCREEN_LANGUAGE_KEY = "HELP_SCREEN_LANGUAGE";
	private const string LANGUAGE_KEY = "LANGUAGE_KEY";
	private const string SHOW_ONBOARDING_KEY = "SHOW_ONBOARDING_KEY";
	private const string CREATURE_NAMES_KEY = "_CreatureNames";
	private const string SIMULATION_SETTINGS_KEY = "EVOLUTION_SETTINGS";
	private const string NETWORK_SETTINGS_KEY = "NEURAL NETWORK SETTINGS";
	private const string EDITOR_SETTINGS_KEY = "EDITOR_SETTINGS_KEY";
	private const string LAST_CREATURE_DESIGN_KEY = "LAST_CREATURE_DESIGN_KEY";

	public static bool DidMigrateCreatureSaves {
		get { return Store.GetBool(DID_MIGRATE_CREATURE_SAVES_KEY, false); }
		set { Store.SetBool(DID_MIGRATE_CREATURE_SAVES_KEY, value); }
	}

	public static bool DidMigrateSimulationSaves {
		get { return Store.GetBool(DID_MIGRATE_SIMULATION_SAVES_KEY, false); }
		set { Store.SetBool(DID_MIGRATE_SIMULATION_SAVES_KEY, value); }
	}

	public static bool ShowMuscleContraction {
		get { return Store.GetBool(SHOW_MUSCLE_CONTRACTION_KEY, false); }
		set { Store.SetBool(SHOW_MUSCLE_CONTRACTION_KEY, value); }
	}

	public static bool ShowMuscles {
		get { return Store.GetBool(SHOW_MUSCLES_KEY, true); }
		set { Store.SetBool(SHOW_MUSCLES_KEY, value); }
	}

	public static bool ShowOneAtATime {
		get { return Store.GetBool(SHOW_ONE_AT_ATIME_KEY, false); }
		set { Store.SetBool(SHOW_ONE_AT_ATIME_KEY, value); }
	}

	public static float HiddenCreatureOpacity {
		get { return Store.GetFloat(HIDDEN_CREATURE_OPACITY_KEY, 0.225f); }
		set { Store.SetFloat(HIDDEN_CREATURE_OPACITY_KEY, value); }
	}

	public static float DefaultGridVisibility {
		get { return Store.GetFloat(DEFAULT_GRID_VISIBILITY_KEY, 0.0f); }
		set { Store.SetFloat(DEFAULT_GRID_VISIBILITY_KEY, value); }
	}

	public static float FlyingGridVisibility {
		get { return Store.GetFloat(FLYING_GRID_VISIBILITY_KEY, 0.5f); }
		set { Store.SetFloat(FLYING_GRID_VISIBILITY_KEY, value); }
	}

	public static bool DontShowExitConfirmationOverlayAgain {
		get { return Store.GetBool(DONT_SHOW_EXIT_CONFIRMATION_OVERLAY_AGAIN_KEY, false); }
		set { Store.SetBool(DONT_SHOW_EXIT_CONFIRMATION_OVERLAY_AGAIN_KEY, value); }
	}

	public static float GridSize {
		get { return Store.GetFloat(GRID_SIZE_KEY, 1.0f); }
		set { Store.SetFloat(GRID_SIZE_KEY, value); }
	}

	public static bool GridEnabled {
		get { return Store.GetBool(GRID_ENABLED_KEY, false); }
		set { Store.SetBool(GRID_ENABLED_KEY, value); }
	}

	public static bool AutoSaveEnabled {
		get { return Store.GetBool(AUTO_SAVE_ENABLED_KEY, false); }
		set { Store.SetBool(AUTO_SAVE_ENABLED_KEY, value); }
	}

	public static int AutoSaveDistance {
		get { return Store.GetInt(AUTO_SAVE_DISTANCE_KEY, 5); }
		set { Store.SetInt(AUTO_SAVE_DISTANCE_KEY, value); }
	}

	public static string HelpScreenLanguage {
		get { return Store.GetString(HELP_SCREEN_LANGUAGE_KEY, "LANGUAGE_ENGLISH"); }
		set { Store.SetString(HELP_SCREEN_LANGUAGE_KEY, value); }
	}

	public static string Language {
		get { return Store.GetString(LANGUAGE_KEY, "en"); }
		set { Store.SetString(LANGUAGE_KEY, value); }
	}

	public static bool ShowOnboarding {
		get { return Store.GetBool(SHOW_ONBOARDING_KEY, true); }
		set { Store.SetBool(SHOW_ONBOARDING_KEY, value); }
	}

	public static string CreatureNames {
		get { return Store.GetString(CREATURE_NAMES_KEY, ""); }
		set { Store.SetString(CREATURE_NAMES_KEY, value); }
	}

	public static string SimulationSettings {
		get { return Store.GetString(SIMULATION_SETTINGS_KEY, ""); }
		set { Store.SetString(SIMULATION_SETTINGS_KEY, value); }
	}

	public static string NetworkSettings {
		get { return Store.GetString(NETWORK_SETTINGS_KEY, ""); }
		set { Store.SetString(NETWORK_SETTINGS_KEY, value); }
	}

	public static string EditorSettings {
		get { return Store.GetString(EDITOR_SETTINGS_KEY, ""); }
		set { Store.SetString(EDITOR_SETTINGS_KEY, value); }
	}

	public static string LastCreatureDesign {
		get { return Store.GetString(LAST_CREATURE_DESIGN_KEY, ""); }
		set { Store.SetString(LAST_CREATURE_DESIGN_KEY, value); }
	}

	static Settings() {
		if (Store.GetBool("ALREADY_INITIALIZED_e31cf645-7751-4a7b-ae0d-2ca38f6063b8")) { return; }
		Initialize();
		Store.SetBool("ALREADY_INITIALIZED_e31cf645-7751-4a7b-ae0d-2ca38f6063b8", true);
	}

	private static void Initialize() {
		DidMigrateCreatureSaves = false;
		DidMigrateSimulationSaves = false;
		ShowMuscleContraction = false;
		ShowMuscles = true;
		ShowOneAtATime = false;
		HiddenCreatureOpacity = 0.225f;
		DefaultGridVisibility = 0.0f;
		FlyingGridVisibility = 0.5f;
		DontShowExitConfirmationOverlayAgain = false;
		GridSize = 1.0f;
		GridEnabled = false;
		AutoSaveEnabled = false;
		AutoSaveDistance = 5;
		HelpScreenLanguage = "LANGUAGE_ENGLISH";
		Language = "en";
		ShowOnboarding = true;
	}

 public static void Reset() {
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
		string GetString(string key, string defaultValue = "");
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

		public string GetString(string key, string defaultValue = "") {
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

		public string GetString(string key, string defaultValue = "") {
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
	
}
