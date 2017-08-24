using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkSettings {

	public int numberOfIntermediateLayers { get { return nodesPerIntermediateLayer.Length; } }

	public int[] nodesPerIntermediateLayer;
}
