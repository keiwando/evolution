using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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



	public string Encode() {
		// Format: (# fitness # simulationTime # horizontalDistanceTravelled # verticalDistanceTravelled # maxJumpingHeight # weight # numberOfBones # numberOfMuscles # averageSpeed #)
		// (without spaces)

		return string.Format("(#{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#)",
			fitness.ToString(), simulationTime.ToString(), horizontalDistanceTravelled.ToString(), verticalDistanceTravelled.ToString(), 
			maxJumpingHeight.ToString(), weight.ToString(), numberOfBones.ToString(), numberOfMuscles.ToString(), averageSpeed.ToString()
		);
	}

	public static CreatureStats Decode(string encoded) {

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
}
