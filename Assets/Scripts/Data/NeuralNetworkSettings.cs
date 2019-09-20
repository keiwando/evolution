using UnityEngine;
using System.Linq;
using Keiwando.JSON;

[SerializeField]
public struct NeuralNetworkSettings: IJsonConvertible {

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

	private static class CodingKey {
		public const string NodesPerIntermediateLayer = "NodesPerIntermediateLayer";
	}

	public JObject Encode() {

		JObject json = new JObject();
		json[CodingKey.NodesPerIntermediateLayer] = this.NodesPerIntermediateLayer;
		return json;
	}

	public static NeuralNetworkSettings Decode(JObject json) {

		int[] nodesPerLayer = json[CodingKey.NodesPerIntermediateLayer].ToIntArray();
		return new NeuralNetworkSettings(nodesPerLayer);
	}

	public static NeuralNetworkSettings Decode(string encoded) {

		if (string.IsNullOrEmpty(encoded)) {
			return Default;
		}
		if (encoded.StartsWith("{")) {
			return Decode(JObject.Parse(encoded));
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
