using System;
using System.Collections.Generic;

namespace Keiwando.Evolution {

    [Serializable]
    public struct ChromosomeData: ISelectable<ChromosomeData> {

        public readonly string Chromosome;
        public readonly CreatureStats Stats;

        public ChromosomeData(string chromosome, CreatureStats stats) {
            this.Chromosome = chromosome;
            this.Stats = stats;
        } 

        #region Comparers

        public class AscendingComparer: IComparer<ChromosomeData> {
            public int Compare(ChromosomeData lhs, ChromosomeData rhs) {
                return lhs.Stats.fitness.CompareTo(rhs.Stats.fitness);
            }
        }

        public class DescendingComparer: IComparer<ChromosomeData> {
            public int Compare(ChromosomeData lhs, ChromosomeData rhs) {
                return rhs.Stats.fitness.CompareTo(lhs.Stats.fitness);
            }
        }

        public IComparer<ChromosomeData> GetDescendingComparer() {
            return new DescendingComparer();
        }

        public IComparer<ChromosomeData> GetAscendingComparer() {
            return new AscendingComparer();
        }

        #endregion
    }
}