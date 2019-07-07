using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CreatureStats {

	/// <summary>
	/// The fitness this creature reached when during the simulation.
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

	private static class CodingKey {
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
		
		return new CreatureStats() {
			fitness = json[CodingKey.Fitness].ToObject<float>(),
			simulationTime = json[CodingKey.SimulationTime].ToObject<int>(),
			horizontalDistanceTravelled = json[CodingKey.HorizontalDistance].ToObject<float>(),
			verticalDistanceTravelled = json[CodingKey.VerticalDistance].ToObject<float>(),
			maxJumpingHeight = json[CodingKey.MaxJumpHeight].ToObject<float>(),
			weight = json[CodingKey.Weight].ToObject<float>(),
			numberOfBones = json[CodingKey.NumberOfBones].ToObject<int>(),
			numberOfMuscles = json[CodingKey.NumberOfMuscles].ToObject<int>(),
			averageSpeed = json[CodingKey.AverageSpeed].ToObject<float>()
		};
	}

	public static CreatureStats DecodeV1(string encoded) {

		var stats = new CreatureStats();
		var parts = encoded.Split('#');

		stats.fitness = float.Parse(parts[1]);
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
