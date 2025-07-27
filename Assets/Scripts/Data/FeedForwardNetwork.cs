using System;
using System.Text;

public class FeedForwardNetwork: IChromosomeEncodable<string>, IChromosomeEncodable<float[]> {

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

    
    private FeedForwardNetwork(int inputCount, int outputCount, NeuralNetworkSettings settings) {

        // Setup layerSizes
        int layerCount = settings.NumberOfIntermediateLayers + 2;
        this.layerSizes = new int[layerCount];
        this.layerSizes[0] = inputCount;
        this.layerSizes[layerCount - 1] = outputCount;
        for (int i = 1; i < layerCount - 1; i++) {
            layerSizes[i] = settings.NodesPerIntermediateLayer[i - 1];
        }

        this.Inputs = new float[inputCount];
    }

    public FeedForwardNetwork(
        int inputCount, 
        int outputCount, 
        NeuralNetworkSettings settings, 
        float[] weights
    ) : this(inputCount, outputCount, settings) {

        // Create weights arrays
        if (weights == null || weights.Length == 0) {
            SetupRandomWeights();
        } else {
            this.weights = WeightsFromFloatArray(weights);
        }
        
        InitializeTempResults();
    }

    public FeedForwardNetwork(
        int inputCount, 
        int outputCount, 
        NeuralNetworkSettings settings, 
        string encoded = ""
    ) : this(inputCount, outputCount, settings) {

        // Create weights arrays
        if (string.IsNullOrEmpty(encoded)) {
            SetupRandomWeights();            
        } else {
            // Decode Weights
            this.weights = WeightsFromBinaryString(encoded);
        }

        InitializeTempResults();
    }

    private void InitializeTempResults() {
        this.tempResults = new float[weights.Length][];
        for (int i = 0; i < weights.Length; i++) {
            int cols = weights[i][0].Length;
            this.tempResults[i] = new float[cols];
        }
    }

    private void SetupRandomWeights() {
        this.weights = new float[layerSizes.Length - 1][][];
        for (int i = 0; i < weights.Length; i++) {
            this.weights[i] = MatrixUtils.CreateRandomMatrix2D(
                layerSizes[i], layerSizes[i + 1], 
                Constants.MIN_WEIGHT, Constants.MAX_WEIGHT
            );
        }
    }

    public static void PopulateRandomWeights(float[] weights) {
        for (int i = 0; i < weights.Length; i++) {
            weights[i] = UnityEngine.Random.Range(Constants.MIN_WEIGHT, Constants.MAX_WEIGHT);
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

    public float[] ToFloatArray() {

        int weightCount = GetTotalWeightCount();
        float[] allWeights = new float[weightCount];
        int weightIndex = 0;

        for (int i = 0; i < NumberOfLayers - 1; i++) {
            int rows = layerSizes[i];
			int cols = layerSizes[i+1];
            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < cols; col++) {
                    allWeights[weightIndex] = this.weights[i][row][col];       
                    weightIndex++;
                }
            }
        }
        return allWeights;
    }

    private int GetTotalWeightCount() {
        int totalWeightCount = 0;
        for (int i = 0; i < NumberOfLayers - 1; i++) {
            totalWeightCount += this.weights[i].Length * this.weights[i][0].Length;
        }
        return totalWeightCount;
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

    private float[][][] WeightsFromFloatArray(float[] weights) {

        float[][][] matrices = new float[NumberOfLayers - 1][][];
        int weightIndex = 0;

        for (int i = 0; i < NumberOfLayers - 1; i++) {
            int rows = layerSizes[i];
            int cols = layerSizes[i + 1];
            int weightsInMatrix = rows * cols;

            var matrix = new float[rows][];
            for (int row = 0; row < rows; row++) {
                matrix[row] = new float[cols];
                for (int col = 0; col < cols; col++) {
                    matrix[row][col] = weights[weightIndex];
                    weightIndex++;
                }
            }
            matrices[i] = matrix;
        }
        return matrices;
    }

    string IChromosomeEncodable<string>.ToChromosome() {
        return ToBinaryString();
    }

    float[] IChromosomeEncodable<float[]>.ToChromosome() {
        return ToFloatArray();
    }
}