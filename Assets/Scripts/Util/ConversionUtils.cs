using System;
using System.Text;

public static class ConversionUtils {

    private static string[] lut = new string[256];
    // Temporary array created once for memory efficiency
    private static byte[] byteStore = new byte[8];

    static ConversionUtils() {
        for (int i = 0; i < 256; i++)
            lut[i] = Convert.ToString(i, 2).PadLeft(8, '0');
    }

    public static StringBuilder ByteToString(byte b, StringBuilder builder) {
        builder.Append(lut[b]);
        return builder;
    }

    public static string ByteToString(byte b) {
        return lut[b];
    }

    public static StringBuilder FloatToString(float number, StringBuilder builder) {
		var bytes = BitConverter.GetBytes(number);

		for (int i = 0; i < bytes.Length; i++) {
			
			ByteToString(bytes[i], builder);
		}

		return builder;
	}

    public static string FloatToString(float number) {
		string result = "";

		foreach (byte b in BitConverter.GetBytes(number))
			result += Convert.ToString(b, 2).PadLeft(8, '0');
		
		return result;
	}

    public static string MatrixToString(float[][] matrix) {

		string result = "";
		for (int i = 0; i < matrix.Length; i++) {
			for (int j = 0; j < matrix[0].Length; j++) {
				// convert float into binary 32 bit string
				result += FloatToString(matrix[i][j]);
			}
		}

		return result;
	}

    public static void MatrixToString(float[][] matrix, StringBuilder builder) {
	
		for (int i = 0; i < matrix.Length; i++) {
			for (int j = 0; j < matrix[0].Length; j++) {
				// convert float into binary 32 bit string
				FloatToString(matrix[i][j], builder);
			}
		}
	}

    public static float[][] BinaryStringToMatrix(int rows, int cols, string encoded) {

        string[] parts = StringUtils.WholeChunks(encoded, 32);
		float[][] matrix = MatrixUtils.CreateMatrix2D(rows, cols);

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				matrix[i][j] = BinaryStringToFloat(parts[i * cols + j]);
			}
		}

		return matrix;
    }

    public static float[][] BinaryStringToMatrix(int rows, int cols, string encoded, int subStart) {

        float[][] matrix = MatrixUtils.CreateMatrix2D(rows, cols);

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				int substringStart = (i * cols + j) * 32 + subStart;
                matrix[i][j] = BinaryStringToFloat(encoded, substringStart, 32);
			}
		}

		return matrix;
    }

    public static float BinaryStringToFloat(string str) {

		int numOfBytes = str.Length / 8;
		byte[] bytes = new byte[numOfBytes];

		for(int i = 0; i < numOfBytes; ++i)
		{
			bytes[i] = Convert.ToByte(str.Substring(8 * i, 8), 2);
		}

		return BitConverter.ToSingle(bytes, 0);
	}

    public static float BinaryStringToFloat(string str, int start, int length) {
        int numOfBytes = length / 8;

		var bytes = byteStore;

		for (int i = 0; i < numOfBytes; ++i) {

			byte result = 0;

			int byteStart = start + i * 8;
			int byteEnd = byteStart + 7;

			for (int c = byteEnd; c >= byteStart; c--) {

				result += (str[c] == '0') ? (byte)0 : (byte)(Pow2Byte(byteEnd - c));
			}

			bytes[i] = result;
		}

		return BitConverter.ToSingle(bytes, 0);
    }

    private static byte Pow2Byte(int exp) {
	
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

    private static class Tests {

        public static void FloatFromBinaryStringTest() {

            int numberOfTests = 10;

            for (int i = 0; i < numberOfTests; i++) {

                float randFloat = UnityEngine.Random.Range(-5f, 5f);
                string floatStr = FloatToString(randFloat);

                float floatOld = BinaryStringToFloat(floatStr);
                float floatOpt = BinaryStringToFloat(floatStr, 0, floatStr.Length);

                Console.WriteLine(string.Format("Equal? - {3}\nfloat = {0}, \nfloatOld =\t{1}, \nfloatOpt =\t{2}", randFloat, floatOld, floatOpt, floatOld.Equals(floatOpt)));
            }
        }

        public static void StringFromFloatTest() {

            int numberOfTests = 10;

            for (int i = 0; i < numberOfTests; i++) {

                float randFloat = UnityEngine.Random.Range(-5f, 5f);

                var strOld = FloatToString(randFloat);
                var strOpt = FloatToString(randFloat, new StringBuilder()).ToString();

                Console.WriteLine("Equal? - " + (strOld.Equals(strOpt)));
                Console.WriteLine(string.Format("float = {0}, \nstrOld =\t{1}, \nstrOpt =\t{2}", randFloat, strOld, strOpt));
            }
        }
    }
}