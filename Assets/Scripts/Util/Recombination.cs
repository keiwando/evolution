using System;

public interface IRecombinable<T> {

}

public static class Recombination<T> where T: IRecombinable<T> {

    public enum Mode {
        OnePointCrossover,
        MultiPointCrossover,
        UniformCrossover
    }

    public static T[] Recombine(T lhs, T rhs, Mode mode) {

        switch (mode) {
            case Mode.OnePointCrossover: return RecombineOnePoint(lhs, rhs);
            default: return RecombineOnePoint(lhs, rhs);
        }
    }

    private static T[] RecombineOnePoint(T lhs, T rhs) {
        
    }
}