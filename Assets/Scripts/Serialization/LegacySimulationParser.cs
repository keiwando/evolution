using System;
using System.Text;
using System.Text.RegularExpressions;
using Keiwando.Evolution;

/// <summary>
/// The legacy Evolution simulation save files have the following format (VERSION 2):
/// 
/// Content: 
/// 
/// v save format version (v 2)
/// -separator-
/// Encoded Evolution Settings
/// -separator-
/// Encoded Neural Network Settings
/// -separator-
/// CreatureSaveData - created by the CreatureSaver class
/// -separator-
/// A list of the best chromosomes (for each chromosome:  chromosome : (CreatureStats encoded))
/// -separator-
/// A list of the current chromosomes (The chromosomes of all creatures of the last simulating generation)
/// </summary>
public class LegacySimulationLoader {

    public class SplitOptions {

		private string COMPONENT_SEPARATOR_BASE = "--?%%%?--";
		public string COMPONENT_SEPARATOR {
			get { return COMPONENT_SEPARATOR_BASE + NEWLINE; }
		}

		public string[] SPLIT_ARRAY;
		public string[] NEWLINE_SPLIT;
		public string NEWLINE {
			get { return newline; }
		} 
		private string newline = Environment.NewLine;

		public SplitOptions(){
			
			SPLIT_ARRAY = new string[] { COMPONENT_SEPARATOR };
			NEWLINE_SPLIT = new string[] { NEWLINE };
		}

		public SplitOptions(string newline) {

			this.newline = newline;
			this.SPLIT_ARRAY = new string[] { COMPONENT_SEPARATOR };
			this.NEWLINE_SPLIT = new string[] { NEWLINE };
		}
	}

    /// <summary>
	/// The current save file format version. This number has nothing to do with the Application.version.
	/// It should be the first line of every savefile prepended by a v so that it can be immediately identified how to interpret
	/// the rest of the file.
	/// The first savefiles do not contain a version number (1), but instead immediately start with the simulation objective as an int, 
	/// which is how they can be differentiated from the rest (They don't start with a v).
	/// </summary>
	private static int version = 2;


	public static SimulationData ParseSimulationData(string filename, string contents) {

		var lineEndings = contents.Contains("\r\n") ? "\r\n" : "\n";
		var splitOptions = new SplitOptions(lineEndings);

		var components = contents.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		// determine the version of the save file.
		// if the first line doesn't start with a v the version is 1

		if (components[0].ToUpper()[0] != 'V') {
			// V1
			return SimulationParserV1.ParseSimulationData(filename, contents, splitOptions);
		}

		var version = int.Parse(components[0].Split(' ')[1]);

		switch (version) {
			case 2: 
				return SimulationParserV2.ParseSimulationData(filename, contents, splitOptions);
			default: throw new System.Exception("Unknown Save file format!");
		}
	}
}