using System;
using System.Collections.Generic;

namespace Keiwando.Evolution {

    public struct LazyChromosomeData: ISelectable<LazyChromosomeData> {

        public string Chromosome {
            get {
                if (cachedChromosome == null) {
                    cachedChromosome = this.encodable.ToChromosomeString();
                }
                return cachedChromosome;
            }
        }
        private string cachedChromosome;

        private readonly IChromosomeEncodable encodable;
        public readonly CreatureStats Stats;

        public LazyChromosomeData(IChromosomeEncodable encodable, CreatureStats stats) {
            this.encodable = encodable;
            this.Stats = stats;
            this.cachedChromosome = null;
        } 

        #region Comparers

        public class AscendingComparer: IComparer<LazyChromosomeData> {
            public int Compare(LazyChromosomeData lhs, LazyChromosomeData rhs) {
                return lhs.Stats.fitness.CompareTo(rhs.Stats.fitness);
            }
        }

        public class DescendingComparer: IComparer<LazyChromosomeData> {
            public int Compare(LazyChromosomeData lhs, LazyChromosomeData rhs) {
                return rhs.Stats.fitness.CompareTo(lhs.Stats.fitness);
            }
        }

        public IComparer<LazyChromosomeData> GetDescendingComparer() {
            return new DescendingComparer();
        }

        public IComparer<LazyChromosomeData> GetAscendingComparer() {
            return new AscendingComparer();
        }

        #endregion
    }
}