using System.IO;
using UnityEngine;

namespace Keiwando.Evolution {
	
	public class AutoSaver {
	
		public bool Enabled {
			get => Settings.AutoSaveEnabled;
			set => Settings.AutoSaveEnabled = value;
		}

		/// <summary>
		/// The distance between two autosaves in generations.
		/// </summary>
		public int GenerationDistance {
			get => Settings.AutoSaveDistance;
			set => Settings.AutoSaveDistance = value;
		}

		public bool Update(int generation, Evolution evolution) {

			if (!Enabled || generation % GenerationDistance != 0 || generation < 2) { 
				return false; 
			}

			// Perform an auto-save
			evolution.SaveSimulation();

			return true;
		}
	}
}
