using System;
using Keiwando.Evolution;
using Keiwando.JSON;

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
	/// The objective that the creatures need to perform.
	/// </summary>
	public Objective Objective;

	/// <summary>
	/// Specifies the probability of chromosome mutation as a percentage between 1 and 100.
	/// </summary>
	public float MutationRate;

	/// <summary>
	/// The algorithm to use when selecting for survival and reproduction
	/// </summary>
	public SelectionAlgorithm SelectionAlgorithm;

	/// <summary>
	/// The algorithm to use during recombination
	/// </summary>
	public RecombinationAlgorithm RecombinationAlgorithm;

	/// <summary>
	/// The algorithm to use when mutating chromosomes
	/// </summary>
	public MutationAlgorithm MutationAlgorithm;


	public static readonly SimulationSettings Default = new SimulationSettings() {
		Objective = Objective.Running,
		KeepBestCreatures = true,
		SimulationTime = 10,
		PopulationSize = 10,
		BatchSize = 10,
		SimulateInBatches = false,
		MutationRate = 0.5f,
		SelectionAlgorithm = SelectionAlgorithm.RankProportional,
		RecombinationAlgorithm = RecombinationAlgorithm.OnePointCrossover,
		MutationAlgorithm = MutationAlgorithm.Global
	};

	public SimulationSettings(Objective objective) {

		this.Objective = objective;
		this.KeepBestCreatures = Default.KeepBestCreatures;
		this.SimulationTime = Default.SimulationTime;
		this.PopulationSize = Default.PopulationSize;
		this.SimulateInBatches = Default.SimulateInBatches;
		this.BatchSize = Default.BatchSize;
		this.MutationRate = Default.MutationRate;
		this.SelectionAlgorithm = Default.SelectionAlgorithm;
		this.RecombinationAlgorithm = Default.RecombinationAlgorithm;
		this.MutationAlgorithm = Default.MutationAlgorithm;
	}

	#region Encode & Decode

	private static class CodingKey {
		public const string KeepBestCreatures = "keepBestCreatures";
		public const string SimulationTime = "simulationTime";
		public const string PopulationSize = "populationSize";
		public const string SimulateInBatches = "simulateInBatches";
		public const string BatchSize = "batchSize";
		public const string Objective = "objective";
		public const string MutationRate = "mutationRate";
		public const string SelectionAlgorithm = "selectionAlgorithm";
		public const string RecombinationAlgorithm = "recombinationAlgorithm";
		public const string MutationAlgorithm = "mutationAlgorithm";
	}

	public JObject Encode() {
		var json = new JObject();

		json[CodingKey.KeepBestCreatures] = this.KeepBestCreatures;
		json[CodingKey.SimulationTime] = this.SimulationTime;
		json[CodingKey.PopulationSize] = this.PopulationSize;
		json[CodingKey.SimulateInBatches] = this.SimulateInBatches;
		json[CodingKey.BatchSize] = this.BatchSize;
		json[CodingKey.Objective] = (int)this.Objective;
		json[CodingKey.MutationRate] = this.MutationRate;
		json[CodingKey.SelectionAlgorithm] = (int)this.SelectionAlgorithm;
		json[CodingKey.RecombinationAlgorithm] = (int)this.RecombinationAlgorithm;
		json[CodingKey.MutationAlgorithm] = (int)this.MutationAlgorithm;

		return json;
	}

	public static SimulationSettings Decode(string encoded) {

		if (string.IsNullOrEmpty(encoded))
			return Default;
		if (encoded.StartsWith("{"))
			return Decode(JObject.Parse(encoded));
		
		return DecodeV1(encoded);
	}

	public static SimulationSettings Decode(JObject json) {

		var settings = Default;

		settings.KeepBestCreatures = json[CodingKey.KeepBestCreatures].ToBool();
		settings.SimulationTime = json[CodingKey.SimulationTime].ToInt();
		settings.PopulationSize = json[CodingKey.PopulationSize].ToInt();
		settings.SimulateInBatches = json[CodingKey.SimulateInBatches].ToBool();
		settings.BatchSize = json[CodingKey.BatchSize].ToInt();
		settings.Objective = (Objective)json[json.ContainsKey("task") ? "task" : CodingKey.Objective].ToInt();
		settings.MutationRate = json[CodingKey.MutationRate].ToFloat();
		settings.SelectionAlgorithm = (SelectionAlgorithm)json[CodingKey.SelectionAlgorithm].ToInt();
		settings.RecombinationAlgorithm = (RecombinationAlgorithm)json[CodingKey.RecombinationAlgorithm].ToInt();
		settings.MutationAlgorithm = (MutationAlgorithm)json[CodingKey.MutationAlgorithm].ToInt();

		return settings;
	}

	private static SimulationSettings DecodeV1(string encoded) {
		var parts = encoded.Split('#');
		var settings = Default;

		settings.KeepBestCreatures = bool.Parse(parts[1]);
		settings.SimulationTime = int.Parse(parts[2]);
		settings.PopulationSize = int.Parse(parts[3]);
		settings.SimulateInBatches = bool.Parse(parts[4]);
		settings.BatchSize = int.Parse(parts[5]);
		settings.Objective = ObjectiveUtil.ObjectiveFromString(parts[6]);
		settings.MutationRate = Math.Min(Math.Max(((float)int.Parse(parts[7])) / 100f, 0), 1); 

		return settings;
	}

	#endregion
}
