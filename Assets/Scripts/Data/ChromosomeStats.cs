using System;
using UnityEngine;

public struct ChromosomeStats {

	public string chromosome {get; private set; }
	public CreatureStats stats { get; private set; }

	public ChromosomeStats(string chromosome, CreatureStats stats) {
		this.chromosome = chromosome;
		this.stats = stats;
	}

	public override string ToString ()
	{
		return string.Format("{0}:{1}", chromosome, stats.Encode());
	}

	public static ChromosomeStats FromString(string str) {

		var parts = str.Split(':');

		return new ChromosomeStats(parts[0], CreatureStats.Decode(parts[1]));
	}
}


