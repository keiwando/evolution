using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public struct SimulationSettings {
	
	/// <summary>
	/// Specifies whether or not the best two creatures of every generation are copied into the next generation.
	/// </summary>
	public bool KeepBestCreatures;

	/// <summary>
	/// The simulation time per batch.
	/// </summary>
	public int SimulationTime;

	/// <summary>
	/// The number of creatures per generation.
	/// </summary>
	public int PopulationSize;

	/// <summary>
	/// Specifies whether a generation should be simulated in batches.
	/// </summary>
	public bool SimulateInBatches;

	/// <summary>
	/// The number of creatures to simulate at once. Has to be less than
	/// the populationSize.
	/// </summary>
	public int BatchSize;

	/// <summary>
	/// The task that the creatures need to perform.
	/// </summary>
	public EvolutionTask Task;

	/// <summary>
	/// Specifies the probability of chromosome mutation as a percentage between 1 and 100.
	/// </summary>
	public int MutationRate;

	public static SimulationSettings Default = new SimulationSettings() {
		Task = EvolutionTask.Running,
		KeepBestCreatures = true,
		SimulationTime = 10,
		PopulationSize = 10,
		BatchSize = 10,
		SimulateInBatches = false,
		MutationRate = 50
	};

	public SimulationSettings(EvolutionTask task) {

		this.Task = task;
		this.KeepBestCreatures = true;
		this.SimulationTime = 10;
		this.PopulationSize = 10;
		this.SimulateInBatches = false;
		this.BatchSize = 10;
		this.MutationRate = 50;
	}

	// #region Encode & Decode

    //     private static class CodingKey {
    //         public const string GridEnabled = "gridEnabled";
    //         public const string GridSize = "gridSize";
    //     }

    //     public string Encode() {
    //         var json = new JObject();

    //         json[CodingKey.GridEnabled] = this.GridEnabled;
    //         json[CodingKey.GridSize] = this.GridSize;
    //         return json.ToString();
    //     }

    //     public static EditorSettings Decode(string encoded) {

    //         JObject json = JObject.Parse(encoded);

    //         bool gridEnabled = json[CodingKey.GridEnabled].ToObject<bool>();
    //         float gridSize = json[CodingKey.GridSize].ToObject<float>();

    //         return new EditorSettings() {
    //             GridEnabled = gridEnabled,
    //             GridSize = gridSize
    //         };
    //     }

    //     #endregion

	#region Encode & Decode

	/// <summary>
	/// Encodes this instance into a string decodable by the Decode function.
	/// </summary>
	// public string Encode() {
	// 	// Format:
	// 	// keepBestCreatures # simulationTime # populationSize # simulateInBatches # batchSize # task # mutationRate
	// 	// ^ without spaces. enum value as string

	// 	return "((#" + keepBestCreatures.ToString() + "#" + simulationTime.ToString() + "#" + populationSize.ToString() + "#" + simulateInBatches.ToString() + "#"
	// 		+ batchSize.ToString() + "#" + task.StringRepresentation() + "#" + mutationRate.ToString() + "#))";

	// }

	public string Encode() {
		return JsonUtility.ToJson(this, false);
	}

	public static SimulationSettings Decode(string encoded) {

		if (string.IsNullOrEmpty(encoded))
			return Default;
		if (encoded.StartsWith("{"))
			return (SimulationSettings)JsonUtility.FromJson(encoded, typeof(SimulationSettings));
		
		return DecodeV1(encoded);
	}

	private static SimulationSettings DecodeV1(string encoded) {
		var parts = encoded.Split('#');
		var settings = new SimulationSettings();

		settings.KeepBestCreatures = bool.Parse(parts[1]);
		settings.SimulationTime = int.Parse(parts[2]);
		settings.PopulationSize = int.Parse(parts[3]);
		settings.SimulateInBatches = bool.Parse(parts[4]);
		settings.BatchSize = int.Parse(parts[5]);
		settings.Task = EvolutionTaskUtil.TaskFromString(parts[6]);
		settings.MutationRate = int.Parse(parts[7]);

		return settings;
	}
	
	private static SimulationSettings DecodeV2(string encoded) {
		return (SimulationSettings)JsonUtility.FromJson(encoded, typeof(SimulationSettings));
	}

	#endregion
}
