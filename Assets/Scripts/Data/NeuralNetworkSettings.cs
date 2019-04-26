using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[SerializeField]
public struct NeuralNetworkSettings {

	public static readonly int MAX_LAYERS = 10;
	public static readonly int MAX_NODES_PER_LAYER = 100;
	
	public static NeuralNetworkSettings Default = new NeuralNetworkSettings(new int[] { 10 });

	public int NumberOfIntermediateLayers {
		get { return NodesPerIntermediateLayer.Length; }
	}

	public int[] NodesPerIntermediateLayer;

	public NeuralNetworkSettings(int[] nodesPerIntermediateLayer) {
		this.NodesPerIntermediateLayer = nodesPerIntermediateLayer;
	}

	#region Encode & Decode

	public string Encode() {
		return JsonUtility.ToJson(this, false);
	}

	public static NeuralNetworkSettings Decode(string encoded) {

		if (string.IsNullOrEmpty(encoded)) {
			return Default;
		}
		if (encoded.StartsWith("{")) {
			return (NeuralNetworkSettings)JsonUtility.FromJson(encoded, typeof(NeuralNetworkSettings));
		}
		return DecodeV1(encoded);
	}

	private static NeuralNetworkSettings DecodeV1(string encoded) {
		var numsAsStrings = encoded.Split('#');

		var nodesPIL = numsAsStrings.Select(delegate(string arg) {
			return int.Parse(arg);	
		}).ToArray();

		var settings = new NeuralNetworkSettings(nodesPIL);

		return settings;
	}

	#endregion
}
