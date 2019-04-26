using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // REMOVE WHEN NOT TESTING!

/** 
 * The Brain contains a neural network that takes the following inputs and produces
 * one output for every muscle that determines whether the muscle should contract or not.
 * 
*/
abstract public class Brain : MonoBehaviour {

	public Muscle[] muscles;

	/** The Creature that this brain belongs to. */
	public Creature creature;

	public NeuralNetworkSettings networkSettings;

	private bool isActive;

	//protected int NUMBER_OF_LAYERS = 3;

	protected int NUMBER_OF_LAYERS { get { return networkSettings.NumberOfIntermediateLayers + 2; } }
	abstract public int NUMBER_OF_INPUTS { get; }
	protected int NUMBER_OF_OUTPUTS;    // will be determined by the muscle size

	private float MINWEIGHT = -3.0f;
	private float MAXWEIGHT = 3.0f;

	protected int[] layerSizes;
	//abstract protected int[] IntermediateLayerSizes { get; }
	protected int[] IntermediateLayerSizes {
		get {
			return networkSettings.NodesPerIntermediateLayer;
		}
	}

	protected float[][][] weightMatrices;

	protected float[][] inputs; // a 1D array would be enough ( jagged needed for multiplication) 
	protected float[][] outputs;

	private float[][][] tempCalculationMatrices;

	/// <summary>
	/// The time that this creature has to simulate. Factors into the fitness calculation.
	/// </summary>
	public float SimulationTime;

	/** A value between 0 and 1 that determines how good the creature is at solving the task. 0 = bad. 1 = perfect. */
	public float fitness;

	private StringBuilder debugBuilder;

	private StringBuilder builder = new StringBuilder();
	private byte[] byteStore = new byte[8];

	private static string[] byte2StringCache;

	private void Awake() {

		//StringFromFloatTest();
		//FloatFromBinaryStringTest();

		if (Brain.byte2StringCache != null) return;

		Brain.byte2StringCache = new string[256];
		for (int i = 0; i < 256; i++)
			Brain.byte2StringCache[i] = Convert.ToString(i, 2).PadLeft(8, '0');
	}

	virtual public void Update() {

		/*if (isActive) {
			outputs = CalcOutputs();
		}*/
	}

	virtual public void FixedUpdate() {

		if (isActive) {

			outputs = CalcOutputs();
			ApplyOutputs(outputs);
		}
	}

	/** 
	 * Loads a current set of inputs and calculates the outputs. 
	 * Returns: A ( 1 x NUMBER_OF_OUTPUTS ) matrix.
	*/
	float[][] CalcOutputs() {

		UpdateInputs();
		float[][] S = inputs;

		for (int i = 0; i < weightMatrices.Length; i++) {

			float[][] weightMatrix = weightMatrices[i];
			float[][] resultMatrix = tempCalculationMatrices[i];

			S = MatrixProduct(S, weightMatrix, resultMatrix);
			//S = MatrixProduct(S, weightMatrix);
			ApplySigmoid(S);
		}

		return S;

	}

	/** 
	 * Takes the outputs in the specified array and applies them to the 
	 * list of muscles. Calls the ApplyOutputToMuscle function for every output.
	*/
	private void ApplyOutputs(float[][] outputs) {

		for (int i = 0; i < outputs[0].Length; i++) {
			float output = float.IsNaN(outputs[0][i]) ? 0 : outputs[0][i];
			ApplyOutputToMuscle(output, muscles[i]);
		}
	}

	/** Interprets the output and calls the respective function on the muscle. */
	protected virtual void ApplyOutputToMuscle(float output, Muscle muscle) {
		//print(output);
		// shift the output of the sigmoid function to receive a value between -0.5 and 0.5
		// multiply by two to get a value between -1 and 1
		float percent = 2 * output - 1f;

		if (percent < 0)
			muscle.muscleAction = Muscle.MuscleAction.CONTRACT;
		else
			muscle.muscleAction = Muscle.MuscleAction.EXPAND;

		muscle.SetContractionForce(Math.Abs(percent));
	}

	public abstract void EvaluateFitness();

	/** Turns the weight of the neural network to a string. */
	//	public string ToChromosomeString() {
	//
	//		string chromosome = "";
	//
	//		if (creature.muscles.Count == 0) return chromosome;
	//
	//		for(int i = 0; i < weightMatrices.Length; i++) {
	//			chromosome += MatrixToString(weightMatrices[i]);
	//		}
	//
	//		return chromosome;
	//	}

	// Optimized
	public string ToChromosomeString() {

		if (builder == null)
			builder = new StringBuilder();
		else
			builder.Length = 0;

		if (creature.muscles.Count == 0) return "";

		for (int i = 0; i < weightMatrices.Length; i++) {

			MatrixToString(weightMatrices[i], builder);
		}

		return builder.ToString();
	}

	public void SetupWeightsFromChromosome(string chromosome) {
		//weightMatrices = WeightsFromChromosome(chromosome);
		weightMatrices = WeightsFromChromosomeOpt(chromosome);
		//var optWeightMatrices = WeightsFromChromosomeOpt(chromosome);

		//CompareWeightMatrices(weightMatrices, optWeightMatrices);
	}

	private void CompareWeightMatrices(float[][][] mat1, float[][][] mat2) {

		for (int i = 0; i < mat1.Length; i++) {

			print(string.Format("Matrices at index {0} equal? - {1}", i, MatricesEqual(mat1[i], mat2[i])));
		}
	}

	public float[][][] WeightsFromChromosome(string chromosome) {

		float[][][] matrices = new float[NUMBER_OF_LAYERS - 1][][];
		int strIndex = 0;
		// split the cromosome into the required sizes and turn the substrings into weight matrices.
		for (int i = 0; i < NUMBER_OF_LAYERS - 1; i++) {
			int rows = layerSizes[i];
			int cols = layerSizes[i+1];
			//print("rows: " + rows + " cols + " + cols);
			//print("chromosome length: " + chromosome.Length);
			int substrLength = rows * cols * 32;
			string substr = chromosome.Substring(strIndex, substrLength);

			matrices[i] = MatrixFromString(rows, cols, substr); 
			//matrices[i] = MatrixFromString(rows, cols, chromosome, strIndex); 

			strIndex += substrLength;
		}

		return matrices;
	}

	/// <summary>
	/// Optimized!
	/// Takes a chromosome string that was generated from the @ApplyOutputToMuscle function.
	/// </summary>
	public float[][][] WeightsFromChromosomeOpt(string chromosome) {

		float[][][] matrices = new float[NUMBER_OF_LAYERS - 1][][];
		int strIndex = 0;
		// split the cromosome into the required sizes and turn the substrings into weight matrices.
		for (int i = 0; i < NUMBER_OF_LAYERS - 1; i++) {
			int rows = layerSizes[i];
			int cols = layerSizes[i+1];
			//print("rows: " + rows + " cols + " + cols);
			//print("chromosome length: " + chromosome.Length);
			int substrLength = rows * cols * 32;
			//string substr = chromosome.Substring(strIndex, rows * cols * 32);

			//matrices[i] = MatrixFromString(rows, cols, substr); 
			matrices[i] = MatrixFromString(rows, cols, chromosome, strIndex); 

			strIndex += substrLength;
		}

		return matrices;
	}

	private string MatrixToString(float[][] matrix) {

		string result = "";
		for (int i = 0; i < matrix.Length; i++) {
			for (int j = 0; j < matrix[0].Length; j++) {
				// convert float into binary 32 bit string
				result += StringFromFloat(matrix[i][j]);
				//result += matrix[i][j].ToString();
			}
		}

		return result;
	}

	private void MatrixToString(float[][] matrix, StringBuilder builder) {
	
		for (int i = 0; i < matrix.Length; i++) {
			for (int j = 0; j < matrix[0].Length; j++) {
				// convert float into binary 32 bit string
				//result += StringFromFloat(matrix[i][j]);
				FloatToString(matrix[i][j], builder);
				//result += matrix[i][j].ToString();
			}
		}
	}

	private float[][] MatrixFromString(int rows, int cols, string str) {

		string[] parts = WholeChunks(str, 32);
		float[][] matrix = MatrixCreate(rows, cols);

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				matrix[i][j] = FloatFromBinaryString(parts[i * cols + j]);
				//matrix[i][j] = FloatFromBinaryString(parts[i * cols + j], 0, 32);
				//int substringStart = (i * cols + j) * 32 + subStart;
				//matrix[i][j] = FloatFromBinaryString(str, substringStart, 32);
			}
		}

		return matrix;
	}

	// Optimized
	private float[][] MatrixFromString(int rows, int cols, string str, int subStart) {

		//string[] parts = WholeChunks(str, 32);
		float[][] matrix = MatrixCreate(rows, cols);

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				//matrix[i][j] = FloatFromBinaryString(parts[i * cols + j]);
				//matrix[i][j] = FloatFromBinaryString(parts[i * cols + j], 0, 32);
				int substringStart = (i * cols + j) * 32 + subStart;
				matrix[i][j] = FloatFromBinaryStringOpt(str, substringStart, 32);
			}
		}

		return matrix;
	} 

	/** Takes a string of 32 bit and converts it to a float. */
	private float FloatFromBinaryString(string str) {

		int numOfBytes = str.Length / 8;
		byte[] bytes = new byte[numOfBytes];

		for(int i = 0; i < numOfBytes; ++i)
		{
			bytes[i] = Convert.ToByte(str.Substring(8 * i, 8), 2);
		}

		return BitConverter.ToSingle(bytes, 0); //Convert.ToSingle(bytes);
	}

	// Optimized
	private float FloatFromBinaryStringOpt(String str, int start, int length) {

		int numOfBytes = length / 8;

		//byte[] bytes = new byte[numOfBytes];
		var bytes = byteStore;

		for (int i = 0; i < numOfBytes; ++i) {

			byte result = 0;

			int byteStart = start + i * 8;
			int byteEnd = byteStart + 7;

			for (int c = byteEnd; c >= byteStart; c--) {

				result += (str[c] == '0') ? (byte)0 : (byte)(Pow2OptByte(byteEnd - c));
			}

			bytes[i] = result;
		}

		return BitConverter.ToSingle(bytes, 0);
	}

	private byte Pow2OptByte(int exp) {
	
		switch (exp) {
		case 0:
			return 1;
		case 1:
			return 2;
		case 2:
			return 4;
		case 3:
			return 8;
		case 4:
			return 16;
		case 5:
			return 32;
		case 6:
			return 64;
		case 7:
			return 128;
		default:
			throw new Exception("Optimization not implemented for given exponent " + exp);
		}
	}

	private void FloatFromBinaryStringTest() {

		int numberOfTests = 10;

		for (int i = 0; i < numberOfTests; i++) {

			float randFloat = UnityEngine.Random.Range(-5f, 5f);
			string floatStr = StringFromFloat(randFloat);

			float floatOld = FloatFromBinaryString(floatStr);
			float floatOpt = FloatFromBinaryStringOpt(floatStr, 0, floatStr.Length);

			print(string.Format("Equal? - {3}\nfloat = {0}, \nfloatOld =\t{1}, \nfloatOpt =\t{2}", randFloat, floatOld, floatOpt, floatOld.Equals(floatOpt)));
		}
	}

	private void StringFromFloatTest() {

		int numberOfTests = 10;

		for (int i = 0; i < numberOfTests; i++) {

			float randFloat = UnityEngine.Random.Range(-5f, 5f);

			var strOld = StringFromFloat(randFloat);
			var strOpt = FloatToString(randFloat, new StringBuilder()).ToString();

			print("Equal? - " + (strOld.Equals(strOpt)));
			print(string.Format("float = {0}, \nstrOld =\t{1}, \nstrOpt =\t{2}", randFloat, strOld, strOpt));
		}
	}

	private string StringFromFloat(float number) {
		string result = "";

		foreach(byte b in BitConverter.GetBytes(number))
			result += Convert.ToString(b, 2).PadLeft(8, '0');
		
		return result;
	}

	private StringBuilder FloatToString(float number, StringBuilder builder) {

		var bytes = BitConverter.GetBytes(number);

		for (int i = 0; i < bytes.Length; i++) {
			
			ByteToString(bytes[i], builder);
		}

		return builder;
	}

	//private StringBuilder ByteToString(byte b, StringBuilder builder) {

	//	for (int i = 0; i < 8; i++)
	//		byteStore[i] = 0;


	//	int index = 7;
	//	while (b != 0 && index >= 0) {
	//		byteStore[index] = (byte)(b % 2);
	//		b /= 2;
	//		index--;
	//	}

	//	for (int i = 0; i < 8; i++) {
	//		builder.Append(byteStore[i] == 1 ? '1' : '0');
	//	}

	//	return builder;
	//} 

	private StringBuilder ByteToString(byte b, StringBuilder builder) {

		builder.Append(Brain.byte2StringCache[b]);

		return builder;
	} 

	static string[] WholeChunks(string str, int chunkSize) {

		string[] result = new string[str.Length / chunkSize];

		for (int i = 0, index = 0; i < str.Length; i += chunkSize, index++) {
			result[index] = str.Substring(i, chunkSize);
		}

		return result;
	}

//	protected void TestConversion() {
//		float number = RandomFloat();
//		//string NumberAsString = StringFromFloat(number);
//		string NumberAsString = FloatToString(number, new StringBuilder()).ToString();
//
//		//float result = FloatFromBinaryString(NumberAsString, 0, NumberAsString.Length);
//		float result = FloatFromBinaryString(NumberAsString);
//		print("Number: " + number + "  binary String: " + NumberAsString + "  Result: " + result);
//
//	}
//
//	protected void TestMatrixConversion() {
//
//		float[][] testMatrix = RandomMatrixCreate(10, 13);
//		string chromosome = MatrixToString(testMatrix);
//		float[][] chromosomeMatrix = MatrixFromString(10, 13, chromosome, 0);
//		//float[][] chromosomeMatrix = MatrixFromString(10, 13, chromosome, 0);
//		var equal = MatricesEqual(testMatrix, chromosomeMatrix);
//		print("Conversion Test passed?: " + equal);
//	}

	private bool MatricesEqual(float[][] matrix1, float[][] matrix2) {

		if (matrix1.Length != matrix2.Length || matrix1[0].Length != matrix2[0].Length) return false;

		int rows = matrix1.Length;
		int cols = matrix1[0].Length;

		for ( int i = 0; i < rows; i++ ) {
			for ( int j = 0; j < cols; j++ ) {
				if (matrix1[i][j] != matrix2[i][j]) {
					print(string.Format("mat1[{0},{1}] = {2}, mat2[{0},{1}] = {3}", i, j, matrix1[i][j], matrix2[i][j]));
					//print("MAtrix1: " + matrix1[i][j] + " Matrix2: " + matrix2[i][j]);
					return false;	
				}
			}
		}

		return true;

	}

	/** If chromosome is an empty string the weights will be setup randomly. */
	public void SetupNeuralNet(string chromosome = "") {

		if (muscles.Length == 0) return;

		NUMBER_OF_OUTPUTS = muscles.Length;
		// setup the layer sizes
		layerSizes = new int[NUMBER_OF_LAYERS];
		layerSizes[0] = NUMBER_OF_INPUTS;
		layerSizes[NUMBER_OF_LAYERS -1] = NUMBER_OF_OUTPUTS;

		for (int i = 1; i < NUMBER_OF_LAYERS - 1; i++) {
			layerSizes[i] = IntermediateLayerSizes[i-1];
		} 

		// setup weight matrices
		if (chromosome == "") {

			weightMatrices = new float[NUMBER_OF_LAYERS - 1][][]; //new float[NUMBER_OF_LAYERS - 1];
			for (int i = 0; i < NUMBER_OF_LAYERS - 1; i++) {
				//print("layersize: index: " + i + " size: " + layerSizes[i+1]);
				weightMatrices[i] = RandomMatrixCreate(layerSizes[i], layerSizes[i+1]);
			}
		} else {
			SetupWeightsFromChromosome(chromosome);
		}

		// initialize input matrix
		inputs = MatrixCreate(1, NUMBER_OF_INPUTS);

		tempCalculationMatrices = new float[weightMatrices.Length][][];

		for (int i = 0; i < weightMatrices.Length; i++) {

			int cols = weightMatrices[i][0].Length;

			tempCalculationMatrices[i] = MatrixCreate(1, cols);
		}

		isActive = true;
	}


	/** Load the Input values into the inputs Matrix. */
	abstract protected void UpdateInputs();

	public void setMuscles(List<Muscle> muscles) {
		this.muscles = muscles.ToArray();

		SetupNeuralNet("");
	}

	/** Applies the sigmoid function to every entry of the matrix. */
	public void ApplySigmoid(float[][] matrix) {

		for (int i = 0; i < matrix.Length; i++) {
			for (int j = 0; j < matrix[0].Length; j++) {

				matrix[i][j] = Sigmoid(matrix[i][j]);
			}
		}
	}

	float Sigmoid(float x) {
		return 1 / (1 + Mathf.Exp(-x));
	} 

	// Matrix functions
	public float[] MatrixProduct(float[][] matrixA, float[] vectorB) {
		
		int aRows = matrixA.Length; 
		int aCols = matrixA[0].Length;
		int bRows = vectorB.Length;

		if (aCols!=bRows)
			throw new  UnityException("Non-conformable matrices in MatrixProduct");

		float[] result = new float[aRows];
		for (int i = 0; i < aRows; ++i) // each row of A
			for (int k = 0; k < aCols; ++k)
				result[i] += matrixA[i][k] * vectorB[k];
		
		return result;
	}

	public float[][] MatrixProduct(float[][] matrixA, float[][] matrixB) {
		int aRows = matrixA.Length; 
		int aCols = matrixA[0].Length;
		int bRows = matrixB.Length; 
		int bCols = matrixB[0].Length;

		if (aCols!=bRows)
			throw new UnityException("Non-conformable matrices in MatrixProduct");
		
		float[][] result = MatrixCreate(aRows, bCols);

		for (int i = 0; i < aRows; ++i) // each row of A
			for (int j = 0; j < bCols; ++j) // each col of B
				for (int k = 0; k < aCols; ++k)
					result[i][j] += matrixA[i][k] * matrixB[k][j];
		
		return result;
	}

	public float[][] MatrixProduct(float[][] matrixA, float[][] matrixB, float[][] result) {
		
		int aRows = matrixA.Length; 
		int aCols = matrixA[0].Length;
		int bRows = matrixB.Length; 
		int bCols = matrixB[0].Length;

		if (aCols!=bRows)
			throw new UnityException("Non-conformable matrices in MatrixProduct");

//		if (result.Length != aRows || result[0].Length != bCols) {
//			print(string.Format("correct dims = {0} x {1} --- actual dims = {2} x {3}", aRows, bCols, result.Length, result[0].Length));
//			return result;
//		}

		//float[][] result = MatrixCreate(aRows, bCols);

		for (int i = 0; i < aRows; ++i) { // each row of A
			for (int j = 0; j < bCols; ++j) { // each col of B

				float sum = 0;
				for (int k = 0; k < aCols; ++k)
					sum += matrixA[i][k] * matrixB[k][j];

				result[i][j] = sum;
			}
		} 

		return result;
	}

	public float[][] MatrixCreate(int rows, int cols) {
		// creates a matrix initialized to all 0.0s  
		float[][] result = new float[rows][];

		for (int i = 0; i < rows; ++i)
			result[i] = new float[cols];
		// auto init to 0.0  
		return result;
	}

	public float[][] VectorTranspose(float[] array) {
		
		float[][] transpose = new float[1][];
		transpose[0] = array;

		return transpose;
	} 

	public float[][] RandomMatrixCreate(int rows, int cols) {

		float[][] result = MatrixCreate(rows, cols);

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				
				result[i][j] = RandomFloat();
			}
		}

		return result;
	}

	private float RandomFloat() {
		return UnityEngine.Random.Range(MINWEIGHT, MAXWEIGHT);
	}

	private int MatrixSize(float[][] matrix) {
		return matrix.Length * matrix[0].Length;
	}

	private int MatrixSize(float[][][] matrix) {
		int sum = 0;
		foreach(float[][] mat in matrix) {
			sum += MatrixSize(mat);
		}

		return sum;
	}


	protected virtual void DEBUG_PRINT_INPUTS() {

		debugBuilder = new StringBuilder();

		debugBuilder.AppendLine("Distance from ground: " + inputs[0][0]);
		debugBuilder.AppendLine("Horiz vel: " + inputs[0][1]);
		debugBuilder.AppendLine("Vert vel: " + inputs[0][2]);
		debugBuilder.AppendLine("rot vel: " + inputs[0][3]);
		debugBuilder.AppendLine("points touchnig gr: " + inputs[0][4]);
		debugBuilder.AppendLine("rotation: " + inputs[0][5] + "\n");

		//print(sBuilder.ToString());
	}

	protected virtual void DEBUG_PRINT_OUTPUTS() {

		//var sBuilder = new StringBuilder();

		for (int i = 0; i < creature.muscles.Count; i++) {
			debugBuilder.AppendLine("Muscle " + (i+1) + " : " + outputs[0][i]);
		}

		print(debugBuilder.ToString());
	}
}
