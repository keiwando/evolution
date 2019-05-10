using System;

public class FeedForwardNetwork {

    private struct Constants {
        static float MIN_WEIGHT = -3.0f;
        static float MAX_WEIGHT = 3.0f;
    }

    public int NumberOfLayers { get { return layerSizes.Length; } }
    public int NumberOfInputs { get { return layerSizes[0]; } }
    public int NumberOfOutputs { get { return layerSizes[layerSizes.Length - 1]; } }

    private int[] layerSizes;

    private float[][][] weights;

    private float[][][] tempCalculationMatrices;


    public FeedForwardNetwork(int inputCount, int outputCount, NeuralNetworkSettings settings, string encoding = "") {

        // Setup layerSizes
        int layerCount = settings.NumberOfIntermediateLayers + 2;
        this.layerSizes = new int[layerCount];
        this.layerSizes[0] = inputCount;
        this.layerSizes[layerCount - 1] = outputCount;
        for (int i = 1; i < layerCount - 1; i++) {
            layerSizes[i] = settings.NodesPerIntermediateLayer[i - 1];
        }

        // Create weights arrays
        weights = new float[layerCount - 1][][];
    }
}