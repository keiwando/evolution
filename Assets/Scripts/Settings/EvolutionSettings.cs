using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionSettings {

	/// <summary>
	/// Specifies whether or not the best two creatures of every generation are copied into the next generation.
	/// </summary>
	public bool keepBestCreatures = true;

	/// <summary>
	/// The simulation time per generation.
	/// </summary>
	public int simulationTime = 10;

	/// <summary>
	/// The number of creatures per generation.
	/// </summary>
	public int populationSize = 10;

	/// <summary>
	/// Specifies whether a generation should be simulated in batches.
	/// </summary>
	public bool simulateInBatches = false;

	/// <summary>
	/// The number of creatures to simulate at once. Has to be less than
	/// the populationSize.
	/// </summary>
	public int batchSize = 10;

	/// <summary>
	/// The task that the creatures need to perform.
	/// </summary>
	public Evolution.Task task = Evolution.Task.RUNNING;

	/// <summary>
	/// Specifies the probability of chromosome mutation as a percentage between 1 and 100.
	/// </summary>
	public int mutationRate = 50;

	/// <summary>
	/// Encodes this instance into a string decodable by the Decode function.
	/// </summary>
	public string Encode() {
		// Format:
		// keepBestCreatures # simulationTime # populationSize # simulateInBatches # batchSize # task # mutationRate
		// ^ without spaces. enum value as string

		return "((#" + keepBestCreatures.ToString() + "#" + simulationTime.ToString() + "#" + populationSize.ToString() + "#" + simulateInBatches.ToString() + "#"
			+ batchSize.ToString() + "#" + Evolution.TaskToString(task) + "#" + mutationRate.ToString() + "#))";

	}

	public static EvolutionSettings Decode(string encoded) {

		var parts = encoded.Split('#');
		var settings = new EvolutionSettings();

		settings.keepBestCreatures = bool.Parse(parts[1]);
		settings.simulationTime = int.Parse(parts[2]);
		settings.populationSize = int.Parse(parts[3]);
		settings.simulateInBatches = bool.Parse(parts[4]);
		settings.batchSize = int.Parse(parts[5]);
		settings.task = Evolution.TaskFromString(parts[6]);
		settings.mutationRate = int.Parse(parts[7]);

		return settings;
	}
}
