using System;
using System.Text;

public class FeedForwardNetwork: IChromosomeEncodable {

    private struct Constants {
        public static float MIN_WEIGHT = -3.0f;
        public static float MAX_WEIGHT = 3.0f;
    }

    public int NumberOfLayers { get { return layerSizes.Length; } }
    public int NumberOfInputs { get { return layerSizes[0]; } }
    public int NumberOfOutputs { get { return layerSizes[layerSizes.Length - 1]; } }

    public float[] Inputs { get; set; }

    private int[] layerSizes;

    private float[][][] weights;

    // Optimization
    private float[][] tempResults;
    private StringBuilder builder;


    public FeedForwardNetwork(int inputCount, int outputCount, NeuralNetworkSettings settings, string encoded = "") {

        // Setup layerSizes
        int layerCount = settings.NumberOfIntermediateLayers + 2;
        this.layerSizes = new int[layerCount];
        this.layerSizes[0] = inputCount;
        this.layerSizes[layerCount - 1] = outputCount;
        for (int i = 1; i < layerCount - 1; i++) {
            layerSizes[i] = settings.NodesPerIntermediateLayer[i - 1];
        }

        // Create weights arrays

        if (string.IsNullOrEmpty(encoded)) {
            // Setup Random weights
            this.weights = new float[layerCount - 1][][];
            for (int i = 0; i < weights.Length - 1; i++) {
                this.weights[i] = MatrixUtils.CreateRandomMatrix2D(layerSizes[i], layerSizes[i + 1], 
                                                                   Constants.MIN_WEIGHT, Constants.MAX_WEIGHT);
            }
        } else {
            // Decode Weights
            this.weights = WeightsFromBinaryString(encoded);
        }

        this.Inputs = new float[NumberOfInputs];

        // Initialize temporary result vectors
        this.tempResults = new float[weights.Length][];
        for (int i = 0; i < weights.Length; i++) {
            int cols = weights[i][0].Length;
            this.tempResults[i] = new float[cols];
        }
    }

    public float[] CalculateOutputs() {

        float[] result = Inputs;

        for (int i = 0; i < weights.Length; i++) {

            float[][] layerWeights = weights[i];
            float[] tempResultVec = tempResults[i];

            result = MatrixUtils.MatrixProductTranspose(layerWeights, result, tempResultVec);
            ApplySigmoid(result);
        }

        return result;
    }

    public string ToBinaryString() {
        
        if (builder == null)
			builder = new StringBuilder();
		else
			builder.Length = 0;

		if (NumberOfOutputs == 0) return "";

		for (int i = 0; i < weights.Length; i++) {

			ConversionUtils.MatrixToString(weights[i], builder);
		}

		return builder.ToString();
    }

    public static float Sigmoid(float x) {
		return (float)(1 / (1 + Math.Exp(-x)));
	}

    public static void ApplySigmoid(float[] vector) {
        for (int i = 0; i < vector.Length; i++) {
            vector[i] = Sigmoid(vector[i]);
        }
    }

    private float[][][] WeightsFromBinaryString(string encoded) {

        float[][][] matrices = new float[NumberOfLayers - 1][][];
		int strIndex = 0;
		// split the cromosome into the required sizes and turn the substrings into weight matrices.
		for (int i = 0; i < NumberOfLayers - 1; i++) {
			int rows = layerSizes[i];
			int cols = layerSizes[i+1];
			int substrLength = rows * cols * 32;

			matrices[i] = ConversionUtils.BinaryStringToMatrix(rows, cols, encoded, strIndex); 
            // Non Optimized equivalent calls
            // string substr = chromosome.Substring(strIndex, substrLength);
			//matrices[i] = MatrixFromString(rows, cols, substr); 

			strIndex += substrLength;
		}

		return matrices;
    }

    public string ToChromosomeString() {
        return ToBinaryString();
    }
}