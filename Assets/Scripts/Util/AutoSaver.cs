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

		private string lastSaveFileName = "";

		public bool Update(int generation, Evolution evolution) {

			if (!Enabled || generation % GenerationDistance != 0 || generation < 2) { 
				return false; 
			}

			// Perform an auto-save
			Save(generation, evolution);

			return true;
		}

		private void Save(int generation, Evolution evolution) {

			var lastSave = this.lastSaveFileName;

			this.lastSaveFileName = evolution.SaveSimulation();

			// Delete the last auto-saved file
			if (lastSave != "") {
				SimulationSerializer.DeleteSaveFile(lastSave);
			}
		}
	}
}
