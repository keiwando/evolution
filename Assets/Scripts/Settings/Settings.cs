using UnityEngine;

public class Settings {

	private const string DID_MIGRATE_CREATURE_SAVES_KEY = "DID_MIGRATE_CREATURE_SAVES_KEY";
	private const string CREATURE_NAMES_KEY = "_CreatureNames";
	private const string CURRENT_CREATURE_NAME_KEY = "CURRENT_CREATURE_NAME_KEY";
	private const string CURRENT_CREATURE_DESIGN_KEY = "CURRENT_CREATURE_DESIGN_KEY";

	public static bool DidMigrateCreatureSaves {
		get { return GetBool(DID_MIGRATE_CREATURE_SAVES_KEY); }
		set { SetBool(DID_MIGRATE_CREATURE_SAVES_KEY, value); }
	}

	public static string CreatureNames {
		get { return PlayerPrefs.GetString(CREATURE_NAMES_KEY, ""); }
		set { PlayerPrefs.SetString(CREATURE_NAMES_KEY, value); Save(); }
	}

	public static string CurrentCreatureName {
		get { return PlayerPrefs.GetString(CURRENT_CREATURE_NAME_KEY, ""); }
		set { PlayerPrefs.SetString(CURRENT_CREATURE_NAME_KEY, value); Save(); }
	}

	public static string CurrentCreatureDesign {
		get { return PlayerPrefs.GetString(CURRENT_CREATURE_DESIGN_KEY, ""); }
		set { PlayerPrefs.SetString(CURRENT_CREATURE_DESIGN_KEY, value); Save(); }
	}

	static Settings() {
		if (GetBool("ALREADY_INITIALIZED_9e36ea4d-85a0-46b2-a7de-bbfd96ffb0cb")) { return; }
		Initialize();
		SetBool("ALREADY_INITIALIZED_9e36ea4d-85a0-46b2-a7de-bbfd96ffb0cb", true);
		Save();
	}

	private static void Initialize() {
		DidMigrateCreatureSaves = false;
	}

	private static bool GetBool(string key) {
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
	}
}
