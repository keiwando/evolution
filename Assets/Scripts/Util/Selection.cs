using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

    public interface ISelectable<T> {
        
        float GetFitness();
        IComparer<T> GetDescendingComparer();
        IComparer<T> GetAscendingComparer();
    }

    public enum SelectionAlgorithm {
        Uniform = 0,
        FitnessProportional = 1,
        TournamentSelection = 2,
        RankProportional = 3
    }

    public class Selection<T> where T: ISelectable<T> {

        private readonly SelectionAlgorithm mode;
        
        /// <summary>
        /// A sorted list of solutions to select from in ascending order.
        /// </summary>
        private List<T> solutions;

        private RandomPicker<T> random;

        public Selection(SelectionAlgorithm mode, List<T> solutions) {
            this.mode = mode;
            this.solutions = solutions;

            if (solutions.Count > 0) {
                this.solutions.Sort(solutions[0].GetAscendingComparer());
            }

            SetupPickingWeights(mode, solutions);
        }

        /// <summary>
        /// Returns the best n solutions. 
        /// </summary>
        public List<T> SelectBest(int n) {

            var best = new List<T>();
            for (int i = solutions.Count - 1; i > solutions.Count - 1 - n; i--) {
                best.Add(solutions[i]);
            }
            return best;
        }

        public T Select() {
            switch (mode) {

            case SelectionAlgorithm.Uniform:
            case SelectionAlgorithm.RankProportional:
            case SelectionAlgorithm.FitnessProportional:
            return random.Next();

            case SelectionAlgorithm.TournamentSelection:
            T sel1 = random.Next();
            T sel2 = random.Next();
            T sel3 = random.Next();
            float fitness1 = sel1.GetFitness();
            float fitness2 = sel2.GetFitness();
            float fitness3 = sel3.GetFitness();
            if (fitness1 > fitness2) {
                return fitness1 > fitness3 ? sel1 : sel3;
            } else {
                return fitness2 > fitness3 ? sel2 : sel3;
            }

            default: throw new System.ArgumentException("Invalid selection mode.");
            }
        }


        private void SetupPickingWeights(SelectionAlgorithm mode, List<T> solutions) {

            random = new RandomPicker<T>();

            switch (mode) {

            case SelectionAlgorithm.FitnessProportional: 
            
            foreach (var solution in solutions) {
                random.Add(solution, solution.GetFitness());
            }
            break;
            
            case SelectionAlgorithm.Uniform:
            case SelectionAlgorithm.TournamentSelection:
            foreach (var solution in solutions) {
                random.Add(solution, 1);
            }
            break;

            case SelectionAlgorithm.RankProportional:
            for (int i = 0; i < solutions.Count; i++) {
                random.Add(solutions[i], i);
            }
            break;
            
            default: break;
            }
        }
    }
}

