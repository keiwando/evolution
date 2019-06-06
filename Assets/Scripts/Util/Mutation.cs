using System;
using System.Text;
using UnityEngine;

namespace Keiwando.Evolution {

    public interface IMutatable<T> {
        T this[int index] { get; set; }
        T Flipped(T value);
        int Length { get; }
    }

    public enum MutationAlgorithm {
        /// <summary>Flips a random number of values at consecutive indices.</summary>
        ChunkFlip,
        /// <summary>Flips the value at each index with a random probability.</summary>
        Global,
        /// <summary>Chooses a random start and end index and inverts the order of values inbetween.</summary>
        Inversion
    }

    public static class Mutation {

        // TODO: Add intensity parameter

        public static T Mutate<T, E>(T chromosome, MutationAlgorithm mode) where T: IMutatable<E> {

            switch (mode) {
                case MutationAlgorithm.ChunkFlip: return MutateChunkFlip<T, E>(chromosome);
                case MutationAlgorithm.Global: return MutateGlobal<T, E>(chromosome);
                case MutationAlgorithm.Inversion: return MutateInversion<T, E>(chromosome);
                default: return MutateChunkFlip<T, E>(chromosome);
            }
        }

        private static T MutateChunkFlip<T, E>(T chromosome) where T: IMutatable<E> {

            int start = UnityEngine.Random.Range(0, chromosome.Length - 1);
            int length = Math.Min(Math.Max(0, chromosome.Length - start - 3), UnityEngine.Random.Range(2, 15));

            for (int i = start; i < length; i++) {
                chromosome[i] = chromosome.Flipped(chromosome[i]);
            }

            return chromosome;
        }

        private static T MutateGlobal<T, E>(T chromosome) where T: IMutatable<E> {

            for (int i = 0; i < chromosome.Length; i++) {
                if (UnityEngine.Random.Range(1, 100) > 25) {
                    chromosome[i] = chromosome.Flipped(chromosome[i]);
                }
            }

            return chromosome;
        }

        private static T MutateInversion<T, E>(T chromosome) where T: IMutatable<E> {

            int start = UnityEngine.Random.Range(0, chromosome.Length - 1);
            int end = UnityEngine.Random.Range(start, chromosome.Length - 1);

            for (int i = start; i <= end; i++) {
                chromosome[i] = chromosome.Flipped(chromosome[i]);
            }

            return chromosome;
        }

    }
}
