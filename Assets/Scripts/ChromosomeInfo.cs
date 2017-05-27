using System;
using UnityEngine;

public struct ChromosomeInfo
{
	public string chromosome {get; private set; }
	public float fitness {get; private set; }

	public ChromosomeInfo(string chromosome, float fitness) {
		this.chromosome = chromosome;
		this.fitness = fitness;
	}

	public override string ToString ()
	{
		return string.Format("{0}:{1}", chromosome, fitness.ToString());
	}

	public static ChromosomeInfo FromString(string str) {

		var parts = str.Split(':');

		return new ChromosomeInfo(parts[0], float.Parse(parts[1]));
	}
}

