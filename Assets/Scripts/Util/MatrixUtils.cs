using System;
using UnityEngine;

public static class MatrixUtils {

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

    public static float[][] MatrixProduct(float[][] lhs, float[][] rhs, float[][] result = null) {

        int aRows = lhs.Length; 
		int aCols = lhs[0].Length;
		int bRows = rhs.Length; 
		int bCols = rhs[0].Length;

		if (aCols != bRows)
			throw new System.ArgumentException("Non-conformable matrices in MatrixProduct");
		
        if (result == null)
		    result = CreateMatrix2D(aRows, bCols);

		for (int i = 0; i < aRows; ++i) // each row of A
			for (int j = 0; j < bCols; ++j) // each col of B
				for (int k = 0; k < aCols; ++k)
					result[i][j] += lhs[i][k] * rhs[k][j];
		
		return result;
    }

    public static float[] MatrixProduct(float[][] matrixA, float[] vectorB, float[] result = null) {
		
		int aRows = matrixA.Length; 
		int aCols = matrixA[0].Length;
		int bRows = vectorB.Length;

		if (aCols != bRows)
			throw new System.ArgumentException("Non-conformable matrices in MatrixProduct");

        if (result == null)
		    result = new float[aRows];

		for (int i = 0; i < aRows; ++i) // each row of A
			for (int k = 0; k < aCols; ++k)
				result[i] += matrixA[i][k] * vectorB[k];
		
		return result;
	}

    /// <summary>
    /// Multiplies the given vector with the transpose of the specified matrix.
    /// </summary>
    public static float[] MatrixProductTranspose(float[][] matrixA, float[] vectorB, float[] result = null) {
		
		int aRows = matrixA[0].Length; 
		int aCols = matrixA.Length;
		int bRows = vectorB.Length;

		if (aCols != bRows)
			throw new System.ArgumentException("Non-conformable matrices in MatrixProduct");

        if (result == null)
		    result = new float[aRows];

		for (int i = 0; i < aRows; ++i) // each row of A
			for (int k = 0; k < aCols; ++k)
				result[i] += matrixA[k][i] * vectorB[k];
		
		return result;
	}

    public static bool MatricesEqual(float[][] matrix1, float[][] matrix2) {

		if (matrix1.Length != matrix2.Length || matrix1[0].Length != matrix2[0].Length) return false;

		int rows = matrix1.Length;
		int cols = matrix1[0].Length;

		for ( int i = 0; i < rows; i++ ) {
			for ( int j = 0; j < cols; j++ ) {
				if (matrix1[i][j] != matrix2[i][j]) {
					Debug.Log(string.Format("mat1[{0},{1}] = {2}, mat2[{0},{1}] = {3}", i, j, matrix1[i][j], matrix2[i][j]));
					//print("MAtrix1: " + matrix1[i][j] + " Matrix2: " + matrix2[i][j]);
					return false;	
				}
			}
		}

		return true;
	}

    public static bool MatricesEqual(float[][][] mat1, float[][][] mat2) {

        bool result = true;
        for (int i = 0; i < mat1.Length; i++) {
            bool equal = MatrixUtils.MatricesEqual(mat1[i], mat2[i]);
            result &= equal;
            Console.WriteLine(string.Format("Matrices at index {0} equal? - {1}", i, equal));
        }
        return result;
    }
}