using Keiwando.JSON;
using System;
using System.IO;

public class CreatureStats {

	/// <summary>
	/// The unclamped fitness score of this creature as was assigned by the objective tracker.
	/// </summary>
	public float unclampedFitness = 0f;

	/// <summary>
	/// `unclampedFitness` clamped to the range [0-1].
	/// </summary>
	public float fitness = 0f;

	/// <summary>
	/// The amount of time this creature had in order to "show" its behaviour. In seconds.
	/// </summary>
	public int simulationTime = -1;

	/// <summary>
	/// The horizontal distance of the creature from its starting point when its simulation time was up. In Unity3D world space units (meters)
	/// </summary>
	public float horizontalDistanceTravelled = 0f;

	/// <summary>
	/// The vertical distance of the creature from its starting point when its simulation time was up. In Unity3D world space units (meters)
	/// </summary>
	public float verticalDistanceTravelled = 0f;

	/// <summary>
	/// The maximum distance from the ground during the simulation. In meters.
	/// </summary>
	public float maxJumpingHeight = 0f;

	/// <summary>
	/// The weight of this creature.
	/// </summary>
	public float weight = 0f;

	public int numberOfBones = 0;

	public int numberOfMuscles = 0;

	public float averageSpeed = 0f;

	#region Encode & Decode

	public void Encode(BinaryWriter writer) {
		long dataLengthOffset = writer.Seek(0, SeekOrigin.Current);
		writer.WriteDummyBlockLength();
		ushort flags = 0;
		writer.Write(flags);

		writer.Write(unclampedFitness);
		writer.Write(fitness);
		writer.Write(simulationTime);
		writer.Write(horizontalDistanceTravelled);
		writer.Write(verticalDistanceTravelled);
		writer.Write(maxJumpingHeight);
		writer.Write(weight);
		writer.Write(numberOfBones);
		writer.Write(numberOfMuscles);
		writer.Write(averageSpeed);

		writer.WriteBlockLengthToOffset(dataLengthOffset);
	}

	public static CreatureStats Decode(BinaryReader reader) {

		uint dataLength = reader.ReadBlockLength();
		long expectedEndByte = reader.BaseStream.Position + (long)dataLength;

		ushort flags = reader.ReadUInt16();

		float unclampedFitness = reader.ReadSingle();
		float fitness = reader.ReadSingle();
		int simulationTime = reader.ReadInt32();
		float horizontalDistanceTravelled = reader.ReadSingle();
		float verticalDistanceTravelled = reader.ReadSingle();
		float maxJumpingHeight = reader.ReadSingle();
		float weight = reader.ReadSingle();
		int numberOfBones = reader.ReadInt32();
		int numberOfMuscles = reader.ReadInt32();
		float averageSpeed = reader.ReadSingle();

		reader.BaseStream.Seek(expectedEndByte, SeekOrigin.Begin);

		return new CreatureStats() {
			unclampedFitness = unclampedFitness,
			fitness = fitness,
			simulationTime = simulationTime,
			horizontalDistanceTravelled = horizontalDistanceTravelled,
			verticalDistanceTravelled = verticalDistanceTravelled,
			maxJumpingHeight = maxJumpingHeight,
			weight = weight,
			numberOfBones = numberOfBones,
			numberOfMuscles = numberOfMuscles,
			averageSpeed = averageSpeed
		};
	}

	public static void Skip(BinaryReader reader) {
		uint dataLength = reader.ReadBlockLength();
		long expectedEndByte = reader.BaseStream.Position + (long)dataLength;
		reader.BaseStream.Seek(expectedEndByte, SeekOrigin.Begin);
	}

	private static float ReadFloatAtOffsetAndAdvanceToEnd(BinaryReader reader, int offset) {
		uint dataLength = reader.ReadBlockLength();
		long expectedEndByte = reader.BaseStream.Position + (long)dataLength;
		reader.BaseStream.Seek(offset, SeekOrigin.Current);
		float value = reader.ReadSingle();
		reader.BaseStream.Seek(expectedEndByte, SeekOrigin.Begin);
		return value;
	}

	public static float DecodeHorizontalDistanceTravelledAndAdvanceToEnd(BinaryReader reader) {
		return ReadFloatAtOffsetAndAdvanceToEnd(reader, 14);
	}

	public static float DecodeVerticalDistanceTravelledAndAdvanceToEnd(BinaryReader reader) {
		return ReadFloatAtOffsetAndAdvanceToEnd(reader, 18);
	}

	public static float DecodeMaxJumpingHeightAndAdvanceToEnd(BinaryReader reader) {
		return ReadFloatAtOffsetAndAdvanceToEnd(reader, 22);
	}

	private static class CodingKey {
		public const string UnclampedFitness = "unclampedFitness";
		public const string Fitness = "fitness";
		public const string SimulationTime = "simulationTime";
		public const string HorizontalDistance = "horizontalDistanceTravelled";
		public const string VerticalDistance = "verticalDistanceTravelled";
		public const string MaxJumpHeight = "maxJumpHeight";
		public const string Weight = "weight";
		public const string NumberOfBones = "numberOfBones";
		public const string NumberOfMuscles = "numberOfMuscles";
		public const string AverageSpeed = "averageSpeed";
	}

	public JObject Encode() {

		JObject json = new JObject();
		json[CodingKey.UnclampedFitness] = this.unclampedFitness;
		json[CodingKey.Fitness] = this.fitness;
		json[CodingKey.SimulationTime] = this.simulationTime;
		json[CodingKey.HorizontalDistance] = this.horizontalDistanceTravelled;
		json[CodingKey.VerticalDistance] = this.verticalDistanceTravelled;
		json[CodingKey.MaxJumpHeight] = this.maxJumpingHeight;
		json[CodingKey.Weight] = this.weight;
		json[CodingKey.NumberOfBones] = this.numberOfBones;
		json[CodingKey.NumberOfMuscles] = this.numberOfMuscles;
		json[CodingKey.AverageSpeed] = this.averageSpeed;
		return json;
	}

	public static CreatureStats Decode(string encoded) {

		if (encoded.StartsWith("{"))
			return Decode(JObject.Parse(encoded));

		return DecodeV1(encoded);
	}

	public static CreatureStats Decode(JObject json) {
		
		var result = new CreatureStats() {
			fitness = json[CodingKey.Fitness].ToFloat(),
			simulationTime = json[CodingKey.SimulationTime].ToInt(),
			horizontalDistanceTravelled = json[CodingKey.HorizontalDistance].ToFloat(),
			verticalDistanceTravelled = json[CodingKey.VerticalDistance].ToFloat(),
			maxJumpingHeight = json[CodingKey.MaxJumpHeight].ToFloat(),
			weight = json[CodingKey.Weight].ToFloat(),
			numberOfBones = json[CodingKey.NumberOfBones].ToInt(),
			numberOfMuscles = json[CodingKey.NumberOfMuscles].ToInt(),
			averageSpeed = json[CodingKey.AverageSpeed].ToFloat()
		};
		if (json.ContainsKey(CodingKey.UnclampedFitness)) {
			result.unclampedFitness = json[CodingKey.UnclampedFitness].ToFloat();
		} else {
			result.unclampedFitness = result.fitness;
		}
		return result;
	}

	public static CreatureStats DecodeV1(string encoded) {

		var stats = new CreatureStats();
		var parts = encoded.Split('#');

		stats.fitness = float.Parse(parts[1]);
		stats.unclampedFitness = stats.fitness;
		stats.simulationTime = int.Parse(parts[2]);
		stats.horizontalDistanceTravelled = float.Parse(parts[3]);
		stats.verticalDistanceTravelled = float.Parse(parts[4]);
		stats.maxJumpingHeight = float.Parse(parts[5]);
		stats.weight = float.Parse(parts[6]);
		stats.numberOfBones = int.Parse(parts[7]);
		stats.numberOfMuscles = int.Parse(parts[8]);
		stats.averageSpeed = float.Parse(parts[9]);

		return stats;
	}

	#endregion
}
