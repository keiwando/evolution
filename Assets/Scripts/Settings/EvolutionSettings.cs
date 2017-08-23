using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionSettings {

	/// <summary>
	/// Specifies whether or not the best creature of every generation is copied into the next generation.
	/// </summary>
	public bool keepBestCreatures;

	/// <summary>
	/// The simulation time per generation.
	/// </summary>
	public int simulationTime;

	/// <summary>
	/// The number of creatures per generation.
	/// </summary>
	public int populationSize;

	/// <summary>
	/// The task that the creatures need to perform.
	/// </summary>
	public Evolution.Task task;



}
