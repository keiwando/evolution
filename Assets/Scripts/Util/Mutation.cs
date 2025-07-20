using System;
using System.Text;
using UnityEngine;

namespace Keiwando.Evolution {

    public delegate T Mutate<T>(T value);

    public class MutatableFloatArray: IMutatable<float> {
        
        public float[] Values { get; private set; }
        public float this[int i] {
            get { return Values[i]; }
            set { Values[i] = value; } 
        }
        public int Length { get; private set; }

        public MutatableFloatArray(float[] values) {
            this.Values = values;
            this.Length = values.Length;
        }
    }

    public class MutatableString: IMutatable<char> {

        public StringBuilder Builder { get; private set; }
        public char this[int i] {
            get { return Builder[i]; }
            set { Builder[i] = value; }
        }
        public int Length { get; private set; }

        public MutatableString(StringBuilder builder) {
            this.Builder = builder;
            this.Length = builder.Length;
        }
    }

    public interface IMutatable<T> {
        T this[int index] { get; set; }
        int Length { get; }
    }

    public enum MutationAlgorithm: byte {

        /// <summary>
        /// Changes a random number of values at consecutive indices.
        /// </summary>
        Chunk = 0,

        /// <summary>
        /// Changes the value at each index with a random probability.
        /// </summary>
        Global = 1,

        /// <summary>
        /// Chooses a random start and end index and inverts the order of values inbetween.
        /// </summary>
        Inversion = 2
    }

    public static class Mutation {

        private static GaussianPRNG random = new GaussianPRNG();

        private static char Mutate(char c) {
            if (c == '0') 
                return '1';
            else if (c == '1')
                return '0';
            else 
                throw new ArgumentException("Invalid input. Only '0' and '1' are supported");
        }

        private static float Mutate(float x) {
            // Nonlocal offset
            var z = random.Next();
            return x + z;
        }

        public static float[] Mutate(float[] chromosome, MutationAlgorithm mode) {
            return Mutation.Mutate<MutatableFloatArray, float>(new MutatableFloatArray(chromosome), mode, Mutate).Values;
        }

        public static StringBuilder Mutate(StringBuilder builder, MutationAlgorithm mode) {
            return Mutation.Mutate<MutatableString, char>(new MutatableString(builder), mode, Mutate).Builder;
        }

        private static T Mutate<T, E>(T chromosome, MutationAlgorithm mode, Mutate<E> mutate) where T: IMutatable<E> {

            switch (mode) {
                case MutationAlgorithm.Chunk: return MutateChunk(chromosome, mutate);
                case MutationAlgorithm.Global: return MutateGlobal(chromosome, mutate);
                case MutationAlgorithm.Inversion: return MutateInversion(chromosome, mutate);
                default: return MutateChunk<T, E>(chromosome, mutate);
            }
        }

        private static T MutateChunk<T, E>(T chromosome, Mutate<E> mutate) where T: IMutatable<E> {

            int start = UnityEngine.Random.Range(0, chromosome.Length - 1);
            int length = Math.Min(Math.Max(0, chromosome.Length - start - 3), UnityEngine.Random.Range(2, 15));

            for (int i = start; i < length; i++) {
                chromosome[i] = mutate(chromosome[i]);
            }

            return chromosome;
        }

        private static T MutateGlobal<T, E>(T chromosome, Mutate<E> mutate) where T: IMutatable<E> {

            for (int i = 0; i < chromosome.Length; i++) {
                if (UnityEngine.Random.Range(1, 100) > 25) {
                    chromosome[i] = mutate(chromosome[i]);
                }
            }

            return chromosome;
        }

        private static T MutateInversion<T, E>(T chromosome, Mutate<E> mutate) where T: IMutatable<E> {

            int start = UnityEngine.Random.Range(0, chromosome.Length - 1);
            int end = UnityEngine.Random.Range(start, chromosome.Length - 1);
            int mid = (start + end) / 2;

            for (int i = start; i <= mid; i++) {
                int swapIndex = end - i + start;
                E temp = chromosome[i];
                chromosome[i] = chromosome[swapIndex];
                chromosome[swapIndex] = temp;
            }

            return chromosome;
        }

    }
}
