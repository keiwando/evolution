using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

    public interface ISelectable<T> {
        
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

        private SelectionAlgorithm mode;
        
        /// <summary>
        /// A sorted list of solutions to select from in descending order.
        /// </summary>
        private List<T> solutions;

        private int[] randomPickingWeights;

        public Selection(SelectionAlgorithm mode, List<T> solutions) {
            this.mode = mode;
            this.solutions = solutions;

            if (solutions.Count > 0) {
                this.solutions.Sort(solutions[0].GetDescendingComparer());
            }

            SetupPickingWeights(mode, solutions.Count);
        }

        public T Select() {

            // Select a random weighted index
            int rand = UnityEngine.Random.Range(0, this.randomPickingWeights[0] - 1);
            int index = 0;
            for(int i = this.solutions.Count - 1; i >= 0; i--) {
                if( randomPickingWeights[i] >= rand ) {
                    index = i;
                    break;
                }
            }

            return solutions[index];
        }

        /// <summary>
        /// Returns the best n solutions. 
        /// </summary>
        public List<T> SelectBest(int n) {

            var best = new List<T>();
            for (int i = 0; i < n; i++) {
                best.Add(solutions[i]);
            }
            return best;
        }

        #region Weight setup

        private void SetupPickingWeights(SelectionAlgorithm mode, int count) {

            this.randomPickingWeights = new int[count];

            switch (mode) {
                case SelectionAlgorithm.FitnessProportional: 
                SetupFitnessProportionalWeights(this.randomPickingWeights);
                break;
                default: break;
            }
        }

        private void SetupFitnessProportionalWeights(int[] weights) {
            int value = 1;
            for (int i = 1; i <= weights.Length; i++) {
                weights[weights.Length - i] = value;
                value += i;
            }
        }

        #endregion
    }
}

