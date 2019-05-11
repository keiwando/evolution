using System;

public static class StringUtils {

    public static string[] WholeChunks(string str, int chunkSize) {

		string[] result = new string[str.Length / chunkSize];

		for (int i = 0, index = 0; i < str.Length; i += chunkSize, index++) {
			result[index] = str.Substring(i, chunkSize);
		}

		return result;
    }
}