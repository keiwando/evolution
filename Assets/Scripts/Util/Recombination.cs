using System;

namespace Keiwando.Evolution {

    public enum RecombinationAlgorithm: byte {

        /// <summary>
        /// Cuts the parent chromosomes at the same random index 
        /// and uses one part from each parent.
        /// </summary>
        OnePointCrossover = 0,

        /// <summary>
        /// Cuts the parent chromosomes at multiple random indices 
        /// and alternatingly chooses parts from the parent chromosomes.
        /// </summary>
        MultiPointCrossover = 1,

        /// <summary>
        /// Chooses each bit at random from either parent chromosome.
        /// </summary>
        UniformCrossover = 2
    }

    public static class Recombination<T> {

        public static void Recombine(T[] lhs, T[] rhs, T[][] result, RecombinationAlgorithm algorithm) {

            if (lhs.Length != rhs.Length) {
                throw new ArgumentException("The arrays to be recombined must both be of the same length.");
            }
            if (result.Length != 2) {
                throw new ArgumentException("The result array must have a length of 2.");
            }

            switch (algorithm) {
                case RecombinationAlgorithm.OnePointCrossover: 
                    RecombineOnePoint(lhs, rhs, result); break;
                case RecombinationAlgorithm.MultiPointCrossover:
                    RecombineMultiPoint(lhs, rhs, result); break;
                case RecombinationAlgorithm.UniformCrossover:
                    RecombineUniform(lhs, rhs, result); break;
                default: RecombineOnePoint(lhs, rhs, result); break;
            }
        }

        private static void RecombineOnePoint(T[] lhs, T[] rhs, T[][] result) {

            int length = lhs.Length;
            int splitIndex = UnityEngine.Random.Range(1, length);
            T[] result0 = new T[length];
            T[] result1 = new T[length];

            for (int i = 0; i < splitIndex; i++) {
                result0[i] = lhs[i];
                result1[i] = rhs[i];
            }
            for (int i = splitIndex; i < length; i++) {
                result0[i] = rhs[i];
                result1[i] = lhs[i];
            }

            result[0] = result0;
            result[1] = result1;
        }

        private static void RecombineMultiPoint(T[] lhs, T[] rhs, T[][] result) {
            // Determine the k for the k-point crossover
            // Perform at most a 5-point crossover
            int length = lhs.Length;
            int k = Math.Min(lhs.Length, 5);
            int lengthPerPart = length / k;

            result[0] = new T[length];
            result[1] = new T[length];

            for (int i = 0; i < length; i++) {
                int swapIndex = (i / lengthPerPart) % 2;
                result[swapIndex][i] = lhs[i];
                result[1 - swapIndex][i] = rhs[i];
            }
        }

        private static void RecombineUniform(T[] lhs, T[] rhs, T[][] result) {

            int length = lhs.Length;
            result[0] = new T[length];
            result[1] = new T[length];

            for (int i = 0; i < length; i++) {
                int swapIndex = UnityEngine.Random.Range(0, 2);
                result[swapIndex][i] = lhs[i];
                result[1 - swapIndex][i] = rhs[i];
            }
        }
    }
}