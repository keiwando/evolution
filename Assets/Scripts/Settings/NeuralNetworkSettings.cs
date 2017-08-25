using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeuralNetworkSettings {

	public static readonly int MAX_LAYERS = 10;
	public static readonly int MAX_NODES_PER_LAYER = 100; 

	public int numberOfIntermediateLayers { get { return nodesPerIntermediateLayer.Length; } }

	public int[] nodesPerIntermediateLayer;

	/// <summary>
	/// String format: 
	/// number of nodes per intermediate layer separated by a #
	/// </summary>
	public string Encode() {

		var nodesPerLayerAsStrings = nodesPerIntermediateLayer.Select(delegate(int arg) {
			return arg.ToString();
		}).ToArray();

		return string.Join("#", nodesPerLayerAsStrings);
	}

	public static NeuralNetworkSettings Decode(string str) {

		var numsAsStrings = str.Split('#');

		var nodesPIL = numsAsStrings.Select(delegate(string arg) {
			return int.Parse(arg);	
		}).ToArray();

		var settings = new NeuralNetworkSettings();
		settings.nodesPerIntermediateLayer = nodesPIL;

		return settings;
	}

	public static NeuralNetworkSettings GetDefaultSettings() {

		var nodesPIL = new int[]{ 10 };

		var settings = new NeuralNetworkSettings();
		settings.nodesPerIntermediateLayer = nodesPIL;

		return settings;
	}
}
