using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSaver {
	
	public bool Enabled {
		set { this.enabled = value; }
		get { return this.enabled; }
	}
	private bool enabled = false;

	/// <summary>
	/// The distance between two autosaves in generations.
	/// </summary>
	public int GenerationDistance = 10;

	//private int lastSavedGeneration = -100;

	private string lastSaveFileName = "";

	public bool Update(int generation, Evolution evolution) {

		if (!enabled || generation % GenerationDistance != 0 || generation < 2) { 
			return false; 
		}

		// Perform an auto-save
		Save(generation, evolution);

		return true;
	}

	private void Save(int generation, Evolution evolution) {

		var lastSave = this.lastSaveFileName;

		//this.lastSavedGeneration = generation;
		this.lastSaveFileName = evolution.SaveSimulation();

		// Delete the last auto-saved file
		if (lastSave != "" && lastSave.EndsWith(".txt")) {
			EvolutionSaver.DeleteSaveFile(lastSave);
		}
	}
}
