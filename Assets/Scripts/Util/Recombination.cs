using System;

namespace Keiwando.Evolution {

    public interface IRecombinable<T> {

    }

    public enum RecombinationAlgorithm {
        OnePointCrossover,
        MultiPointCrossover,
        UniformCrossover
    }

    public static class Recombination<T> where T: IRecombinable<T> {

        public static T[] Recombine(T lhs, T rhs, RecombinationAlgorithm algorithm) {

            switch (algorithm) {
                case RecombinationAlgorithm.OnePointCrossover: return RecombineOnePoint(lhs, rhs);
                default: return RecombineOnePoint(lhs, rhs);
            }
        }

        private static T[] RecombineOnePoint(T lhs, T rhs) {
            T[] result = new T[2];
            // TODO: Implement
            return result;
        }
    }
}