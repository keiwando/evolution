using System;
using UnityEngine;

public class MatrixUtils {

    public static float[][] CreateRandomMatrix2D(int rows, int cols, float minVal, float maxVal) {
        float[][] result = CreateMatrix2D(rows, cols);

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				
				result[i][j] = UnityEngine.Random.Range(minVal, maxVal);
			}
		}

		return result;
    }

    /// <summary>
    /// // Creates a matrix initialized to all 0.0s  
    /// </summary>
    public static float[][] CreateMatrix2D(int rows, int cols) {

		float[][] result = new float[rows][];

		for (int i = 0; i < rows; ++i)
			result[i] = new float[cols];

		return result;
    }
}